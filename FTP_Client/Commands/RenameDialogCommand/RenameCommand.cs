﻿using FTP_Client.ViewModels;
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
            RenameOnFtpServer();
        }

        private void RenameOnFtpServer()
        {
            var requestUriString = _mainViewModel.FtpConnectionSettings.ServerAddress + _mainViewModel.CurrentPathServer + _mainViewModel.SelectedFileItemServer.FileName;

            try
            {
                var request = (FtpWebRequest)WebRequest.Create(requestUriString);
                request.Credentials = new NetworkCredential(_mainViewModel.FtpConnectionSettings.Username, _mainViewModel.FtpConnectionSettings.Password);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = _mainViewModel.NewFileName;

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