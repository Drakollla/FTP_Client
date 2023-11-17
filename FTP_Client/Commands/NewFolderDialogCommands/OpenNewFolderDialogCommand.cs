using FTP_Client.ViewModels;
using System.Windows;

namespace FTP_Client.Commands.NewFolderDialogCommands
{
    public class OpenNewFolderDialogCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public OpenNewFolderDialogCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object parameter)
        {
            var newFolderDialog = new NewFolderDialog()
            {
                DataContext = _mainViewModel,
                Owner = Application.Current.MainWindow,
            };

            if (newFolderDialog.ShowDialog() == true)
            {
            }
        }
    }
}