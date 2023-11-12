using FTP_Client.ViewModels;

namespace FTP_Client.Commands
{
    public class ForwardCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public ForwardCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object parameter)
        {
            if ((string)parameter == "local")
            {
                if (_mainViewModel.ForwardStackLocal.Count > 0)
                {
                    var nextPath = _mainViewModel.ForwardStackLocal.Pop();
                    _mainViewModel.BackStackLocal.Push(_mainViewModel.CurrentPathLocal);
                    _mainViewModel.NavigateToFolder(nextPath);
                }
            }
            else if ((string)parameter == "server")
            {
                if (_mainViewModel.ForwardStackServer.Count > 0)
                {
                    var nextFolderPath = _mainViewModel.ForwardStackServer.Pop();
                    _mainViewModel.LoadFolder(nextFolderPath);
                    _mainViewModel.CurrentPathServer = nextFolderPath;

                    _mainViewModel.BackStackServer.Push(_mainViewModel.CurrentPathServer);
                }
            }
        }
    }
}