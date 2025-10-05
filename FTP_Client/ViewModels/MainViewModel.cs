using FTP_Client.Enums;
using FTP_Client.Helpers;
using FTP_Client.Models;
using FTP_Client.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace FTP_Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public const string LocalRootPath = "Мой компьютер";

        #region Сервисы 
        private readonly ILoggerService _logger = new LoggerService();
        private readonly ILocalFileService _localFileService = new LocalFileService();
        private readonly IFtpService _ftpService = new FtpService();
        private readonly IDialogService _dialogService = new DialogService();
        #endregion

        #region Свойства

        public FilePanelViewModel LocalPanel { get; }
        public FilePanelViewModel ServerPanel { get; }

        public FtpConnectionSettings FtpConnectionSettings { get; } = new();
        public ObservableCollection<LogMessage> LogMessages => _logger.Messages;

        private bool _isRenameDialogVisible;
        public bool IsRenameDialogVisible
        {
            get => _isRenameDialogVisible;
            set => SetProperty(ref _isRenameDialogVisible, value);
        }

        private string _newItemName;
        public string NewItemName
        {
            get => _newItemName;
            set => SetProperty(ref _newItemName, value);
        }

        #endregion

        #region Команды

        public ICommand ConnectCommand { get; }
        public ICommand UploadCommand { get; }
        public ICommand DownloadCommand { get; }

        public ICommand ShowRenameDialogCommand { get; }
        public ICommand RenameItemCommand { get; }
        public ICommand CancelDialogCommand { get; }

        #endregion

        #region Конструктор и Инициализация

        public MainViewModel()
        {
            LocalPanel = new FilePanelViewModel("Локальный диск", PanelType.Local);
            ServerPanel = new FilePanelViewModel("FTP-Сервер", PanelType.Server);

            LocalPanel.NavigateRequested += NavigateLocalDirectory;
            ServerPanel.NavigateRequested += async (path) => await NavigateServerDirectoryAsync(path);

            ConnectCommand = new RelayCommand(async _ => await ConnectAsync(), _ => CanConnect());
            UploadCommand = new RelayCommand(async _ => await UploadAsync(), _ => CanUpload());
            DownloadCommand = new RelayCommand(async _ => await DownloadAsync(), _ => CanDownload());

            DeleteLocalItemCommand = new RelayCommand(_ => DeleteLocalItem(), _ => CanOperateOnLocalItem());
            DeleteServerItemCommand = new RelayCommand(async _ => await DeleteServerItemAsync(), _ => CanOperateOnServerItem());
            RefreshLocalPanelCommand = new RelayCommand(_ => RefreshLocalPanel());
            RefreshServerPanelCommand = new RelayCommand(async _ => await RefreshServerPanelAsync(), _ => IsConnected);

            CreateLocalDirectoryCommand = new RelayCommand(_ => CreateLocalDirectory());
            CreateServerDirectoryCommand = new RelayCommand(async _ => await CreateServerDirectoryAsync(), _ => IsConnected);
            RenameLocalItemCommand = new RelayCommand(_ => RenameLocalItem(), _ => CanOperateOnLocalItem());
            RenameServerItemCommand = new RelayCommand(async _ => await RenameServerItemAsync(), _ => CanOperateOnServerItem());


            //LocalPanel.ContextMenu = CreateLocalContextMenu();
            //ServerPanel.ContextMenu = CreateServerContextMenu();

            LoadInitialDrives();
        }



        private void CreateLocalDirectory()
        {
            string newFolderName = _dialogService.ShowNewItemDialog("Создать папку", "Новая папка");
            if (string.IsNullOrWhiteSpace(newFolderName)) return;

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

        private async Task CreateServerDirectoryAsync()
        {
            string newFolderName = _dialogService.ShowNewItemDialog("Создать папку на сервере", "Новая папка");
            if (string.IsNullOrWhiteSpace(newFolderName)) return;

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

        private void RenameLocalItem()
        {
            var item = LocalPanel.SelectedFileItem;
            string newName = _dialogService.ShowNewItemDialog("Переименовать", item.FileName);
            if (string.IsNullOrWhiteSpace(newName) || newName == item.FileName) return;

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

        private async Task RenameServerItemAsync()
        {
            var item = ServerPanel.SelectedFileItem;
            string newName = _dialogService.ShowNewItemDialog("Переименовать на сервере", item.FileName);
            if (string.IsNullOrWhiteSpace(newName) || newName == item.FileName) return;

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



        private ContextMenu CreateLocalContextMenu()
        {
            var menu = new ContextMenu();

            var uploadMenuItem = new MenuItem { Header = "Загрузить на сервер" };
            uploadMenuItem.Command = UploadCommand;
            menu.Items.Add(uploadMenuItem);

            var deleteMenuItem = new MenuItem { Header = "Удалить" };
            deleteMenuItem.Command = DeleteLocalItemCommand;
            menu.Items.Add(deleteMenuItem);

            var createLocalDirectorMenuItem = new MenuItem { Header = "Создать папку" };
            createLocalDirectorMenuItem.Command = CreateLocalDirectoryCommand;
            menu.Items.Add(createLocalDirectorMenuItem);

            var renameLocalItemMenuItem = new MenuItem { Header = "Переименовать" };
            renameLocalItemMenuItem.Command = RenameLocalItemCommand;

            menu.Items.Add(renameLocalItemMenuItem);

            return menu;
        }

        private ContextMenu CreateServerContextMenu()
        {
            var menu = new ContextMenu();

            var downloadMenuItem = new MenuItem { Header = "Скачать" };
            downloadMenuItem.Command = DownloadCommand;
            menu.Items.Add(downloadMenuItem);

            var deleteMenuItem = new MenuItem { Header = "Удалить" };
            deleteMenuItem.Command = DeleteServerItemCommand;
            menu.Items.Add(deleteMenuItem);

            var createServerDirectorMenuItem = new MenuItem { Header = "Создать папку" };
            createServerDirectorMenuItem.Command = CreateServerDirectoryCommand;
            menu.Items.Add(createServerDirectorMenuItem);

            var renameServerItemMenuItem = new MenuItem { Header = "Переименовать" };
            renameServerItemMenuItem.Command = RenameServerItemCommand;
            menu.Items.Add(renameServerItemMenuItem);


            return menu;
        }


        private bool CanOperateOnLocalItem() => LocalPanel.SelectedFileItem != null;
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

        private bool CanOperateOnServerItem() => ServerPanel.SelectedFileItem != null && IsConnected;
        private async Task DeleteServerItemAsync()
        {
            var item = ServerPanel.SelectedFileItem;
            var path = Path.Combine(ServerPanel.CurrentPath, item.FileName).Replace('\\', '/');

            if (_dialogService.ShowConfirmation("Подтверждение", $"Удалить '{item.FileName}' с сервера?"))
            {
                try
                {
                    if (item.IsDirectory)
                        await _ftpService.DeleteDirectoryAsync(path, FtpConnectionSettings);
                    else
                        await _ftpService.DeleteFileAsync(path, FtpConnectionSettings);

                    _logger.Log($"'{item.FileName}' удален с сервера.", LogLevel.Success);
                    await RefreshServerPanelAsync();
                }
                catch (Exception ex)
                {
                    _logger.Log($"Ошибка удаления: {ex.Message}", LogLevel.Error);
                }
            }
        }

        public ICommand CreateLocalDirectoryCommand { get; }
        public ICommand CreateServerDirectoryCommand { get; }
        public ICommand RenameLocalItemCommand { get; }
        public ICommand RenameServerItemCommand { get; }

        public ICommand DeleteLocalItemCommand { get; }
        public ICommand DeleteServerItemCommand { get; }
        public ICommand RefreshLocalPanelCommand { get; }
        public ICommand RefreshServerPanelCommand { get; }

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

        private async Task NavigateServerDirectoryAsync(string path)
        {
            if (!IsConnected) return;
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

        //private void NavigateLocalDirectory(string path)
        //{
        //    try
        //    {
        //        var items = _localFileService.GetDirectoryListing(path);
        //        LocalPanel.LoadItems(items, path);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log($"Ошибка навигации: {ex.Message}", LogLevel.Error);
        //    }
        //}

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
            catch (System.Exception ex)
            {
                _logger.Log($"Ошибка подключения: {ex.Message}", LogLevel.Error);
            }
        }

        private async Task UploadAsync()
        {
            var localPath = Path.Combine(LocalPanel.CurrentPath, LocalPanel.SelectedFileItem.FileName);
            var remoteDir = ServerPanel.CurrentPath;

            _logger.Log($"Загрузка файла {LocalPanel.SelectedFileItem.FileName}...", LogLevel.Info);
            try
            {
                // Просто вызываем метод сервиса
                await _ftpService.UploadFileAsync(localPath, remoteDir, FtpConnectionSettings);
                _logger.Log("Файл успешно загружен.", LogLevel.Success);

                // Обновляем содержимое серверной панели после загрузки
                await RefreshServerPanel();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка загрузки: {ex.Message}", LogLevel.Error);
            }
        }

        // Новый приватный метод для обновления
        private async Task RefreshServerPanel()
        {
            try
            {
                var files = await _ftpService.GetDirectoryListingAsync(ServerPanel.CurrentPath, FtpConnectionSettings);
                ServerPanel.LoadItems(files, ServerPanel.CurrentPath);
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка обновления: {ex.Message}", LogLevel.Error);
            }
        }


        #endregion

        #region Логика Команд (Приватные методы)

        private bool CanConnect()
        {
            return !string.IsNullOrEmpty(FtpConnectionSettings.Host) &&
                   !string.IsNullOrEmpty(FtpConnectionSettings.Username);
        }

        private bool CanUpload()
        {
            bool isLocalFileSelected = LocalPanel.SelectedFileItem != null &&
                                       LocalPanel.SelectedFileItem.FileType == FileItemType.File;

            return isLocalFileSelected && IsConnected;
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }


        private bool CanDownload()
        {
            return ServerPanel.SelectedFileItem != null &&
                   !ServerPanel.SelectedFileItem.IsDirectory &&
                   IsConnected;
        }

        private async Task DownloadAsync()
        {
            var selectedFile = ServerPanel.SelectedFileItem;
            var remoteFilePath = Path.Combine(ServerPanel.CurrentPath, selectedFile.FileName).Replace('\\', '/');
            var localDirectory = LocalPanel.CurrentPath;

            _logger.Log($"Скачивание файла '{selectedFile.FileName}'...", LogLevel.Info);
            try
            {
                await _ftpService.DownloadFileAsync(remoteFilePath, localDirectory, FtpConnectionSettings);
                _logger.Log("Файл успешно скачан.", LogLevel.Success);
                RefreshLocalPanel();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка скачивания файла: {ex.Message}", LogLevel.Error);
            }
        }

        #endregion

        #region Вспомогательные методы

        private async Task RefreshServerPanelAsync()
        {
            if (!IsConnected) return;
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
    }
}