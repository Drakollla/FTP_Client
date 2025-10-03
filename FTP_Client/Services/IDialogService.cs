namespace FTP_Client.Services
{
    public interface IDialogService
    {
        void ShowMessage(string title, string message);

        bool ShowConfirmation(string title, string message);

        string ShowRenameDialog(string currentName);

        string ShowNewItemDialog(string title, string message, string defaultName = "");
    }
}