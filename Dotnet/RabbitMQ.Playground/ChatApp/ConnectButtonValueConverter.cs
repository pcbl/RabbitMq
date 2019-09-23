using System;
using System.Globalization;
using System.Windows.Data;

namespace ChatApp
{
    public sealed class ConnectButtonValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isConnected = bool.Parse(value.ToString());

            if (isConnected)
            {
                return "Disconnect";
            }
            else
            {
                return "Connect";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
