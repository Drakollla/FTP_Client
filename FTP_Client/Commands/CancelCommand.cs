using FTP_Client.ViewModels;
using System.Linq;
using System.Windows;

namespace FTP_Client.Commands
{
    public class CancelCommand : BaseCommand
    {
        private readonly MainViewModel _mainViewModel;

        public CancelCommand(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public override void Execute(object parameter)
        {
            Window? topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            topWindow?.Close();
        }
    }
}