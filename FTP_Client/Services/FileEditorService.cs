using FTP_Client.Enums;
using FTP_Client.Helpers;
using FTP_Client.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FTP_Client.Services
{
    public class FileEditorService : IFileEditorService
    {
        private readonly IFtpService _ftpService;
        private readonly ILoggerService _logger;

        public FileEditorService(IFtpService ftpService, ILoggerService logger)
        {
            _ftpService = ftpService;
            _logger = logger;
        }

        public async Task EditRemoteFileAsync(FileItem fileToEdit, string currentRemotePath, FtpConnectionSettings ftpSettings)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{fileToEdit.FileName}");
            string remoteFilePath = Path.Combine(currentRemotePath, fileToEdit.FileName).Replace('\\', '/');

            try
            {
                _logger.Log($"Скачивание '{fileToEdit.FileName}' для редактирования...", LogLevel.Info);
                await _ftpService.DownloadFileAsync(remoteFilePath, Path.GetTempPath(), ftpSettings);

                File.Move(Path.Combine(Path.GetTempPath(), fileToEdit.FileName), tempFilePath);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(tempFilePath) { UseShellExecute = true }
                };
                process.Start();

                _logger.Log($"Файл открыт в редакторе. Ожидание сохранения...", LogLevel.Info);

                await WatchForFileChangesAndUpload(tempFilePath, remoteFilePath, ftpSettings);

            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при редактировании файла: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        private async Task WatchForFileChangesAndUpload(string localPath, string remotePath, FtpConnectionSettings settings)
        {
            using (var watcher = new FileSystemWatcher(Path.GetDirectoryName(localPath)))
            {
                watcher.Filter = Path.GetFileName(localPath);
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                var tcs = new TaskCompletionSource<bool>();

                FileSystemEventHandler onFileChanged = (sender, e) =>
                {
                    tcs.TrySetResult(true);
                };

                watcher.Changed += onFileChanged;
                watcher.EnableRaisingEvents = true;

                await tcs.Task;

                watcher.EnableRaisingEvents = false;
                watcher.Changed -= onFileChanged;

                _logger.Log("Изменения сохранены. Загрузка файла на сервер...", LogLevel.Info);

                await _ftpService.UploadFileAsync(localPath, Path.GetDirectoryName(remotePath).Replace('\\', '/'), settings);
                _logger.Log("Файл успешно обновлен на сервере.", LogLevel.Success);
            }
        }
    }
}