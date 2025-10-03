using FTP_Client.Models;
using System.Collections.Generic;

namespace FTP_Client.Services
{
    public interface ILocalFileService
    {
        IEnumerable<FileItem> GetDrives();

        IEnumerable<FileItem> GetDirectoryListing(string folderPath);
        
        void DeleteItem(string path);
        
        void CreateDirectory(string path);
        
        void RenameItem(string oldPath, string newPath);
    }
}