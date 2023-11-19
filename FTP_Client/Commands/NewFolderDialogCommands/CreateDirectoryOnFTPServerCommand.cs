using FTP_Client.ViewModels;
using System;
using System.Net;
using System.Windows.Media;

namespace FTP_Client.Commands.NewFolderDialogCommands
{
    public class CreateDirectoryOnFTPServerCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public CreateDirectoryOnFTPServerCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object parameter)
        {
            try
            {
                var ftpFolder = "/" + _mainViewModel.FolderName;
                var ftpPath = _mainViewModel.FtpConnectionSettings.ServerAddress + _mainViewModel.CurrentPathServer + ftpFolder;

                var request = (FtpWebRequest)WebRequest.Create(ftpPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(_mainViewModel.FtpConnectionSettings.Username, _mainViewModel.FtpConnectionSettings.Password);
                var response = (FtpWebResponse)request.GetResponse();
                response.Close();

                _mainViewModel.AddLogMessage($"Папка успешно создана на FTP сервере: {response.StatusDescription}", Brushes.Green);
                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;

                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    _mainViewModel.AddLogMessage("Папка уже существует на FTP сервере", Brushes.Orange);
                else
                    _mainViewModel.AddLogMessage("Ошибка при создании папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при создании папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}