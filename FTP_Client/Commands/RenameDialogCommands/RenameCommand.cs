using FTP_Client.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.RenameDialogCo
{
    public class RenameCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public RenameCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object? parameter)
        {
            RenameOnFtpServer();
        }

        private void RenameOnFtpServer()
        {
            try
            {
                var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.GetFilePatnOnFTP);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = _mainViewModel.NewFileName;

                using var response = (FtpWebResponse)request.GetResponse();
                
                _mainViewModel.AddLogMessage($"Файл успешно переименован: {response.StatusDescription}", Brushes.Green);
                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);

                var topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                topWindow?.Close();
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage("Ошибка при попытке переименовать файла/папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при попытке переименовать файла/папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}