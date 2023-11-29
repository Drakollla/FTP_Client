using FTP_Client.ViewModels;
using FTP_Client.Views;
using System.Windows;

namespace FTP_Client.Commands.CreateDirectoryCommands
{
    public class OpenCreateDialogCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public OpenCreateDialogCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Создать папку";

        public override void Execute(object? parameter)
        {
            var createFolderDialog = new CreateFolderDialog()
            {
                DataContext = _mainViewModel,
                Owner = Application.Current.MainWindow,
            };

            _mainViewModel.NewFileName = string.Empty;

            if (createFolderDialog.ShowDialog() == true) { }
        }
    }
}