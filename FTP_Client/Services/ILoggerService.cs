using FTP_Client.Enums;
using FTP_Client.Models;
using System.Collections.ObjectModel;

namespace FTP_Client.Services
{
    public interface ILoggerService
    {
        ObservableCollection<LogMessage> Messages { get; }

        void Log(string message, LogLevel level);
    }
}