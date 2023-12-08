using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class UploadFileCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public UploadFileCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Загрузить файл на сеервер";

        public async override void Execute(object? parameter)
        {
            var ftpServerUrl = _mainViewModel.CurrentPathServer + _mainViewModel.SelectedFileItemLocal.FileName;

            try
            {
                await Task.Run(async () =>
               {
                   var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(ftpServerUrl);
                   request.Method = WebRequestMethods.Ftp.UploadFile;

                   using (var fileStream = File.OpenRead(_mainViewModel.GetFilePath))
                   using (var ftpStream = request.GetRequestStream())
                   {
                       fileStream.CopyTo(ftpStream);
                   }

                   await Application.Current.Dispatcher.InvokeAsync(async () =>
                   {
                       _mainViewModel.AddLogMessage($"Файл {_mainViewModel.SelectedFileItemLocal.FileName} успешно загружен на FTP-сервер.", Brushes.Green);
                       await _mainViewModel.LoadFolderAsync(_mainViewModel.CurrentPathServer);
                   });
               });
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    _mainViewModel.AddLogMessage($"Ошибка при попытке загрузить файл {_mainViewModel.SelectedFileItemLocal.FileName} на сервер: {response.StatusDescription}" + ex.Message, Brushes.Orange);
                else
                    _mainViewModel.AddLogMessage($"Ошибка при создании папки на FTP сервере: {response.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage($"Ошибка при создании папки на FTP сервере:" + ex.Message, Brushes.Red);
            }
        }
    }
}