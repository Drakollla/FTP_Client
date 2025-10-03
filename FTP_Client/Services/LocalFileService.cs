using FTP_Client.Enums;
using FTP_Client.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FTP_Client.Services
{
    public class LocalFileService : ILocalFileService
    {
        public IEnumerable<FileItem> GetDrives()
        {
            return DriveInfo.GetDrives()
                            .Where(drive => drive.IsReady)
                            .Select(drive => new FileItem
                            {
                                FileName = drive.Name,
                                FileType = FileItemType.Drive
                            });
        }

        public IEnumerable<FileItem> GetDirectoryListing(string folderPath)
        {
            var items = new List<FileItem>();

            foreach (var directory in Directory.GetDirectories(folderPath))
            {
                var dirInfo = new DirectoryInfo(directory);
                items.Add(new FileItem
                {
                    FileName = dirInfo.Name,
                    FileType = FileItemType.Folder,
                    LastModified = dirInfo.LastWriteTime
                });
            }

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var fileInfo = new FileInfo(file);
                items.Add(new FileItem
                {
                    FileName = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    LastModified = fileInfo.LastWriteTime,
                    Size = fileInfo.Length
                });
            }

            return items;
        }

        public void DeleteItem(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
            else throw new FileNotFoundException("Файл или папка не найдены.", path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void RenameItem(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
                File.Move(oldPath, newPath);
            else if (Directory.Exists(oldPath))
                Directory.Move(oldPath, newPath);
            else
                throw new FileNotFoundException("Элемент не найден", oldPath);
        }
    }
}