using System;
using System.Windows.Data;

namespace LedControl.Converters
{
    internal class WidthToBorderThickConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double height = (double)value;
            return height * 10 / 100; // border is 8% of height
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}