using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Windows.Media;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class DeleteFileCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public DeleteFileCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Удалить файл";

        public override void Execute(object? parameter)
        {
            if (parameter as string == "local")
                DeleteLocal();
            else if (parameter as string == "server")
                DeleteOnFtpServer();
        }

        private void DeleteLocal()
        {
            if (_mainViewModel.SelectedFileItemLocal.FileType == "Folder" && Directory.Exists(_mainViewModel.GetFilePath))
            {
                Directory.Delete(_mainViewModel.GetFilePath, true);
                _mainViewModel.AddLogMessage($"Папка {_mainViewModel.SelectedFileItemLocal.FileName} успешно удалена.", Brushes.Green);
            }
            else
            {
                File.Delete(_mainViewModel.GetFilePath);
                _mainViewModel.AddLogMessage($"Файл {_mainViewModel.SelectedFileItemLocal.FileName} успешно удален.", Brushes.Green);
            }

            _mainViewModel.NavigateToFolder(_mainViewModel.CurrentPathLocal);
        }

        private void DeleteOnFtpServer()
        {
            try
            {
                var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.GetFilePatnOnFTP);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                using var response = (FtpWebResponse)request.GetResponse();
                
                _mainViewModel.AddLogMessage($"Удаление файла завершено: {response.StatusDescription}", Brushes.Green);
                _mainViewModel.LoadFolder(_mainViewModel.CurrentPathServer);
                response.Close();
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage($"Ошибка при удалении папки на FTP сервере: {response?.StatusDescription}", Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при удалении папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}