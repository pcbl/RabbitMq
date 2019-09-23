using System;
using System.Globalization;
using System.Windows.Data;

namespace ChatApp
{
    /// <summary>
    /// Converter to Handle the Connect Button Label amon "Connect" or "Disconnet"
    /// </summary>
    public sealed class ConnectButtonValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //We expect to get a Boolean
            var isConnected = bool.Parse(value.ToString());
            //And based on that we return the appropriate label
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
