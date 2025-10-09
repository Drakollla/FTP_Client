using FTP_Client.Helpers;
using FTP_Client.Models;
using System.Threading.Tasks;

namespace FTP_Client.Services
{
    public interface IFileEditorService
    {
        Task EditRemoteFileAsync(FileItem fileToEdit, string currentRemotePath, FtpConnectionSettings ftpSettings);
    }
}