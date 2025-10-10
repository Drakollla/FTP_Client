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
        #region File Operations

        public async Task DownloadFileAsync(string remoteFilePath, string localDirectory, FtpConnectionSettings settings)
        {
            await DownloadFileAsync(remoteFilePath, localDirectory, settings, null);
        }

        public async Task DownloadFileAsync(string remoteFilePath, string localDirectory, FtpConnectionSettings settings, IProgress<double> progress)
        {
            long fileSize = await GetFileSizeAsync(remoteFilePath, settings);
            var fileName = Path.GetFileName(remoteFilePath);
            var localFilePath = Path.Combine(localDirectory, fileName);
            var request = CreateFtpRequest(remoteFilePath, settings);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            const int bufferSize = 4096;
            var buffer = new byte[bufferSize];
            long totalBytesReceived = 0;

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            using (var fileStream = File.Create(localFilePath))
            {
                int readBytes;

                while ((readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, readBytes);
                    
                    totalBytesReceived += readBytes;

                    if (fileSize > 0)
                    {
                        var percentage = (double)totalBytesReceived * 100.0 / fileSize;
                        progress?.Report(percentage);
                    }
                }
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
                    if (response.StatusCode != FtpStatusCode.FileActionOK)
                        throw new WebException($"Сервер вернул неожиданный статус: {response.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;

                if (response != null && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    throw new FileNotFoundException($"Файл не найден на FTP сервере: {remotePath}", ex);

                throw;
            }
        }

        public async Task UploadFileAsync(string localFilePath, string remoteDirectory, FtpConnectionSettings settings)
        {
            var fileName = Path.GetFileName(localFilePath);
            var remoteFilePath = Path.Combine(remoteDirectory, fileName).Replace('\\', '/');

            await UploadFileAsync(localFilePath, remoteFilePath, settings, null);
        }

        public async Task UploadFileAsync(string localFilePath, string remoteDirectory, FtpConnectionSettings settings, IProgress<double> progress)
        {
            var fileInfo = new FileInfo(localFilePath);
            var fileName = Path.GetFileName(localFilePath);
            var request = CreateFtpRequest(remoteDirectory, settings);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.ContentLength = fileInfo.Length;
            const int bufferSize = 4096;
            var buffer = new byte[bufferSize];
            long totalBytesSent = 0;

            using (var fileStream = fileInfo.OpenRead())
            using (var requestStream = await request.GetRequestStreamAsync())
            {
                int readBytes;

                while ((readBytes = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await requestStream.WriteAsync(buffer, 0, readBytes);
                
                    totalBytesSent += readBytes;
                    var percentage = (double)totalBytesSent * 100.0 / fileInfo.Length;
                    progress?.Report(percentage);
                }
            }
        }

        #endregion

        #region Directory Operations

        public async Task<IEnumerable<FileItem>> GetDirectoryListingAsync(string remotePath, FtpConnectionSettings settings)
        {
            var request = CreateFtpRequest(Path.Combine(remotePath, ""), settings);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            var fileItems = new List<FileItem>();

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var fileItem = ParseFtpLine(line);

                    if (fileItem != null)
                        fileItems.Add(fileItem);
                }
            }

            return fileItems;
        }

        public async Task CreateDirectoryAsync(string remotePath, FtpConnectionSettings settings)
        {
            var request = CreateFtpRequest(remotePath, settings);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                if (response.StatusCode != FtpStatusCode.PathnameCreated)
                    throw new WebException($"Сервер вернул ошибку при создании папки: {response.StatusDescription}");
            }
        }

        public async Task DeleteDirectoryAsync(string remotePath, FtpConnectionSettings settings)
        {
            try
            {
                var request = CreateFtpRequest(remotePath, settings);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                using (var response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode != FtpStatusCode.FileActionOK)
                        throw new WebException($"Сервер вернул неожиданный статус: {response.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as FtpWebResponse;

                if (response != null)
                {
                    if (response.StatusDescription.Contains("550") && (response.StatusDescription.ToLower().Contains("not empty") || response.StatusDescription.ToLower().Contains("не пуста")))
                        throw new IOException($"Не удалось удалить директорию '{remotePath}', так как в ней содержатся данные.", ex);
                }

                throw;
            }
        }

        public async Task DownloadDirectoryAsync(string remoteDirectoryPath, string localParentDirectoryPath, FtpConnectionSettings settings)
        {
            await DownloadDirectoryAsync(remoteDirectoryPath, localParentDirectoryPath, settings, null);
        }

        public async Task DownloadDirectoryAsync(string remoteDirectoryPath, string localParentDirectoryPath, FtpConnectionSettings settings, IProgress<double> progress)
        {
            long totalDirectorySize = await GetRemoteDirectorySizeAsync(remoteDirectoryPath, settings);

            var tracker = new TransferProgressTracker(totalDirectorySize);

            var dirName = Path.GetFileName(remoteDirectoryPath.TrimEnd('/'));
            var localDirectoryPath = Path.Combine(localParentDirectoryPath, dirName);
            Directory.CreateDirectory(localDirectoryPath);

            await DownloadDirectoryRecursiveAsync(remoteDirectoryPath, localDirectoryPath, settings, progress, tracker);
        }

        private async Task DownloadDirectoryRecursiveAsync(string remoteDirPath, string localDirPath, FtpConnectionSettings settings, IProgress<double> overallProgress, TransferProgressTracker tracker)
        {
            var items = await GetDirectoryListingAsync(remoteDirPath, settings);

            foreach (var item in items)
            {
                var remoteItemPath = Path.Combine(remoteDirPath, item.FileName).Replace('\\', '/');
                var localItemPath = Path.Combine(localDirPath, item.FileName);

                if (item.IsDirectory)
                {
                    Directory.CreateDirectory(localItemPath);
                    await DownloadDirectoryRecursiveAsync(remoteItemPath, localItemPath, settings, overallProgress, tracker);
                }
                else
                {
                    await DownloadFileAsync(remoteItemPath, localDirPath, settings);

                    tracker.TransferredBytes += item.Size;

                    if (tracker.TotalSize > 0)
                    {
                        var overallPercentage = (double)tracker.TransferredBytes * 100.0 / tracker.TotalSize;
                        overallProgress?.Report(overallPercentage);
                    }
                }
            }
        }

        private async Task DownloadDirectoryRecursiveAsync(string remoteDirPath, string localDirPath, FtpConnectionSettings settings)
        {
            var items = await GetDirectoryListingAsync(remoteDirPath, settings);

            foreach (var item in items)
            {
                var remoteItemPath = Path.Combine(remoteDirPath, item.FileName).Replace('\\', '/');
                var localItemPath = Path.Combine(localDirPath, item.FileName);

                if (item.IsDirectory)
                {
                    Directory.CreateDirectory(localItemPath);
                    await DownloadDirectoryRecursiveAsync(remoteItemPath, localItemPath, settings);
                }
                else await DownloadFileAsync(remoteItemPath, localDirPath, settings);
            }
        }

        public async Task UploadDirectoryAsync(string localDirectoryPath, string remoteParentDirectoryPath, FtpConnectionSettings settings)
        {
            await UploadDirectoryAsync(localDirectoryPath, remoteParentDirectoryPath, settings, null);
        }

        public async Task UploadDirectoryAsync(string localDirectoryPath, string remoteParentDirectoryPath, FtpConnectionSettings settings, IProgress<double> progress)
        {
            long totalDirectorySize = GetLocalDirectorySize(localDirectoryPath);
            var tracker = new TransferProgressTracker(totalDirectorySize);

            var dirName = new DirectoryInfo(localDirectoryPath).Name;
            var remoteDirectoryPath = Path.Combine(remoteParentDirectoryPath, dirName).Replace('\\', '/');

            try { await CreateDirectoryAsync(remoteDirectoryPath, settings); } catch (WebException) { }

            await UploadDirectoryRecursiveAsync(localDirectoryPath, remoteDirectoryPath, settings, progress, tracker);
        }

        private async Task UploadDirectoryRecursiveAsync(string localDirPath, string remoteDirPath, FtpConnectionSettings settings, IProgress<double> overallProgress, TransferProgressTracker tracker)
        {
            var entries = Directory.GetFileSystemEntries(localDirPath);

            foreach (var entry in entries)
            {
                var entryName = Path.GetFileName(entry);
                var remoteItemPath = Path.Combine(remoteDirPath, entryName).Replace('\\', '/');

                if (Directory.Exists(entry))
                {
                    try
                    {
                        await CreateDirectoryAsync(remoteItemPath, settings);
                    }
                    catch (WebException) { }

                    await UploadDirectoryRecursiveAsync(entry, remoteItemPath, settings, overallProgress, tracker);
                }
                else
                {
                    var fileInfo = new FileInfo(entry);
                    await UploadFileAsync(entry, remoteDirPath, settings);

                    tracker.TransferredBytes += fileInfo.Length;

                    if (tracker.TotalSize > 0)
                    {
                        var overallPercentage = (double)tracker.TransferredBytes * 100.0 / tracker.TotalSize;
                        overallProgress?.Report(overallPercentage);
                    }
                }
            }
        }

        private async Task UploadDirectoryRecursiveAsync(string localDirPath, string remoteDirPath, FtpConnectionSettings settings)
        {
            var entries = Directory.GetFileSystemEntries(localDirPath);

            foreach (var entry in entries)
            {
                var entryName = Path.GetFileName(entry);
                var remoteItemPath = Path.Combine(remoteDirPath, entryName).Replace('\\', '/');

                if (Directory.Exists(entry))
                {
                    try
                    {
                        await CreateDirectoryAsync(remoteItemPath, settings);
                    }
                    catch (WebException ex) when ((ex.Response as FtpWebResponse)?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) { }

                    await UploadDirectoryRecursiveAsync(entry, remoteItemPath, settings);
                }
                else await UploadFileAsync(entry, remoteDirPath, settings);
            }
        }

        #endregion

        #region General Operations

        public async Task RenameAsync(string oldRemotePath, string newRemotePath, FtpConnectionSettings settings)
        {
            var request = CreateFtpRequest(oldRemotePath, settings);
            request.Method = WebRequestMethods.Ftp.Rename;
            request.RenameTo = Path.GetFileName(newRemotePath);

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                if (response.StatusCode != FtpStatusCode.FileActionOK)
                    throw new WebException($"Сервер вернул ошибку при переименовании: {response.StatusDescription}");
            }
        }

        private async Task<long> GetRemoteDirectorySizeAsync(string remoteDirPath, FtpConnectionSettings settings)
        {
            long totalSize = 0;
            var items = await GetDirectoryListingAsync(remoteDirPath, settings);

            foreach (var item in items)
            {
                if (item.IsDirectory)
                {
                    var remoteItemPath = Path.Combine(remoteDirPath, item.FileName).Replace('\\', '/');
                    totalSize += await GetRemoteDirectorySizeAsync(remoteItemPath, settings);
                }
                else totalSize += item.Size;
            }

            return totalSize;
        }

        private long GetLocalDirectorySize(string localDirPath)
        {
            long totalSize = 0;
            var directoryInfo = new DirectoryInfo(localDirPath);

            foreach (var file in directoryInfo.GetFiles())
                totalSize += file.Length;

            foreach (var dir in directoryInfo.GetDirectories())
                totalSize += GetLocalDirectorySize(dir.FullName);

            return totalSize;
        }

        public async Task<long> GetFileSizeAsync(string remoteFilePath, FtpConnectionSettings settings)
        {
            var request = CreateFtpRequest(remoteFilePath, settings);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                return response.ContentLength;
            }
        }
        #endregion

        #region Private Helper Methods
        private FtpWebRequest CreateFtpRequest(string path, FtpConnectionSettings settings)
        {
            if (!Uri.TryCreate($"ftp://{settings.Host}/{path.TrimStart('/')}", UriKind.Absolute, out var uri))
                throw new ArgumentException("Не удалось сформировать корректный FTP URL.");

            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(settings.Username, settings.Password);
            request.UseBinary = true;
            request.UsePassive = true;

            return request;
        }

        private FileItem ParseFtpLine(string line)
        {
            try
            {
                string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length < 9)
                    return null;

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
            var dateStr = $"{day} {month} {(timeOrYear.Contains(":") ? DateTime.Now.Year.ToString() : timeOrYear)}";
            DateTime.TryParseExact(dateStr, "d MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
            return result;
        }

        #endregion
    }
}