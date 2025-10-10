using FTP_Client.Helpers;
using FTP_Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FTP_Client.Services
{
    public interface IFtpService
    {
        Task<IEnumerable<FileItem>> GetDirectoryListingAsync(string remotePath, FtpConnectionSettings settings);

        Task<long> GetFileSizeAsync(string remoteFilePath, FtpConnectionSettings settings);

        Task CreateDirectoryAsync(string remotePath, FtpConnectionSettings settings);
        
        Task RenameAsync(string oldRemotePath, string newRemotePath, FtpConnectionSettings settings);

        Task DeleteFileAsync(string remotePath, FtpConnectionSettings settings);

        Task DeleteDirectoryAsync(string remotePath, FtpConnectionSettings settings);

        Task DownloadFileAsync(string remoteFilePath, string localDirectory, FtpConnectionSettings settings);

        Task DownloadFileAsync(string remotePath, string localPath, FtpConnectionSettings settings, IProgress<double> progress);

        Task DownloadDirectoryAsync(string remoteDirectoryPath, string localParentDirectoryPath, FtpConnectionSettings settings);

        Task DownloadDirectoryAsync(string localDirectoryPath, string remoteParentDirectoryPath, FtpConnectionSettings settings, IProgress<double> progress);

        Task UploadFileAsync(string localFilePath, string remoteDirectory, FtpConnectionSettings settings);

        Task UploadFileAsync(string localPath, string remotePath, FtpConnectionSettings settings, IProgress<double> progress);

        Task UploadDirectoryAsync(string localDirectoryPath, string remoteParentDirectoryPath, FtpConnectionSettings settings);

        Task UploadDirectoryAsync(string localDirectoryPath, string remoteParentDirectoryPath, FtpConnectionSettings settings, IProgress<double> progress);
    }
}