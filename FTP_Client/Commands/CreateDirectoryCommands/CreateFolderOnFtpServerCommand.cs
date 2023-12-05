using FTP_Client.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.CreateDirectoryCommands
{
    public class CreateFolderOnFtpServerCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public CreateFolderOnFtpServerCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object? parameter)
        {
            var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.CurrentPathServer + _mainViewModel.NewFileName);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            try
            {
                var response = (FtpWebResponse)request.GetResponse();

                _mainViewModel.AddLogMessage("Папка успешно создана. Статус: " + response.StatusDescription, Brushes.Green);
                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);

                var topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                topWindow?.Close();
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage($"Ошибка при попытке создать папку на FTP сервере:  {response?.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception e)
            {
                _mainViewModel.AddLogMessage("Ошибка: " + e.Message, Brushes.Red);
            }
        }
    }
}