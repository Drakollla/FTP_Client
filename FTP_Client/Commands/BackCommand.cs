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

        public override void Execute(object parameter)
        {
            if ((string)parameter == "local")
            {
                if (_mainViewModel.BackStackLocal.Count > 0)
                {
                    var previousPath = _mainViewModel.BackStackLocal.Pop();
                    _mainViewModel.ForwardStackLocal.Push(_mainViewModel.CurrentPathLocal);
                    _mainViewModel.NavigateToFolder(previousPath);
                }
            }
            else if ((string)parameter == "server")
            {
                if (_mainViewModel.BackStackServer.Count > 1)
                {
                    _mainViewModel.ForwardStackServer.Push(_mainViewModel.BackStackServer.Pop());
                    string previousFolderPath = _mainViewModel.BackStackServer.Peek();
                    _mainViewModel.LoadFolder(previousFolderPath);
                    _mainViewModel.CurrentPathServer = previousFolderPath;
                }
            }
        }
    }
}