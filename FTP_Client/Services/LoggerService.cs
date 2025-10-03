using FTP_Client.Enums;
using FTP_Client.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace FTP_Client.Services
{
    public class LoggerService : ILoggerService
    {
        public ObservableCollection<LogMessage> Messages { get; } = new();

        public void Log(string message, LogLevel level)
        {
            var logMessage = new LogMessage
            {
                Text = message,
                Level = level
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(logMessage);
            });
        }
    }
}