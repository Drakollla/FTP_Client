using System;

namespace FTP_Client.Models
{
    public class FileItem : ObservableObject
    {
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        private long _size;
        public long Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        private DateTime? _lastModified;
        public DateTime? LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        private string _fileType;
        public string FileType
        {
            get => _fileType;
            set => SetProperty(ref _fileType, value);
        }
    }
}