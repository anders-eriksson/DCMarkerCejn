using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DCHistory
{
    public class NumberFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object
            parameter, System.Globalization.CultureInfo culture)
        {
            if ((string)value == string.Empty)
            {
                return "0";
            }

            try
            {
                return System.Convert.ToInt32(value).ToString(parameter as string);
            }
            catch (Exception ex)
            {
                DCLog.Log.Error(ex, "Error converting value to string!");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}