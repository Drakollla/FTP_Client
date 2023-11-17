using FTP_Client.Commands;
using FTP_Client.Commands.NewFolderDialogCommands;
using FTP_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;

namespace FTP_Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private CreateDirectoryOnFTPServerCommand _createDirectory;
        public CreateDirectoryOnFTPServerCommand CreateDirectoryOnFTPServerCommand
        {
            get => _createDirectory;
            set => SetProperty(ref _createDirectory, value);
        }

        private CancelCommand _cancelCommand;
        public CancelCommand CancelCommand
        {
            get => _cancelCommand;
            set => SetProperty(ref _cancelCommand, value);
        }







        private OpenNewFolderDialogCommand _openNewFolderDialogCommand;
        public OpenNewFolderDialogCommand OpenNewFolderDialogCommand
        {
            get => _openNewFolderDialogCommand;
            set => SetProperty(ref _openNewFolderDialogCommand, value);
        }

        private string _folderName;
        public string FolderName
        {
            get => _folderName;
            set => SetProperty(ref _folderName, value);
        }



        public MainViewModel()
        {
            OpenNewFolderDialogCommand = new OpenNewFolderDialogCommand(this);
            CreateDirectoryOnFTPServerCommand = new CreateDirectoryOnFTPServerCommand(this);
            CancelCommand = new CancelCommand();


            CurrentPathServer = "/";

            BackCommand = new BackCommand(this);
            ForwardCommand = new ForwardCommand(this);
            MouseClickCommand = new MouseClickCommand(this);
            ConnectFTPServerCommand = new ConnectFTPServerCommand(this);
            FtpConnectionSettings = new FtpConnectionSettings();

            LoadDrives();
        }

        #region FieldsAndProperty
        public Stack<string> BackStackLocal = new();
        public Stack<string> ForwardStackLocal = new();
        public Stack<string> BackStackServer = new();
        public Stack<string> ForwardStackServer = new();
        public ObservableCollection<FileItem> FilesAndFoldersLocal { get; set; } = new();
        public ObservableCollection<FileItem> FilesAndFoldersServer { get; set; } = new();
        public ObservableCollection<string> LogItems { get; set; } = new();

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

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }
        #endregion FieldsAndProperty

        #region PublicMethods
        public void AddLogItem(string logItem) => LogItems.Add(logItem);

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
                AddLogItem("Подключение к FTP серверу...");

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FtpConnectionSettings.ServerAddress + folderPath);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                request.Credentials = new NetworkCredential(FtpConnectionSettings.Username, FtpConnectionSettings.Password);

                AddLogItem("Загрузка содержимого на FTP сервере...");

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

                reader.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                AddLogItem("Error: " + ex.Message);
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
                AddLogItem("Error: " + ex.Message);
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
        #endregion Commands
    }
}