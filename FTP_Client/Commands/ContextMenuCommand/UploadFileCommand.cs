using FTP_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

        public override void Execute(object parameter)
        {
            var ftpServerUrl = _mainViewModel.FtpConnectionSettings.ServerAddress + _mainViewModel.CurrentPathServer + @"/" + _mainViewModel.SelectedFileItemLocal.FileName;

            var filePath = _mainViewModel.CurrentPathLocal + @"\" + _mainViewModel.SelectedFileItemLocal.FileName;

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(ftpServerUrl);
                request.Credentials = new NetworkCredential(_mainViewModel.FtpConnectionSettings.Username, _mainViewModel.FtpConnectionSettings.Password);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (var fileStream = File.OpenRead(filePath))
                using (var ftpStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(ftpStream);
                }

                _mainViewModel.AddLogMessage("Файл успешно загружен на FTP-сервер.", Brushes.Green);

                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;

                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    _mainViewModel.AddLogMessage($"Ошибка при попытке загрузить файл на сервер: {response.StatusDescription}", Brushes.Orange);
                else
                    _mainViewModel.AddLogMessage($"Ошибка при создании папки на FTP сервере: {response.StatusDescription}", Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage($"Ошибка при создании папки на FTP сервере:" + ex.Message, Brushes.Red);
            }
        }
    }
}