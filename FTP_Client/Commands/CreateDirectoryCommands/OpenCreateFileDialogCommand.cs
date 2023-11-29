using FTP_Client.ViewModels;
using FTP_Client.Views;
using System.Windows;

namespace FTP_Client.Commands.CreateDirectoryCommands
{
    public class OpenCreateFileDialogCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public OpenCreateFileDialogCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Создать файл";

        public override void Execute(object? parameter)
        {
            var createFileDialog = new CreateFileDialog()
            {
                DataContext = _mainViewModel,
                Owner = Application.Current.MainWindow,
            };

            _mainViewModel.NewFileName = string.Empty;

            if (createFileDialog.ShowDialog() == true) { }
        }
    }
}