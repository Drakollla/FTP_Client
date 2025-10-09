using FTP_Client.Helpers;
using FTP_Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FTP_Client.Services
{
    public interface IFtpService
    {
        Task<IEnumerable<FileItem>> GetDirectoryListingAsync(string remotePath, FtpConnectionSettings settings);

        Task UploadFileAsync(string localFilePath, string remoteDirectory, FtpConnectionSettings settings);

        Task DownloadFileAsync(string remoteFilePath, string localDirectory, FtpConnectionSettings settings);

        Task DeleteFileAsync(string remotePath, FtpConnectionSettings settings);

        Task DeleteDirectoryAsync(string remotePath, FtpConnectionSettings settings);

        Task CreateDirectoryAsync(string remotePath, FtpConnectionSettings settings);

        Task RenameAsync(string oldRemotePath, string newRemotePath, FtpConnectionSettings settings);

        Task UploadDirectoryAsync(string localDirectoryPath, string remoteParentDirectoryPath, FtpConnectionSettings settings);
        
        Task DownloadDirectoryAsync(string remoteDirectoryPath, string localParentDirectoryPath, FtpConnectionSettings settings);
    }
}