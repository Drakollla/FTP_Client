using FTP_Client.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
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

        public override string CommandName => "Удалить";

        public override void Execute(object? parameter)
        {
            if (parameter as string == "local")
                DeleteLocal();
            else if (parameter as string == "server")
            {
                if (_mainViewModel.SelectedFileItemServer.FileType == "Folder")
                    DeleteDirectoryOnFtpServer();
                else
                    DeleteFileOnFtpServer();
            }
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

        private async void DeleteFileOnFtpServer()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.GetFilePatnOnFTP);
                    request.Method = WebRequestMethods.Ftp.DeleteFile;

                    using var response = (FtpWebResponse)request.GetResponse();

                    await Application.Current.Dispatcher.InvokeAsync(async() =>
                    {
                        _mainViewModel.AddLogMessage($"Удаление файла {_mainViewModel.SelectedFileItemServer.FileName} завершено: {response.StatusDescription}", Brushes.Green);
                        await _mainViewModel.LoadFolderAsync(_mainViewModel.CurrentPathServer);
                    });
                });
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage($"Ошибка при удалении папки {_mainViewModel.SelectedFileItemServer.FileName} на FTP сервере: {response?.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage($"Ошибка при удалении папки {_mainViewModel.SelectedFileItemServer.FileName} на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }

        private async void DeleteDirectoryOnFtpServer()
        {
            try
            {
                var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(_mainViewModel.CurrentPathServer + _mainViewModel.SelectedFileItemServer.FileName);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                using var response = (FtpWebResponse)request.GetResponse();

                _mainViewModel.AddLogMessage($"Удаление папки {_mainViewModel.SelectedFileItemServer.FileName} завершено: {response.StatusDescription}", Brushes.Green);
                await _mainViewModel.LoadFolderAsync(_mainViewModel.CurrentPathServer);
                response.Close();
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                _mainViewModel.AddLogMessage($"Ошибка при удалении папки {_mainViewModel.SelectedFileItemServer.FileName} на FTP сервере: {response?.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage($"Ошибка при удалении папки {_mainViewModel.SelectedFileItemServer.FileName}  на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}