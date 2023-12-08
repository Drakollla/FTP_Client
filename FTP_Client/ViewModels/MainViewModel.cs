using FTP_Client.Commands;
using FTP_Client.Commands.ContextMenuCommand;
using FTP_Client.Commands.CreateDirectoryCommands;
using FTP_Client.Commands.NewFolderDialogCommands;
using FTP_Client.Commands.RenameDialogCo;
using FTP_Client.Helpers;
using FTP_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FTP_Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<LogMessage> LogMessages { get; set; } = new();

        private LogMessage _selectedLogMessage;
        public LogMessage SelectedLogMessage
        {
            get => _selectedLogMessage;
            set => SetProperty(ref _selectedLogMessage, value);
        }

        public MainViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            InitializingCommands();
            LoadDrives();
        }

        private void InitializingCommands()
        {
            OpenNewFolderDialogCommand = new OpenNewFolderDialogCommand(this);
            ViewFileCommand = new ViewFileCommand(this);
            SaveCommand = new SaveCommand(this);
            DownloadFileCommand = new DownloadFileCommand(this);
            OpenRenameDialogCommand = new OpenRenameDialogCommand(this);
            DeleteFileCommand = new DeleteFileCommand(this);
            UpdateCommand = new UpdateCommand(this);

            OpenCreateDialogCommand = new OpenCreateDialogCommand(this);
            CreateFolderOnFtpServerCommand = new CreateFolderOnFtpServerCommand(this);

            OpenCreateFileDialogCommand = new OpenCreateFileDialogCommand(this);
            CreateFileOnFtpServerCommand = new CreateFileOnFtpServerCommand(this);

            CreateDirectoryOnFTPServerCommand = new CreateDirectoryOnFTPServerCommand(this);
            RenameCommand = new RenameCommand(this);
            CancelCommand = new CancelCommand();

            UploadFileCommand = new UploadFileCommand(this);
            ViewLocalTxtFile = new ViewLocalTxtFile(this);

            BackCommand = new BackCommand(this);
            ForwardCommand = new ForwardCommand(this);
            MouseClickCommand = new MouseClickCommand(this);
            ConnectFTPServerCommand = new ConnectFTPServerCommand(this);
            MainPaigeCommand = new MainPaigeCommand(this);

            FtpConnectionSettings = new FtpConnectionSettings();
        }

        private ViewLocalTxtFile _viewLocalTxt;
        public ViewLocalTxtFile ViewLocalTxtFile
        {
            get => _viewLocalTxt;
            set => SetProperty(ref _viewLocalTxt, value);
        }

        private MainPaigeCommand _mainPaigeCommand;
        public MainPaigeCommand MainPaigeCommand
        {
            get => _mainPaigeCommand;
            set => SetProperty(ref _mainPaigeCommand, value);
        }

        public void AddLogMessage(string mesaage, SolidColorBrush color)
        {

            var logMessage = new LogMessage() { Text = mesaage, MessageColor = color };
            LogMessages.Add(logMessage);
            SelectedLogMessage = logMessage;
        }

        #region FieldsAndProperty

        private string _folderName;
        public string FolderName
        {
            get => _folderName;
            set => SetProperty(ref _folderName, value);
        }

        private FtpConnectionSettings _ftpConnectionSettings = new();
        public FtpConnectionSettings FtpConnectionSettings
        {
            get => _ftpConnectionSettings;
            set => SetProperty(ref _ftpConnectionSettings, value);
        }

        private string _newFileName;
        public string NewFileName
        {
            get => _newFileName;
            set => SetProperty(ref _newFileName, value);
        }

        private string _txtFileContent;
        public string TxtFileContent
        {
            get => _txtFileContent;
            set => SetProperty(ref _txtFileContent, value);
        }

        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }
        #endregion FieldsAndProperty

        #region Commands
        private BackCommand _backCommand;
        public BackCommand BackCommand
        {
            get => _backCommand;
            set => SetProperty(ref _backCommand, value);
        }

        private ForwardCommand _forwardCommand;
        public ForwardCommand ForwardCommand
        {
            get => _forwardCommand;
            set => SetProperty(ref _forwardCommand, value);
        }

        private DeleteFileCommand _deleteFileCommand;
        public DeleteFileCommand DeleteFileCommand
        {
            get => _deleteFileCommand;
            set => SetProperty(ref _deleteFileCommand, value);
        }

        private UpdateCommand _updateCommand;
        public UpdateCommand UpdateCommand
        {
            get => _updateCommand;
            set => SetProperty(ref _updateCommand, value);
        }

        private ViewFileCommand _viewFileCommand;
        public ViewFileCommand ViewFileCommand
        {
            get => _viewFileCommand;
            set => SetProperty(ref _viewFileCommand, value);
        }

        private CancelCommand _cancelCommand;
        public CancelCommand CancelCommand
        {
            get => _cancelCommand;
            set => SetProperty(ref _cancelCommand, value);
        }
        #endregion Commands


        #region LocalFileManager

        #region LoaclProperty
        public Stack<string> BackStackLocal = new();
        public Stack<string> ForwardStackLocal = new();

        public ObservableCollection<FileItem> FilesAndFoldersLocal { get; set; } = new();

        public string _currentPathLocal;
        public string CurrentPathLocal
        {
            get => _currentPathLocal;
            set => SetProperty(ref _currentPathLocal, value);
        }

        private FileItem _selectedFileItemLocal;
        public FileItem SelectedFileItemLocal
        {
            get => _selectedFileItemLocal;
            set => SetProperty(ref _selectedFileItemLocal, value);
        }

        public string GetFilePath => CurrentPathLocal + @"\" + SelectedFileItemLocal.FileName;
        #endregion LoaclProperty

        #region LocalMethod
        public void LoadDrives()
        {

            FilesAndFoldersLocal?.Clear();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    FilesAndFoldersLocal.Add(new FileItem
                    {
                        FileName = drive.Name,
                        FileType = "Drive",
                        Size = 0
                    });
                }
            }
        }

        public void NavigateToFolder(string folderPath)
        {
            try
            {
                CurrentPathLocal = folderPath;

                FilesAndFoldersLocal.Clear();

                foreach (var directory in Directory.GetDirectories(folderPath))
                {
                    var dirInfo = new DirectoryInfo(directory);

                    FilesAndFoldersLocal.Add(new FileItem
                    {
                        FileName = dirInfo.Name,
                        FileType = "Folder",
                        LastModified = dirInfo.LastWriteTime,
                        Size = 0
                    });
                }

                foreach (var file in Directory.GetFiles(folderPath))
                {
                    var fileInfo = new FileInfo(file);

                    FilesAndFoldersLocal.Add(new FileItem
                    {
                        FileName = fileInfo.Name,
                        FileType = fileInfo.Extension,
                        Size = fileInfo.Length
                    });
                }
            }
            catch (Exception ex)
            {
                AddLogMessage("Error: " + ex.Message, Brushes.Red);
            }
        }
        #endregion LocalMethod


        private UploadFileCommand _uploadFileCommand;
        public UploadFileCommand UploadFileCommand
        {
            get => _uploadFileCommand;
            set => SetProperty(ref _uploadFileCommand, value);
        }

        #endregion LocalFileManager


        #region FtpFileManager

        #region FtpProperty
        public Stack<string> BackStackServer = new();
        public Stack<string> ForwardStackServer = new();
        public ObservableCollection<FileItem> FilesAndFoldersServer { get; set; } = new();

        public string _currentPathServer;
        public string CurrentPathServer
        {
            get => _currentPathServer;
            set => SetProperty(ref _currentPathServer, value);
        }

        private FileItem _selectedFileItemServer;
        public FileItem SelectedFileItemServer
        {
            get => _selectedFileItemServer;
            set => SetProperty(ref _selectedFileItemServer, value);
        }

        private string _selectedExtension;
        public string SelectedExtension
        {
            get { return _selectedExtension; }
            set
            {
                _selectedExtension = value;
                OnPropertyChanged(nameof(SelectedExtension));
            }
        }

        public List<string> AvailableExtensions { get; } = new List<string> { ".txt", ".pdf", ".docx" };

        public string GetFilePatnOnFTP => CurrentPathServer + SelectedFileItemServer.FileName;
        #endregion FtpProperty

        #region FtpMethod
        //public void LoadFolder(string folderPath)
        //{
        //    if (FilesAndFoldersServer.Count != 0)
        //        FilesAndFoldersServer.Clear();

        //    try
        //    {
        //        var request = FtpConnectionSettings.CreateFtpRequest(folderPath);
        //        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

        //        using var response = (FtpWebResponse)request.GetResponse();
        //        var responseStream = response.GetResponseStream();
        //        var reader = new StreamReader(responseStream);

        //        var files = new List<FileItem>();

        //        var line = reader.ReadLine();
        //        while (line != null)
        //        {
        //            string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //            string name = tokens[8];

        //            bool isFolder = line.StartsWith("d");
        //            string fileType = isFolder ? "Folder" : Path.GetExtension(name);

        //            var size = long.Parse(tokens[4]);

        //            string month = tokens[5];
        //            int day = int.Parse(tokens[6]);

        //            var fileDateTime = new DateTime(DateTime.Now.Year, GetMonthNumber(month), day);
        //            files.Add(new FileItem { FileName = name, Size = size, LastModified = fileDateTime, FileType = fileType });


        //            line = reader.ReadLine();
        //        }

        //        foreach (var file in files)
        //            FilesAndFoldersServer.Add(file);

        //        AddLogMessage($"Загрузка содержимого {folderPath}  на FTP-сервере завершена: {response.StatusDescription}", Brushes.Green);

        //        reader.Close();
        //        response.Close();
        //    }
        //    catch (WebException ex)
        //    {
        //        var response = ex.Response as FtpWebResponse;
        //        AddLogMessage($"Ошибка при попытке загрузить содержимое {folderPath} на FTP-сервере: " + ex.Message, Brushes.Red);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddLogMessage("Error: " + ex.Message, Brushes.Red);
        //    }
        //}

        public async Task LoadFolderAsync(string folderPath)
        {
            if (FilesAndFoldersServer.Count != 0)
                FilesAndFoldersServer.Clear();

            try
            {
                var request = FtpConnectionSettings.CreateFtpRequest(folderPath);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    var files = new List<FileItem>();

                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string name = tokens[8];

                        bool isFolder = line.StartsWith("d");
                        string fileType = isFolder ? "Folder" : Path.GetExtension(name);

                        var size = long.Parse(tokens[4]);

                        string month = tokens[5];
                        int day = int.Parse(tokens[6]);

                        var fileDateTime = new DateTime(DateTime.Now.Year, GetMonthNumber(month), day);
                        files.Add(new FileItem { FileName = name, Size = size, LastModified = fileDateTime, FileType = fileType });
                    }

                    foreach (var file in files)
                        FilesAndFoldersServer.Add(file);

                    AddLogMessage($"Загрузка содержимого {folderPath} на FTP-сервере завершена: {response.StatusDescription}", Brushes.Green);
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                AddLogMessage($"Ошибка при попытке загрузить содержимое {folderPath} на FTP-сервере: {response?.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                AddLogMessage("Error: " + ex.Message, Brushes.Red);
            }
        }


        static int GetMonthNumber(string month)
        {
            DateTime dt = DateTime.ParseExact(month, "MMM", System.Globalization.CultureInfo.InvariantCulture);
            return dt.Month;
        }
        #endregion FtpMethod

        #region FtpCommands
        private SaveCommand _saveCommand;
        public SaveCommand SaveCommand
        {
            get => _saveCommand;
            set => SetProperty(ref _saveCommand, value);
        }

        private OpenCreateFileDialogCommand _openCreateFileDialogCommand;
        public OpenCreateFileDialogCommand OpenCreateFileDialogCommand
        {
            get => _openCreateFileDialogCommand;
            set => SetProperty(ref _openCreateFileDialogCommand, value);
        }

        private CreateFileOnFtpServerCommand _createFileOnFtpServerCommand;
        public CreateFileOnFtpServerCommand CreateFileOnFtpServerCommand
        {
            get => _createFileOnFtpServerCommand;
            set => SetProperty(ref _createFileOnFtpServerCommand, value);
        }

        private OpenCreateDialogCommand _openCreateDialogCommand;
        public OpenCreateDialogCommand OpenCreateDialogCommand
        {
            get => _openCreateDialogCommand;
            set => SetProperty(ref _openCreateDialogCommand, value);
        }

        private CreateFolderOnFtpServerCommand _createFolderOnFtpServerCommand;
        public CreateFolderOnFtpServerCommand CreateFolderOnFtpServerCommand
        {
            get => _createFolderOnFtpServerCommand;
            set => SetProperty(ref _createFolderOnFtpServerCommand, value);
        }

        private MouseClickCommand _mouseClickCommand;
        public MouseClickCommand MouseClickCommand
        {
            get => _mouseClickCommand;
            set => SetProperty(ref _mouseClickCommand, value);
        }

        private ConnectFTPServerCommand _connectFTPServerCommand;
        public ConnectFTPServerCommand ConnectFTPServerCommand
        {
            get => _connectFTPServerCommand;
            set => SetProperty(ref _connectFTPServerCommand, value);
        }

        private CreateDirectoryOnFTPServerCommand _createDirectory;
        public CreateDirectoryOnFTPServerCommand CreateDirectoryOnFTPServerCommand
        {
            get => _createDirectory;
            set => SetProperty(ref _createDirectory, value);
        }

        private OpenNewFolderDialogCommand _openNewFolderDialogCommand;
        public OpenNewFolderDialogCommand OpenNewFolderDialogCommand
        {
            get => _openNewFolderDialogCommand;
            set => SetProperty(ref _openNewFolderDialogCommand, value);
        }

        private DownloadFileCommand _downloadFileCommand;
        public DownloadFileCommand DownloadFileCommand
        {
            get => _downloadFileCommand;
            set => SetProperty(ref _downloadFileCommand, value);
        }

        private OpenRenameDialogCommand _openRenameDialogCommand;
        public OpenRenameDialogCommand OpenRenameDialogCommand
        {
            get => _openRenameDialogCommand;
            set => SetProperty(ref _openRenameDialogCommand, value);
        }

        private RenameCommand _renameCommand;
        public RenameCommand RenameCommand
        {
            get => _renameCommand;
            set => SetProperty(ref _renameCommand, value);
        }
        #endregion FtpCommands

        #endregion FtpFileManager
    }
}