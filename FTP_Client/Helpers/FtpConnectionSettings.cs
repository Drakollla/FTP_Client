using FTP_Client.Models;

namespace FTP_Client.Helpers
{
    public class FtpConnectionSettings : ObservableObject
    {
        private string _host;
        private int _port = 21;
        private string _userName;
        private string _password;

        public string Host
        {
            get => _host;
            set => SetProperty(ref _host, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string Username
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
    }
}