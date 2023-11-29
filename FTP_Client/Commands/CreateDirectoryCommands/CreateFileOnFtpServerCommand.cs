using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Net;
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

        public override void Execute(object? parameter)
        {
            var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.FtpConnectionSettings.ServerAddress + _mainViewModel.CurrentPathServer + _mainViewModel.NewFileName);
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
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage("Ошибка при попытке создать файлЫ на FTP сервере: " + ex.Message, Brushes.Red);
            }
            catch (Exception e)
            {
                _mainViewModel.AddLogMessage("Ошибка: " + e.Message, Brushes.Red);
            }
        }
    }
}