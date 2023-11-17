using FTP_Client.ViewModels;

namespace FTP_Client.Commands
{
    public class ConnectFTPServerCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public ConnectFTPServerCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object parameter)
        {
            _mainViewModel.IsConnected = true;
            _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
        }
    }
}