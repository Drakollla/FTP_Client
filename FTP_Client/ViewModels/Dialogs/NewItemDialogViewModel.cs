namespace FTP_Client.ViewModels.Dialogs
{
    public class NewItemDialogViewModel : DialogViewModelBase
    {
        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        protected override void OnOk()
        {
            if (string.IsNullOrWhiteSpace(ItemName))
            {
                // Здесь можно показать сообщение об ошибке
                return;
            }
            base.OnOk();
        }
    }
}
