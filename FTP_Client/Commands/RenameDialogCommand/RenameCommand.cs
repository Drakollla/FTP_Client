using FTP_Client.ViewModels;
using System;
using System.Net;
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

        public override void Execute(object parameter)
        {
            RenameFileOnFtp(_mainViewModel.FtpConnectionSettings.ServerAddress,
              _mainViewModel.FtpConnectionSettings.Username,
              _mainViewModel.FtpConnectionSettings.Password,
              _mainViewModel.SelectedFileItemServer.FileName,
              _mainViewModel.NewName
              );
        }

        public void RenameFileOnFtp(string serverUri, string username, string password, string currentFileName, string newFileName)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri + _mainViewModel.CurrentPathServer + currentFileName);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = newFileName;

                using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                _mainViewModel.AddLogMessage($"Файл успешно переименован: {response.StatusDescription}", Brushes.Green);

                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;

                _mainViewModel.AddLogMessage("Ошибка при попытке переименовать файла/папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при попытке переименовать файла/папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}