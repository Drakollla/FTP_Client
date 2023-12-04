using FTP_Client.Helpers;
using FTP_Client.ViewModels;

namespace TestProject
{
    public class UnitTest1
    {
        [Fact]
        public void TestConnectionToFTP()
        {
            FtpConnectionSettings connectionSettings = new FtpConnectionSettings
            {
                ServerAddress = "127.0.0.1",
                Port = 21,
                Username = "test",
                Password = "BeloJek"
            };

            FtpClient ftpClient = new FtpClient();

            ftpClient.Connect(connectionSettings);
            string response = ftpClient.GetLastServerResponse();

            Assert.True(ftpClient.IsConnected);
            Assert.NotNull(response);
            Assert.NotEmpty(response);
        }


        [Fact]
        public void TestLoadFolder()
        {
            var viewModel = new MainViewModel();
            string folderPath = "/File";
            viewModel.LoadFolder(folderPath);
            Assert.True(viewModel.FilesAndFoldersServer.Count > 0);
        }
    }
}