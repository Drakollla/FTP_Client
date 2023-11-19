using FTP_Client.Models;
using System.Windows.Media;

namespace FTP_Client.Helpers
{
    public class LogMessage : ObservableObject
    {
        private string _text;
        public string Text 
        { 
            get => _text; 
            set => SetProperty(ref _text, value); 
        }

        private SolidColorBrush _messageColor;
        public SolidColorBrush MessageColor 
        { 
            get => _messageColor; 
            set => SetProperty(ref _messageColor, value); 
        }
    }
}