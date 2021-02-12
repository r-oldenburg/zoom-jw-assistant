using System;
using System.Globalization;
using System.Windows.Data;

namespace ZoomJWAssistant.ValueConverter
{
    public class TypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            return value.GetType().Name;
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
