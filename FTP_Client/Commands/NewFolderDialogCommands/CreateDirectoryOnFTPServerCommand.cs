using FTP_Client.ViewModels;
using System;
using System.Net;

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

                _mainViewModel.AddLogItem("Папка успешно создана на FTP сервере");
                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;

                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    _mainViewModel.AddLogItem("Папка уже существует на FTP сервере");
                else
                    _mainViewModel.AddLogItem("Ошибка при создании папки на FTP сервере: " + ex.Message);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogItem("Ошибка при создании папки на FTP сервере: " + ex.Message);
            }
        }
    }
}