using FTP_Client.Helpers;
using System.Net;

namespace TestProject
{
    public class FtpClient
    {
        public bool IsConnected { get; private set; }

        public void Connect(FtpConnectionSettings connectionSettings)
        {
            var request = connectionSettings.CreateFtpRequest("/");
            IsConnected = true;
        }



        public string GetLastServerResponse()
        {
            return "220 FTP server ready";
        }
    }
}
