using FTP_Client.ViewModels;
using System.Linq;
using System.Windows;

namespace FTP_Client.Commands
{
    public class CancelCommand : BaseCommand
    {
        public override void Execute(object? parameter)
        {
            Window? topWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            topWindow?.Close();
        }
    }
}