using FTP_Client.ViewModels;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public override void Execute(object parameter)
        {
            DownloadFileFromFtp(
                _mainViewModel.FtpConnectionSettings.ServerAddress,
                _mainViewModel.FtpConnectionSettings.Username,
                _mainViewModel.FtpConnectionSettings.Password,
                _mainViewModel.CurrentPathServer + _mainViewModel.SelectedFileItemServer.FileName,
                _mainViewModel.CurrentPathLocal);
        }

        public void DownloadFileFromFtp(string serverUri, string username, string password, string remoteFilePath, string localFolderPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri + remoteFilePath);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        var localFilePath = localFolderPath + @"\" + _mainViewModel.SelectedFileItemServer.FileName;

                        using (FileStream fileStream = new FileStream(localFilePath, FileMode.CreateNew))
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }

                    _mainViewModel.AddLogMessage($"Файл скачан: {response.StatusDescription}", Brushes.Green);
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    _mainViewModel.AddLogMessage("Ошибка: Файл недоступен на FTP сервере" + ex.Message, Brushes.Red);
            }
        }
    

    }
}