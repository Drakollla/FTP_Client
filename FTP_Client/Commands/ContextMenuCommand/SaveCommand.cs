using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        public override async void Execute(object? parameter)
        {
            string newFileContent = _mainViewModel.TxtFileContent;

            string serverUri = _mainViewModel.CurrentPathServer;
            string filePath = _mainViewModel.SelectedFileItemServer.FileName;

            await UploadTextFileToFtpAsync(serverUri, filePath, newFileContent);
        }

        public async Task UploadTextFileToFtpAsync(string serverUri, string filePath, string fileContent)
        {
            try
            {
                var requestUriString = serverUri + filePath;

                await Task.Run(async () =>
                {
                    var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(requestUriString);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.ContentLength = fileContent.Length;
                    request.UseBinary = true;
                    request.UsePassive = true;
                    request.KeepAlive = false;

                    byte[] fileData = System.Text.Encoding.UTF8.GetBytes(fileContent);

                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(fileData, 0, fileData.Length);
                    }

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        _mainViewModel.AddLogMessage($"Файла {filePath} сохранён: status {response.StatusDescription}", Brushes.Green);
                    });

                });

                var topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                topWindow?.Close();

                await _mainViewModel.LoadFolderAsync(_mainViewModel.CurrentPathServer);
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage($"Ошибка при попытке изменить содержимое файла {filePath} на FTP сервере: {response?.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage($"Ошибка при попытке изменить содержимое файла {filePath} на FTP сервере: " + ex.Message, Brushes.Red) ;
            }
        }
    }
}