using FTP_Client.ViewModels;

namespace FTP_Client.Commands
{
    public class BackCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public BackCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public async override void Execute(object? parameter)
        {
            if (parameter as string == "local")
            {
                if (_mainViewModel.BackStackLocal.Count > 0)
                {
                    var previousPath = _mainViewModel.BackStackLocal.Pop();
                    _mainViewModel.ForwardStackLocal.Push(_mainViewModel.CurrentPathLocal);
                    _mainViewModel.NavigateToFolder(previousPath);
                }
            }
            else if (parameter as string == "server")
            {
                if (_mainViewModel.BackStackServer.Count > 1)
                {
                    _mainViewModel.ForwardStackServer.Push(_mainViewModel.BackStackServer.Pop());
                    var previousFolderPath = _mainViewModel.BackStackServer.Peek();
                    await _mainViewModel.LoadFolderAsync(previousFolderPath);
                    _mainViewModel.CurrentPathServer = previousFolderPath;
                }
            }
        }
    }
}