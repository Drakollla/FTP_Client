using FTP_Client.ViewModels;
using FTP_Client.Views;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace FTP_Client.Commands.ContextMenuCommand
{
    public class ViewFileCommand : MenuContextBaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public ViewFileCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override string CommandName => "Просмотр";

        public override void Execute(object parameter)
        {
            _mainViewModel.TxtFileContent = DownloadAndReadTextFileFromFtp(
                _mainViewModel.FtpConnectionSettings.ServerAddress,
                _mainViewModel.FtpConnectionSettings.Username,
                _mainViewModel.FtpConnectionSettings.Password,
                _mainViewModel.SelectedFileItemServer.FileName
                );

            var readFileDialog = new ReadFileDialog()
            {
                DataContext = _mainViewModel,
                Owner = Application.Current.MainWindow,
            };

            _mainViewModel.NewName = _mainViewModel.SelectedFileItemServer.FileName;

            if (readFileDialog.ShowDialog() == true)
            {
            }
        }

        public string DownloadAndReadTextFileFromFtp(string serverUri, string username, string password, string filePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri + _mainViewModel.CurrentPathServer + filePath);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                _mainViewModel.AddLogMessage("Ошибка при попытке просмотреть содержимое файла на FTP сервере: " + ex.Message, Brushes.Red);
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при попытке просмотреть содержимое файла на FTP сервере: " + ex.Message, Brushes.Red);
                
                return string.Empty;
            }
        }
    }
}