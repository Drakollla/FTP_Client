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

        public async override void Execute(object? parameter)
        {
            if (parameter as string == "local")
                _mainViewModel.NavigateToFolder(_mainViewModel.CurrentPathLocal);
            else if (parameter as string == "server")
                await _mainViewModel.LoadFolderAsync(_mainViewModel.CurrentPathServer);
        }
    }
}