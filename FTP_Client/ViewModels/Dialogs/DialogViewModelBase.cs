using FTP_Client.Models;
using System.Windows.Input;

namespace FTP_Client.ViewModels.Dialogs
{
    public abstract class DialogViewModelBase : ObservableObject
    {
        public bool? DialogResult { get; protected set; }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        protected DialogViewModelBase()
        {
            OkCommand = new RelayCommand(_ => OnOk());
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        protected virtual void OnOk() => Close(true);
        protected virtual void OnCancel() => Close(false);

        protected void Close(bool? result)
        {
            DialogResult = result;
            OnPropertyChanged(nameof(DialogResult));
        }
    }
}