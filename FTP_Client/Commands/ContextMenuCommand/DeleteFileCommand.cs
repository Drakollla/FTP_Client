using FTP_Client.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace FTP_Client.Commands.ContextMenuCommand
{
    //todo:удаление файла
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
                //request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                _mainViewModel.AddLogItem($"Удаление файла завершено: {response.StatusDescription}");

                response.Close();
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;

                _mainViewModel.AddLogItem("Ошибка при удалении папки на FTP сервере: " + ex.Message);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogItem("Ошибка при удалении папки на FTP сервере: " + ex.Message);
            }
        }
    }
}
