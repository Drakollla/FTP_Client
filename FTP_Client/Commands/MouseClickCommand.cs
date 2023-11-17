﻿using FTP_Client.ViewModels;
using System.IO;

namespace FTP_Client.Commands
{
    public class MouseClickCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public MouseClickCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object parameter)
        {
            if ((string)parameter == "local")
            {
                if (_mainViewModel.SelectedFileItemLocal != null)
                {
                    if (_mainViewModel.SelectedFileItemLocal.FileType == "Drive")
                        _mainViewModel.NavigateToFolder(_mainViewModel.SelectedFileItemLocal.FileName);
                    else if (_mainViewModel.SelectedFileItemLocal.FileType == "Folder")
                    {
                        var folderPath = Path.Combine(_mainViewModel.CurrentPathLocal, _mainViewModel.SelectedFileItemLocal.FileName);
                        _mainViewModel.BackStackLocal.Push(_mainViewModel.CurrentPathLocal);
                        _mainViewModel.ForwardStackLocal.Clear();
                        _mainViewModel.NavigateToFolder(folderPath);
                    }
                }
            }
            else if ((string)parameter == "server")
                if (_mainViewModel.SelectedFileItemServer != null)
                {
                    if (_mainViewModel.SelectedFileItemServer != null && _mainViewModel.SelectedFileItemServer.FileType == "Folder")
                    {
                        var folderPath = _mainViewModel.CurrentPathServer + _mainViewModel.SelectedFileItemServer.FileName + "/";
                        _mainViewModel.LoadFolder(folderPath);
                        _mainViewModel.CurrentPathServer = folderPath;
                        _mainViewModel.BackStackServer.Push(_mainViewModel.CurrentPathServer);
                    }
                }
        }
    }
}