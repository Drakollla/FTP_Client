using FTP_Client.Commands;
using FTP_Client.Commands.ContextMenuCommand;
using FTP_Client.Commands.NewFolderDialogCommands;
using FTP_Client.Commands.RenameDialogCo;
using FTP_Client.Helpers;
using FTP_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FTP_Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<LogMessage> LogMessages { get; set; } = new();

        public MainViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            CurrentPathServer = "/";
            InitializingCommands();
            LoadDrives();
        }

        private void InitializingCommands()
        {
            OpenNewFolderDialogCommand = new OpenNewFolderDialogCommand(this);
            ViewFileCommand = new ViewFileCommand(this);
            DownloadFileCommand = new DownloadFileCommand(this);
            OpenRenameDialogCommand = new OpenRenameDialogCommand(this);
            DeleteFileCommand = new DeleteFileCommand(this);
            UpdateCommand = new UpdateCommand(this);

            CreateDirectoryOnFTPServerCommand = new CreateDirectoryOnFTPServerCommand(this);
            RenameCommand = new RenameCommand(this);
            CancelCommand = new CancelCommand();

            UploadFileCommand = new UploadFileCommand(this);

            BackCommand = new BackCommand(this);
            ForwardCommand = new ForwardCommand(this);
            MouseClickCommand = new MouseClickCommand(this);
            ConnectFTPServerCommand = new ConnectFTPServerCommand(this);
            FtpConnectionSettings = new FtpConnectionSettings();
        }

        public void AddLogMessage(string mesaage, SolidColorBrush color) =>
            LogMessages.Add(new LogMessage { Text = mesaage, MessageColor = color });

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
        #endregion LoaclProperty

        #region LocalMethod
        private void LoadDrives()
        {
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
        #endregion FtpProperty

        #region FtpMethod
        public void LoadFolder(string folderPath)
        {
            if (FilesAndFoldersServer.Count != 0)
                FilesAndFoldersServer.Clear();

            try
            {
                var request = FtpConnectionSettings.CreateFtpRequest(FtpConnectionSettings.ServerAddress + folderPath);

                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                using var response = (FtpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);

                var files = new List<FileItem>();

                var line = reader.ReadLine();
                while (line != null)
                {
                    string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string name = tokens[8];

                    bool isFolder = line.StartsWith("d");
                    string fileType = isFolder ? "Folder" : "File";

                    var size = long.Parse(tokens[4]);

                    string month = tokens[5];
                    int day = int.Parse(tokens[6]);
                    string[] timeParts = tokens[7].Split(':');
                    int hour = int.Parse(timeParts[0]);
                    int minute = int.Parse(timeParts[1]);

                    DateTime now = DateTime.Now;
                    int year = now.Year; // Предполагаем, что текущий год - это год файла

                    DateTime fileDateTime = new DateTime(year, GetMonthNumber(month), day, hour, minute, 0);

                    files.Add(new FileItem { FileName = name, Size = size, LastModified = fileDateTime, FileType = fileType });

                    line = reader.ReadLine();
                }

                foreach (var file in files)
                    FilesAndFoldersServer.Add(file);

                AddLogMessage($"Загрузка содержимого на FTP сервере завершена: {response.StatusDescription}", Brushes.Green);

                reader.Close();
                response.Close();
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