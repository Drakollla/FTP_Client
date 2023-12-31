﻿using FTP_Client.ViewModels;
using System.Windows.Media;

namespace FTP_Client.Commands
{
    public class ConnectFTPServerCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public ConnectFTPServerCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public async override void Execute(object? parameter)
        {
            _mainViewModel.AddLogMessage("Подключение к FTP серверу...", Brushes.Black);
            await _mainViewModel.LoadFolderAsync(string.Empty);
            _mainViewModel.CurrentPathServer = string.Empty;
        }
    }
}