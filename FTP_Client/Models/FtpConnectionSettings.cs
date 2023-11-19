namespace FTP_Client.Models
{
    public class FtpConnectionSettings : ObservableObject
    {
        private string _serverAddress = "ftp://127.0.0.1";
        public string ServerAddress
        {
            get => _serverAddress;
            set => SetProperty(ref _serverAddress, value);
        }

        private int _port;
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        private string _userName = "test";
        public string Username
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _password = "BeloJek";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
    }
}