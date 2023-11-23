using System;
using System.Globalization;
using System.Windows.Data;

namespace FTP_Client.Converters
{
    public class FolderSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long size && size == 0)
                return string.Empty;
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}