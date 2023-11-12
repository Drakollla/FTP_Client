using FTP_Client.Commands;
using FTP_Client.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace FTP_Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            CurrentPathServer = "/";
            LoadFolder(CurrentPathServer);

            BackCommand = new BackCommand(this);
            ForwardCommand = new ForwardCommand(this);
            MouseClickCommand = new MouseClickCommand(this);
            FtpConnectionSettings = new FtpConnectionSettings();

            //todo: кнопка подключения
            ConnectCommand = new RelayCommand(Connect);

            LoadDrives();
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

                    //bool isFolder = line.StartsWith("d");   FileType = isFolder ? "Folder" : "File" 

                    var size = long.Parse(tokens[4]);

                    var dateModified = DateTime.Parse(tokens[5] + " " + tokens[6] + " " + tokens[7]);

                    files.Add(new FileItem { FileName = name, Size = size, LastModified = dateModified });

                    line = reader.ReadLine();
                }

                foreach (var file in files)
                    FilesAndFoldersServer.Add(file);

                reader.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public Stack<string> BackStackLocal = new Stack<string>();
        public Stack<string> ForwardStackLocal = new Stack<string>();

        public Stack<string> BackStackServer = new Stack<string>();
        public Stack<string> ForwardStackServer = new Stack<string>();
        public ObservableCollection<FileItem> FilesAndFoldersLocal { get; set; } = new ObservableCollection<FileItem>();
        public ObservableCollection<FileItem> FilesAndFoldersServer { get; set; } = new ObservableCollection<FileItem>();
        public ObservableCollection<string> LogItems { get; set; } = new ObservableCollection<string>();

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

        public void AddLogItem(string logItem) => LogItems.Add(logItem);

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
                MessageBox.Show("Error: " + ex.Message);
            }
        }

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

        private MouseClickCommand _mouseClickCommand;
        public MouseClickCommand MouseClickCommand
        {
            get => _mouseClickCommand;
            set => SetProperty(ref _mouseClickCommand, value);
        }

        public ObservableCollection<FileItem> SelectedFolderContents { get; set; } = new ObservableCollection<FileItem>();

        public ICommand ConnectCommand { get; set; }

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

        private void Connect()
        {
            CurrentPathServer = "/";
            LoadFolder(CurrentPathServer);


            // Код для установки соединения с FTP-сервером
            // используя ConnectionModel.ServerAddress, ConnectionModel.Username и ConnectionModel.Password

            // Проверка успешного соединения
            IsConnected = true;
        }

        private FtpConnectionSettings _ftpConnectionSettings = new();
        public FtpConnectionSettings FtpConnectionSettings
        {
            get => _ftpConnectionSettings;
            set => SetProperty(ref _ftpConnectionSettings, value);
        }
    }
}