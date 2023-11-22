using FTP_Client.ViewModels;
using FTP_Client.Views;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            string fileName = _mainViewModel.SelectedFileItemServer.FileName;
            string fileExtension = Path.GetExtension(fileName);

            if (!string.IsNullOrEmpty(fileExtension))
            {
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string[] textExtensions = { ".txt", ".doc", ".docx", ".pdf" };

                if (imageExtensions.Contains(fileExtension.ToLower()))
                {
                    DownloadImageFromFtp(
                             _mainViewModel.FtpConnectionSettings.ServerAddress,
                    _mainViewModel.FtpConnectionSettings.Username,
                    _mainViewModel.FtpConnectionSettings.Password,
                    _mainViewModel.SelectedFileItemServer.FileName
                    );


                }
                else if (textExtensions.Contains(fileExtension.ToLower()))
                {

                    _mainViewModel.TxtFileContent = DownloadAndReadTextFileFromFtp(
                        _mainViewModel.FtpConnectionSettings.ServerAddress,
                        _mainViewModel.FtpConnectionSettings.Username,
                        _mainViewModel.FtpConnectionSettings.Password,
                        _mainViewModel.SelectedFileItemServer.FileName
                        );

                    _mainViewModel.NewFileName = _mainViewModel.SelectedFileItemServer.FileName;
                }
                else
                {
                    _mainViewModel.AddLogMessage($"Невозможно просмотреть: {_mainViewModel.SelectedFileItemServer.FileName}", Brushes.Orange);
                }
            }

            var readFileDialog = new ReadFileDialog()
            {
                DataContext = _mainViewModel,
                Owner = Application.Current.MainWindow,
            };



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

        public void DownloadImageFromFtp(string serverUri, string username, string password, string remoteImagePath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri + _mainViewModel.CurrentPathServer + remoteImagePath);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = responseStream;
                        image.EndInit();

                        _mainViewModel.ImageSource = new();

                        _mainViewModel.ImageSource = image;
                    }
                }
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage($"Ошибка загрузки изображения: {ex.Message}", Brushes.Red);
            }
        }
    }
}