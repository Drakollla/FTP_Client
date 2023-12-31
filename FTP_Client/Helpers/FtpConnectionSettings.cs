﻿using FTP_Client.Models;
using System;
using System.Net;

namespace FTP_Client.Helpers
{
    public class FtpConnectionSettings : ObservableObject
    {
        private string _serverAddress = "127.0.0.1";
        public string ServerAddress
        {
            get => _serverAddress;
            set => SetProperty(ref _serverAddress, value);
        }

        private int _port = 21;
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

        public FtpWebRequest CreateFtpRequest(string stringUriRequest)
        {
            var uriBuilder = new UriBuilder("ftp", ServerAddress, Port);

            var request = (FtpWebRequest)WebRequest.Create(uriBuilder.ToString() + stringUriRequest);
            request.Credentials = new NetworkCredential(Username, Password);


            return request;
        }
    }
}