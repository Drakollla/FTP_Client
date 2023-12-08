using FTP_Client.ViewModels;
using FTP_Client.Views;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class ViewLocalTxtFile : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public ViewLocalTxtFile(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Просмотр";

        public override void Execute(object? parameter)
        {
            var fileExtension = Path.GetExtension(_mainViewModel.GetFilePath);

            if (!string.IsNullOrEmpty(fileExtension))
            {
                string[] textExtensions = { ".txt" };

                try
                {
                    if (textExtensions.Contains(fileExtension.ToLower()))
                        _mainViewModel.TxtFileContent = File.ReadAllText(_mainViewModel.GetFilePath);
                }
                catch (Exception ex)
                {
                    _mainViewModel.AddLogMessage($"Невозможно просмотреть: {_mainViewModel.SelectedFileItemLocal.FileName}", Brushes.Orange);
                }
            }

            var readFileDialog = new ReadFileLocalDialog()
            {
                DataContext = _mainViewModel,
                Owner = Application.Current.MainWindow,
            };

            if (readFileDialog.ShowDialog() == true) { }
        }
    }
}