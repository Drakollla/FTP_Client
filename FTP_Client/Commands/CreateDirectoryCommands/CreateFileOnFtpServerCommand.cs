using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.CreateDirectoryCommands
{
    public class CreateFileOnFtpServerCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public CreateFileOnFtpServerCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override async void Execute(object? parameter)
        {
            var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.CurrentPathServer + _mainViewModel.NewFileName + _mainViewModel.SelectedExtension);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            try
            {
                var fileData = Array.Empty<byte>();

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileData, 0, fileData.Length);
                }

                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                _mainViewModel.AddLogMessage("Файл успешно создан. Статус: " + response.StatusDescription, Brushes.Green);
                response.Close();

                await _mainViewModel.LoadFolderAsync(_mainViewModel.CurrentPathServer);

                var topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                topWindow?.Close();
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage($"Ошибка при попытке создать файлЫ на FTP сервере: {response?.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception e)
            {
                _mainViewModel.AddLogMessage("Ошибка: " + e.Message, Brushes.Red);
            }
        }
    }
}