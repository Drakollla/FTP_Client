using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class DownloadFileCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public DownloadFileCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Скачать";

        public override async void Execute(object? parameter)
        {
            try
            {
                await Task.Run(async () =>
                  {
                      var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.GetFilePatnOnFTP);
                      request.Method = WebRequestMethods.Ftp.DownloadFile;

                      using var response = (FtpWebResponse)request.GetResponse();
                      using (var responseStream = response.GetResponseStream())
                      {
                          var localFilePath = _mainViewModel.CurrentPathLocal + @"\" + _mainViewModel.SelectedFileItemServer.FileName;

                          using var fileStream = new FileStream(localFilePath, FileMode.CreateNew);
                          byte[] buffer = new byte[1024];
                          int bytesRead;

                          while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                          {
                              fileStream.Write(buffer, 0, bytesRead);
                          }
                      }

                      await Application.Current.Dispatcher.InvokeAsync(() =>
                      {
                          _mainViewModel.AddLogMessage($"Файл {_mainViewModel.SelectedFileItemServer.FileName} скачан: {response.StatusDescription}", Brushes.Green);
                          _mainViewModel.NavigateToFolder(_mainViewModel.CurrentPathLocal);
                      });
                  });
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                if (response?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    _mainViewModel.AddLogMessage($"Ошибка: Файл {_mainViewModel.SelectedFileItemServer.FileName} недоступен на FTP сервере: {response.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage($"Ошибка при удалении папки {_mainViewModel.SelectedFileItemServer.FileName} на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}