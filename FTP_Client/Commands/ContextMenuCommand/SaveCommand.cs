using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class SaveCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public SaveCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object? parameter)
        {
            string newFileContent = _mainViewModel.TxtFileContent;

            string serverUri = _mainViewModel.CurrentPathServer;
            string filePath = _mainViewModel.SelectedFileItemServer.FileName;
            UploadTextFileToFtp(serverUri, filePath, newFileContent);
        }

        public void UploadTextFileToFtp(string serverUri, string filePath, string fileContent)
        {
            try
            {
                var requestUriString = serverUri + filePath;
                var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(requestUriString);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.ContentLength = fileContent.Length;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                byte[] fileData = System.Text.Encoding.UTF8.GetBytes(fileContent);

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileData, 0, fileData.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    _mainViewModel.AddLogMessage($"Загрузка файла завершена, status {response.StatusDescription}", Brushes.Green);
                }

                var topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                topWindow?.Close();

                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage("Ошибка при попытке изменить содержимое файла на FTP сервере: " + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при попытке изменить содержимое файла на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}