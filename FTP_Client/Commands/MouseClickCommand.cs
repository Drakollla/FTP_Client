using FTP_Client.ViewModels;
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

        public async override void Execute(object? parameter)
        {
            if (parameter as string == "local")
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
            else if (parameter as string == "server")
            {
                if (_mainViewModel.SelectedFileItemServer != null)
                {
                    if (_mainViewModel.SelectedFileItemServer != null && _mainViewModel.SelectedFileItemServer.FileType == "Folder")
                    {
                        var folderPath = _mainViewModel.CurrentPathServer + _mainViewModel.SelectedFileItemServer.FileName + "/";
                        await _mainViewModel.LoadFolderAsync(folderPath);
                        _mainViewModel.CurrentPathServer = folderPath;
                        _mainViewModel.BackStackServer.Push(_mainViewModel.CurrentPathServer);
                    }
                }
            }
        }
    }
}