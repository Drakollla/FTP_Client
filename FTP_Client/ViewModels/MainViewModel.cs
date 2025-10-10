using FTP_Client.Enums;
using FTP_Client.Helpers;
using FTP_Client.Models;
using FTP_Client.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FTP_Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public const string LocalRootPath = "Мой компьютер";

        public MainViewModel()
        {
            LocalPanel = new FilePanelViewModel("Локальный диск", PanelType.Local);
            ServerPanel = new FilePanelViewModel("FTP-Сервер", PanelType.Server);

            _fileEditorService = new FileEditorService(_ftpService, _logger);

            LocalPanel.NavigateRequested += NavigateLocalDirectory;
            ServerPanel.NavigateRequested += async (path) => await NavigateServerDirectoryAsync(path);
            
            InitializeCommands();
            LoadInitialDrives();
        }

        #region Services
        
        private readonly ILoggerService _logger = new LoggerService();
        private readonly ILocalFileService _localFileService = new LocalFileService();
        private readonly IFtpService _ftpService = new FtpService();
        private readonly IDialogService _dialogService = new DialogService();
        private readonly IFileEditorService _fileEditorService;
        
        #endregion

        #region Properties And Fields
        private bool _isRenameDialogVisible;
        private string _newItemName;
        private bool _isConnected;

        public FilePanelViewModel LocalPanel { get; }
        public FilePanelViewModel ServerPanel { get; }
        public FtpConnectionSettings FtpConnectionSettings { get; } = new();
        public ObservableCollection<LogMessage> LogMessages => _logger.Messages;

        public bool IsRenameDialogVisible
        {
            get => _isRenameDialogVisible;
            set => SetProperty(ref _isRenameDialogVisible, value);
        }

        public string NewItemName
        {
            get => _newItemName;
            set => SetProperty(ref _newItemName, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }

        #endregion

        #region Commands

        public ICommand ConnectCommand { get; private set; }
        public ICommand UploadCommand { get; private set; }
        public ICommand DownloadCommand { get; private set; }
        public ICommand ShowRenameDialogCommand { get; private set; }
        public ICommand RenameItemCommand { get; private set; }
        public ICommand CancelDialogCommand { get; private set; }
        public ICommand CreateLocalDirectoryCommand { get; private set; }
        public ICommand CreateServerDirectoryCommand { get; private set; }
        public ICommand RenameLocalItemCommand { get; private set; }
        public ICommand RenameServerItemCommand { get; private set; }
        public ICommand DeleteLocalItemCommand { get; private set; }
        public ICommand DeleteServerItemCommand { get; private set; }
        public ICommand RefreshLocalPanelCommand { get; private set; }
        public ICommand RefreshServerPanelCommand { get; private set; }
        public ICommand CreateLocalFileCommand { get; private set; }
        public ICommand EditServerFileCommand { get; private set; }

        #region Command Predicates(CanExecute Logic)
        
        private async Task ConnectAsync()
        {
            IsConnected = false;
            ServerPanel.FilesAndFolders.Clear();

            _logger.Log($"Подключение к {FtpConnectionSettings.Host}...", LogLevel.Info);

            try
            {
                var files = await _ftpService.GetDirectoryListingAsync("/", FtpConnectionSettings);
                ServerPanel.LoadItems(files, "/");
                _logger.Log("Подключение успешно.", LogLevel.Success);

                IsConnected = true;
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка подключения: {ex.Message}", LogLevel.Error);
            }
        }

        private bool CanEditServerFile()
        {
            if (ServerPanel.SelectedFileItem == null || ServerPanel.SelectedFileItem.IsDirectory || !IsConnected)
                return false;

            var extension = Path.GetExtension(ServerPanel.SelectedFileItem.FileName).ToLower();

            return extension == ".txt" || extension == ".json" || extension == ".xml" || extension == ".log";
        }

        private bool CanUpload()
        {
            return LocalPanel.SelectedFileItem != null &&
                   IsConnected &&
                   !IsTransferInProgress;
        }

        private bool CanOperateOnLocalItem() => LocalPanel.SelectedFileItem != null;

        private bool CanOperateOnServerItem() => ServerPanel.SelectedFileItem != null && IsConnected;

        private bool CanConnect() => !string.IsNullOrEmpty(FtpConnectionSettings.Host) &&
                                     !string.IsNullOrEmpty(FtpConnectionSettings.Username);

        private bool CanDownload() => ServerPanel.SelectedFileItem != null &&
                                      IsConnected &&
                                      !IsTransferInProgress;

        private bool CanCreateLocalDirectory() => LocalPanel.CurrentPath != LocalRootPath;

        #endregion Command Predicates(CanExecute Logic)

        #endregion

        #region Methods

        #region LocalPanel Methods
        private void LoadInitialDrives()
        {
            try
            {
                var drives = _localFileService.GetDrives();
                LocalPanel.LoadItems(drives, LocalRootPath);
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка загрузки дисков: {ex.Message}", LogLevel.Error);
            }
        }

        private void NavigateLocalDirectory(string path)
        {
            try
            {
                if (path == LocalRootPath)
                {
                    LoadInitialDrives();
                    return;
                }

                var items = _localFileService.GetDirectoryListing(path);
                LocalPanel.LoadItems(items, path);
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка навигации: {ex.Message}", LogLevel.Error);
            }
        }

        private void CreateLocalDirectory()
        {
            string newFolderName = _dialogService.ShowNewItemDialog("Создать папку", "Новая папка");

            if (string.IsNullOrWhiteSpace(newFolderName))
                return;

            var newPath = Path.Combine(LocalPanel.CurrentPath, newFolderName);

            try
            {
                _localFileService.CreateDirectory(newPath);
                _logger.Log($"Папка '{newFolderName}' создана.", LogLevel.Success);
                RefreshLocalPanel();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка создания папки: {ex.Message}", LogLevel.Error);
            }
        }

        private void RenameLocalItem()
        {
            var item = LocalPanel.SelectedFileItem;

            if (item == null)
                return;

            string newName = _dialogService.ShowNewItemDialog("Переименовать", "", item.FileName);

            if (string.IsNullOrWhiteSpace(newName) || newName == item.FileName)
                return;

            if (!item.IsDirectory && !Path.HasExtension(newName))
                newName += Path.GetExtension(item.FileName);

            var oldPath = Path.Combine(LocalPanel.CurrentPath, item.FileName);
            var newPath = Path.Combine(LocalPanel.CurrentPath, newName);

            try
            {
                _localFileService.RenameItem(oldPath, newPath);
                _logger.Log($"'{item.FileName}' переименован в '{newName}'.", LogLevel.Success);
                RefreshLocalPanel();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка переименования: {ex.Message}", LogLevel.Error);
            }
        }

        private void DeleteLocalItem()
        {
            var item = LocalPanel.SelectedFileItem;
            var path = Path.Combine(LocalPanel.CurrentPath, item.FileName);

            if (_dialogService.ShowConfirmation("Подтверждение", $"Удалить '{item.FileName}'?"))
            {
                try
                {
                    _localFileService.DeleteItem(path);
                    _logger.Log($"'{item.FileName}' удален.", LogLevel.Success);
                    RefreshLocalPanel();
                }
                catch (Exception ex)
                {
                    _logger.Log($"Ошибка удаления: {ex.Message}", LogLevel.Error);
                }
            }
        }

        #endregion LocalPanel Methods

        #region ServerPanel Methods
        private async Task NavigateServerDirectoryAsync(string path)
        {
            if (!IsConnected)
                return;

            _logger.Log($"Загрузка {path}...", LogLevel.Info);

            try
            {
                var items = await _ftpService.GetDirectoryListingAsync(path, FtpConnectionSettings);
                ServerPanel.LoadItems(items, path);
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка навигации по FTP: {ex.Message}", LogLevel.Error);
            }
        }

        private async Task CreateServerDirectoryAsync()
        {
            string newFolderName = _dialogService.ShowNewItemDialog("Создать папку на сервере", "Новая папка");

            if (string.IsNullOrWhiteSpace(newFolderName))
                return;

            var newPath = Path.Combine(ServerPanel.CurrentPath, newFolderName).Replace('\\', '/');

            try
            {
                await _ftpService.CreateDirectoryAsync(newPath, FtpConnectionSettings);
                _logger.Log($"Папка '{newFolderName}' создана на сервере.", LogLevel.Success);
                await RefreshServerPanelAsync();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка создания папки: {ex.Message}", LogLevel.Error);
            }
        }

        private async Task RenameServerItemAsync()
        {
            var item = ServerPanel.SelectedFileItem;

            if (item == null)
                return;

            string newName = _dialogService.ShowNewItemDialog("Переименовать на сервере", "", item.FileName);

            if (string.IsNullOrWhiteSpace(newName) || newName == item.FileName)
                return;

            if (!item.IsDirectory && !Path.HasExtension(newName))
                newName += Path.GetExtension(item.FileName);

            var oldPath = Path.Combine(ServerPanel.CurrentPath, item.FileName).Replace('\\', '/');
            var newPath = Path.Combine(ServerPanel.CurrentPath, newName).Replace('\\', '/');

            try
            {
                await _ftpService.RenameAsync(oldPath, newPath, FtpConnectionSettings);
                _logger.Log($"'{item.FileName}' переименован в '{newName}' на сервере.", LogLevel.Success);
                await RefreshServerPanelAsync();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка переименования: {ex.Message}", LogLevel.Error);
            }
        }

        private async Task DeleteServerItemAsync()
        {
            var item = ServerPanel.SelectedFileItem;
            var path = Path.Combine(ServerPanel.CurrentPath, item.FileName).Replace('\\', '/');

            if (_dialogService.ShowConfirmation("Подтверждение", $"Удалить файл '{item.FileName}' с сервера?"))
            {
                try
                {
                    if (item.IsDirectory)
                        await _ftpService.DeleteDirectoryAsync(path, FtpConnectionSettings);
                    else
                        await _ftpService.DeleteFileAsync(path, FtpConnectionSettings);

                    _logger.Log($"Файл '{item.FileName}' удален с сервера.", LogLevel.Success);
                    await RefreshServerPanelAsync();
                }
                catch (Exception ex)
                {
                    _logger.Log($"Ошибка удаления: {ex.Message}", LogLevel.Error);
                }
            }
        }

        private async Task UploadAsync()
        {
            var selectedLocalItem = LocalPanel.SelectedFileItem;

            if (selectedLocalItem == null)
                return;

            IsTransferInProgress = true;
            StatusText = $"Загрузка: {selectedLocalItem.FileName}";
            CurrentProgress = 0;

            try
            {
                var localPath = Path.Combine(LocalPanel.CurrentPath, selectedLocalItem.FileName);
                var remoteDir = ServerPanel.CurrentPath;
                var progress = new Progress<double>(percent => CurrentProgress = percent);

                if (selectedLocalItem.IsDirectory)
                {
                    await _ftpService.UploadDirectoryAsync(localPath, remoteDir, FtpConnectionSettings, progress);
                    _logger.Log($"Папка '{selectedLocalItem.FileName}' успешно загружена на ftp-сервер.", LogLevel.Success);
                }
                else
                {
                    var remoteFilePath = Path.Combine(remoteDir, selectedLocalItem.FileName).Replace('\\', '/');

                    await _ftpService.UploadFileAsync(localPath, remoteFilePath, FtpConnectionSettings, progress);
                    _logger.Log($"Файл '{selectedLocalItem.FileName}' успешно загружен на ftp-сервер.", LogLevel.Success);
                }

                await RefreshServerPanelAsync();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка передачи: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsTransferInProgress = false;
            }
        }

        private async Task DownloadAsync()
        {
            var selectedRemoteItem = ServerPanel.SelectedFileItem;
            if (selectedRemoteItem == null) return;

            IsTransferInProgress = true;
            StatusText = $"Скачивание: {selectedRemoteItem.FileName}";
            CurrentProgress = 0;

            try
            {
                var remotePath = Path.Combine(ServerPanel.CurrentPath, selectedRemoteItem.FileName).Replace('\\', '/');
                var localDir = LocalPanel.CurrentPath;
                var progress = new Progress<double>(percent => CurrentProgress = percent);

                if (selectedRemoteItem.IsDirectory)
                {
                    await _ftpService.DownloadDirectoryAsync(remotePath, localDir, FtpConnectionSettings, progress);
                    _logger.Log($"Папка '{selectedRemoteItem.FileName}' успешно скачана в '{LocalPanel.CurrentPath}'.", LogLevel.Success);
                }
                else
                {
                    await _ftpService.DownloadFileAsync(remotePath, localDir, FtpConnectionSettings, progress);
                    _logger.Log($"Файл '{selectedRemoteItem.FileName}' успешно скачан в '{LocalPanel.CurrentPath}'.", LogLevel.Success);
                }

                RefreshLocalPanel();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка скачивания: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsTransferInProgress = false;
            }
        }

        private async Task EditServerFileAsync()
        {
            var selectedFile = ServerPanel.SelectedFileItem;

            if (selectedFile == null)
                return;

            await _fileEditorService.EditRemoteFileAsync(selectedFile, ServerPanel.CurrentPath, FtpConnectionSettings);

            await RefreshServerPanelAsync();
        }

        #endregion ServerPanel Methods

        private void InitializeCommands()
        {
            ConnectCommand = new RelayCommand(async _ => await ConnectAsync(), _ => CanConnect());
            UploadCommand = new RelayCommand(async _ => await UploadAsync(), _ => CanUpload());
            DownloadCommand = new RelayCommand(async _ => await DownloadAsync(), _ => CanDownload());

            CreateLocalDirectoryCommand = new RelayCommand(_ => CreateLocalDirectory(), _ => CanCreateLocalDirectory());
            RenameLocalItemCommand = new RelayCommand(_ => RenameLocalItem(), _ => CanOperateOnLocalItem());
            DeleteLocalItemCommand = new RelayCommand(_ => DeleteLocalItem(), _ => CanOperateOnLocalItem());
            RefreshLocalPanelCommand = new RelayCommand(_ => RefreshLocalPanel());

            CreateServerDirectoryCommand = new RelayCommand(async _ => await CreateServerDirectoryAsync(), _ => IsConnected);
            RenameServerItemCommand = new RelayCommand(async _ => await RenameServerItemAsync(), _ => CanOperateOnServerItem());
            DeleteServerItemCommand = new RelayCommand(async _ => await DeleteServerItemAsync(), _ => CanOperateOnServerItem());
            RefreshServerPanelCommand = new RelayCommand(async _ => await RefreshServerPanelAsync(), _ => IsConnected);
            EditServerFileCommand = new RelayCommand(async _ => await EditServerFileAsync(), _ => CanEditServerFile());
        }

        #endregion

        #region HelpMethods

        private async Task RefreshServerPanelAsync()
        {
            if (!IsConnected)
                return;

            try
            {
                var items = await _ftpService.GetDirectoryListingAsync(ServerPanel.CurrentPath, FtpConnectionSettings);
                ServerPanel.LoadItems(items, ServerPanel.CurrentPath);
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка обновления серверной панели: {ex.Message}", LogLevel.Error);
            }
        }

        private void RefreshLocalPanel()
        {
            try
            {
                var items = _localFileService.GetDirectoryListing(LocalPanel.CurrentPath);
                LocalPanel.LoadItems(items, LocalPanel.CurrentPath);
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка обновления локальной панели: {ex.Message}", LogLevel.Error);
            }
        }

        #endregion

        #region ProgressBarProperties  

        private bool _isTransferInProgress;
        private double _currentProgress;
        private string _statusText;

        public bool IsTransferInProgress
        {
            get => _isTransferInProgress;
            set => SetProperty(ref _isTransferInProgress, value);
        }

        public double CurrentProgress
        {
            get => _currentProgress;
            set => SetProperty(ref _currentProgress, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        #endregion
    }
}