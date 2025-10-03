using FTP_Client.Enums;
using FTP_Client.Helpers;
using FTP_Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FTP_Client.Services
{
    public class FtpService : IFtpService
    {
        // === ПОЛУЧЕНИЕ СПИСКА ФАЙЛОВ ===
        public async Task<IEnumerable<FileItem>> GetDirectoryListingAsync(string remotePath, FtpConnectionSettings settings)
        {
            var request = CreateFtpRequest(Path.Combine(remotePath, ""), settings); // Path.Combine для корректной сборки URL
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            var fileItems = new List<FileItem>();

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // Логика парсинга вынесена в отдельный приватный метод
                    var fileItem = ParseFtpLine(line);
                    if (fileItem != null)
                    {
                        fileItems.Add(fileItem);
                    }
                }
            }
            return fileItems;
        }

        // === ЗАГРУЗКА ФАЙЛА НА СЕРВЕР ===
        public async Task UploadFileAsync(string localFilePath, string remoteDirectory, FtpConnectionSettings settings)
        {
            var fileName = Path.GetFileName(localFilePath);
            var remoteFilePath = Path.Combine(remoteDirectory, fileName);

            var request = CreateFtpRequest(remoteFilePath, settings);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            using (var fileStream = File.OpenRead(localFilePath))
            using (var requestStream = await request.GetRequestStreamAsync())
            {
                await fileStream.CopyToAsync(requestStream);
            }
        }

        // === СКАЧИВАНИЕ ФАЙЛА С СЕРВЕРА ===
        public async Task DownloadFileAsync(string remoteFilePath, string localDirectory, FtpConnectionSettings settings)
        {
            var fileName = Path.GetFileName(remoteFilePath);
            var localFilePath = Path.Combine(localDirectory, fileName);

            var request = CreateFtpRequest(remoteFilePath, settings);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            using (var fileStream = File.Create(localFilePath))
            {
                await responseStream.CopyToAsync(fileStream);
            }
        }

        public async Task DeleteFileAsync(string remotePath, FtpConnectionSettings settings)
        {
            try
            {
                var request = CreateFtpRequest(remotePath, settings);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    // FTP статус-коды, говорящие об успешном выполнении команды.
                    // 250 File action okay, completed.
                    // Другие коды могут означать, что команда принята, но еще выполняется.
                    // Для простоты проверяем самый частый код успеха.
                    if (response.StatusCode != FtpStatusCode.FileActionOK)
                    {
                        throw new WebException($"Сервер вернул неожиданный статус: {response.StatusDescription}");
                    }
                }
            }
            catch (WebException ex)
            {
                // Перехватываем WebException, чтобы добавить больше контекста
                var response = ex.Response as FtpWebResponse;
                if (response != null && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // Это частая ошибка "Файл не найден", делаем ее более понятной
                    throw new FileNotFoundException($"Файл не найден на FTP сервере: {remotePath}", ex);
                }
                // Пробрасываем другие сетевые ошибки
                throw;
            }
        }

        /// <summary>
        /// Асинхронно удаляет пустую директорию на FTP-сервере.
        /// </summary>
        /// <remarks>
        /// Стандартная команда RMD (RemoveDirectory) у большинства FTP-серверов работает только для пустых папок.
        /// </remarks>
        public async Task DeleteDirectoryAsync(string remotePath, FtpConnectionSettings settings)
        {
            try
            {
                var request = CreateFtpRequest(remotePath, settings);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    // 250 Directory successfully removed.
                    if (response.StatusCode != FtpStatusCode.FileActionOK)
                    {
                        throw new WebException($"Сервер вернул неожиданный статус: {response.StatusDescription}");
                    }
                }
            }
            catch (WebException ex)
            {
                // Добавляем контекст к возможной ошибке
                var response = ex.Response as FtpWebResponse;
                if (response != null)
                {
                    // Даем более понятное сообщение об ошибке, если папка не пуста
                    if (response.StatusDescription.Contains("550") && (response.StatusDescription.ToLower().Contains("not empty") || response.StatusDescription.ToLower().Contains("не пуста")))
                    {
                        throw new IOException($"Не удалось удалить директорию '{remotePath}', так как она не пуста.", ex);
                    }
                }
                // Пробрасываем другие сетевые ошибки
                throw;
            }
        }


        public async Task CreateDirectoryAsync(string remotePath, FtpConnectionSettings settings)
        {
            var request = CreateFtpRequest(remotePath, settings);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                // Проверяем успешный статус
                if (response.StatusCode != FtpStatusCode.PathnameCreated)
                {
                    throw new WebException($"Сервер вернул ошибку при создании папки: {response.StatusDescription}");
                }
            }
        }

        public async Task RenameAsync(string oldRemotePath, string newRemotePath, FtpConnectionSettings settings)
        {
            var request = CreateFtpRequest(oldRemotePath, settings);
            request.Method = WebRequestMethods.Ftp.Rename;
            // ВАЖНО: новое имя передается через свойство RenameTo
            request.RenameTo = Path.GetFileName(newRemotePath);
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                if (response.StatusCode != FtpStatusCode.FileActionOK)
                {
                    throw new WebException($"Сервер вернул ошибку при переименовании: {response.StatusDescription}");
                }
            }
        }


        #region Приватные методы-хелперы

        // Этот метод теперь живет здесь, а не в ViewModel.
        private FtpWebRequest CreateFtpRequest(string path, FtpConnectionSettings settings)
        {
            if (!Uri.TryCreate($"ftp://{settings.Host}/{path.TrimStart('/')}", UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Не удалось сформировать корректный FTP URL.");
            }

            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(settings.Username, settings.Password);

            return request;
        }

        private FileItem ParseFtpLine(string line)
        {
            // Это очень упрощенный парсер, который может не работать на всех FTP-серверах.
            // Для реального проекта лучше использовать библиотеку.
            try
            {
                string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 9) return null; // Некорректная строка

                var permissions = tokens[0];
                var sizeStr = tokens[4];
                var month = tokens[5];
                var day = tokens[6];
                var timeOrYear = tokens[7];
                var name = string.Join(" ", tokens, 8, tokens.Length - 8);

                bool isDirectory = permissions.StartsWith("d");

                return new FileItem
                {
                    FileName = name,
                    Size = isDirectory ? 0 : long.Parse(sizeStr),
                    FileType = isDirectory ? FileItemType.Folder : FileItemType.File,
                    LastModified = ParseFtpDate(month, day, timeOrYear)
                };
            }
            catch
            {
                return null;
            }
        }

        private DateTime ParseFtpDate(string month, string day, string timeOrYear)
        {
            // Очень упрощенный парсер даты
            var dateStr = $"{day} {month} {(timeOrYear.Contains(":") ? DateTime.Now.Year.ToString() : timeOrYear)}";
            DateTime.TryParseExact(dateStr, "d MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
            return result;
        }

        #endregion
    }
}