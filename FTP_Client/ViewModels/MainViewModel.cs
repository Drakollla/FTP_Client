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
using System.Windows.Input;
using System.Windows.Media;

namespace FTP_Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            CurrentPathServer = "/";
            //temp
            LoadFolder(CurrentPathServer);

            InitializingCommands();
            LoadDrives();
        }

        private void InitializingCommands()
        {
            ListViewContextMenuCommands.Add(OpenNewFolderDialogCommand = new OpenNewFolderDialogCommand(this));
            ListViewContextMenuCommands.Add(ViewFileCommand = new ViewFileCommand(this));
            ListViewContextMenuCommands.Add(LoadFromFTPServer = new DownloadFileCommand(this));
            ListViewContextMenuCommands.Add(OpenRenameDialogCommand = new OpenRenameDialogCommand(this));
            ListViewContextMenuCommands.Add(DeleteFileCommand = new DeleteFileCommand(this));
            ListViewContextMenuCommands.Add(UpdateCommand = new UpdateCommand(this));

            CreateDirectoryOnFTPServerCommand = new CreateDirectoryOnFTPServerCommand(this);
            RenameCommand = new RenameCommand(this);
            CancelCommand = new CancelCommand(this);

            BackCommand = new BackCommand(this);
            ForwardCommand = new ForwardCommand(this);
            MouseClickCommand = new MouseClickCommand(this);
            ConnectFTPServerCommand = new ConnectFTPServerCommand(this);
            FtpConnectionSettings = new FtpConnectionSettings();
        }

        #region FieldsAndProperty
        public Stack<string> BackStackLocal = new();
        public Stack<string> ForwardStackLocal = new();
        public Stack<string> BackStackServer = new();
        public Stack<string> ForwardStackServer = new();
        public ObservableCollection<FileItem> FilesAndFoldersLocal { get; set; } = new();
        public ObservableCollection<FileItem> FilesAndFoldersServer { get; set; } = new();
        public ObservableCollection<ICommand> ListViewContextMenuCommands { get; } = new();
        public ObservableCollection<LogMessage> LogMessages { get; set; } = new();

        public string _currentPathLocal;
        public string CurrentPathLocal
        {
            get => _currentPathLocal;
            set => SetProperty(ref _currentPathLocal, value);
        }

        public string _currentPathServer;
        public string CurrentPathServer
        {
            get => _currentPathServer;
            set => SetProperty(ref _currentPathServer, value);
        }

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

        private FileItem _selectedFileItemLocal;
        public FileItem SelectedFileItemLocal
        {
            get => _selectedFileItemLocal;
            set => SetProperty(ref _selectedFileItemLocal, value);
        }

        private FileItem _selectedFileItemServer;
        public FileItem SelectedFileItemServer
        {
            get => _selectedFileItemServer;
            set => SetProperty(ref _selectedFileItemServer, value);
        }

        private string _newName;
        public string NewName
        {
            get => _newName;
            set => SetProperty(ref _newName, value);
        }

        private string _txtFileContent;
        public string TxtFileContent
        {
            get => _txtFileContent;
            set => SetProperty(ref _txtFileContent, value);
        }
        #endregion FieldsAndProperty

        #region PublicMethods
        public void AddLogMessage(string mesaage, SolidColorBrush color) =>
            LogMessages.Add(new LogMessage { Text = mesaage, MessageColor = color });

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

        public void LoadFolder(string folderPath)
        {
            if (FilesAndFoldersServer.Count != 0)
                FilesAndFoldersServer.Clear();

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FtpConnectionSettings.ServerAddress + folderPath);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(FtpConnectionSettings.Username, FtpConnectionSettings.Password);

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                List<FileItem> files = new List<FileItem>();

                string line = reader.ReadLine();
                while (line != null)
                {
                    string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string name = tokens[8];

                    bool isFolder = line.StartsWith("d");
                    string fileType = isFolder ? "Folder" : "File";

                    var size = long.Parse(tokens[4]);

                    var dateModified = DateTime.Parse(tokens[5] + " " + tokens[6] + " " + tokens[7]);

                    files.Add(new FileItem { FileName = name, Size = size, LastModified = dateModified, FileType = fileType });

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
        #endregion PublicMethods

        #region Commands
        private BaseCommand _backCommand;
        public BaseCommand BackCommand
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

        private DeleteFileCommand _deleteFileCommand;
        public DeleteFileCommand DeleteFileCommand
        {
            get => _deleteFileCommand;
            set => SetProperty(ref _deleteFileCommand, value);
        }

        private DownloadFileCommand _loadFromFTPServer;
        public DownloadFileCommand LoadFromFTPServer
        {
            get => _loadFromFTPServer;
            set => SetProperty(ref _loadFromFTPServer, value);
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
    }
}