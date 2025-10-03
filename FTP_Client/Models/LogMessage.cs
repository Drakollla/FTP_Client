using FTP_Client.Enums;
using System;

namespace FTP_Client.Models
{
    public class LogMessage
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public string Text { get; set; }
        public LogLevel Level { get; set; }
    }
}