﻿using FTP_Client.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Windows;
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

        public async override void Execute(object? parameter)
        {
            var ftpFolder = "/" + _mainViewModel.FolderName;
            var ftpPath = _mainViewModel.CurrentPathServer + ftpFolder;

            try
            {
                var request = _mainViewModel.FtpConnectionSettings.CreateFtpRequest(ftpPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                var response = (FtpWebResponse)request.GetResponse();
                response.Close();

                _mainViewModel.AddLogMessage($"Папка успешно создана на FTP сервере: {response.StatusDescription}", Brushes.Green);
                await _mainViewModel.LoadFolderAsync(_mainViewModel.CurrentPathServer);

                var topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                topWindow?.Close();
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;
                if (response?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    _mainViewModel.AddLogMessage($"Папка уже существует на FTP сервере:  {response?.StatusDescription}", Brushes.Orange);
                else
                    _mainViewModel.AddLogMessage($"Ошибка при создании папки на FTP сервере:  {response?.StatusDescription}" + ex.Message, Brushes.Red);
            }
            catch (Exception ex)
            {
                _mainViewModel.AddLogMessage("Ошибка при создании папки на FTP сервере: " + ex.Message, Brushes.Red);
            }
        }
    }
}