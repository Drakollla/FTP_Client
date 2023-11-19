using FTP_Client.ViewModels;
using FTP_Client.Views;
using System.Windows;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class OpenRenameDialogCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public OpenRenameDialogCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        //todo: менять текст в зависимости папка или файл будет переименовываться
        public override string CommandName => "Переименовать";

        public override void Execute(object parameter)
        {
            var renameDialog = new RenameDialog()
            {
                DataContext = _mainViewModel,
                Owner = Application.Current.MainWindow,
            };
            
            _mainViewModel.NewName = _mainViewModel.SelectedFileItemServer.FileName;

            if (renameDialog.ShowDialog() == true)
            {
            }
        }
    }
}