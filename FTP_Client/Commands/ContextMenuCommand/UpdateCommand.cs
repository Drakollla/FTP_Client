using FTP_Client.ViewModels;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class UpdateCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public UpdateCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Обновить";

        public override void Execute(object parameter)
        {
            _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
        }
    }
}