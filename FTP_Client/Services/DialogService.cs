using FTP_Client.Services;
using FTP_Client.ViewModels.Dialogs;
using FTP_Client.Views.Dialogs;
using System.Windows;

namespace FTP_Client.Helpers
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public string ShowRenameDialog(string currentName)
        {
            // Создаем экземпляр нашего WPF-окна
            //var dialog = new RenameDialog();

            //// Можно передать ему начальные данные через ViewModel этого окна
            //dialog.DataContext = new RenameDialogViewModel { NewName = currentName };

            //// Показываем окно как модальное
            //if (dialog.ShowDialog() == true)
            //{
            //    // Если пользователь нажал "ОК", возвращаем новое имя
            //    return (dialog.DataContext as RenameDialogViewModel)?.NewName;
            //}

            // Если пользователь нажал "Отмена" или закрыл окно, возвращаем null
            return null;
        }

        public string ShowNewItemDialog(string title, string message, string defaultName = "")
        {
            var vm = new NewItemDialogViewModel
            {
                Title = title,     
                Message = message,
                ItemName = defaultName
            };

            var dialog = new NewItemDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            dialog.ShowDialog();

            if (vm.DialogResult == true)
                return vm.ItemName;

            return null;
        }
    }
}