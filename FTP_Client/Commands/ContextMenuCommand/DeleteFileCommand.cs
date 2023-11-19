using FTP_Client.ViewModels;
using System;
using System.Net;
using System.Windows.Media;

namespace FTP_Client.Commands.ContextMenuCommand
{
    //todo:удаление папки
    public class DeleteFileCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public DeleteFileCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Удалить файл";

        public override void Execute(object parameter)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_mainViewModel.FtpConnectionSettings.ServerAddress + _mainViewModel.CurrentPathServer + _mainViewModel.SelectedFileItemServer.FileName);
                request.Credentials = new NetworkCredential(_mainViewModel.FtpConnectionSettings.Username, _mainViewModel.FtpConnectionSettings.Password);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                _mainViewModel.AddLogMessage($"Удаление файла завершено: {response.StatusDescription}", Brushes.Green);

                response.Close();
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                _mainViewModel.AddLogMessage("Ошибка при удалении папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при удалении папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}