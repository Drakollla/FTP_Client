﻿using FTP_Client.ViewModels;
using System.Threading.Tasks;

namespace FTP_Client.Commands
{
    public class MainPaigeCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public MainPaigeCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public async override void Execute(object? parameter)
        {
            if (parameter as string == "local")
            {
                _mainViewModel.LoadDrives();
                _mainViewModel.CurrentPathLocal = string.Empty;
            }
            else if (parameter as string == "server")
            {
                await _mainViewModel.LoadFolderAsync("/");
                _mainViewModel.CurrentPathServer = string.Empty;
            }
        }
    }
}