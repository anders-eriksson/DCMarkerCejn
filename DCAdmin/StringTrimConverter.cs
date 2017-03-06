using System;
using System.ComponentModel;
using System.Windows.Data;

namespace DCAdmin
{
    // *********************************************************
    // * Will not work if  UpdateSourceTrigger=PropertyChanged *
    // *********************************************************

    [ValueConversion(typeof(object), typeof(string))]
    public class StringTrimConverter : ByteConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string stringValue = value as string;
                if (stringValue != null)
                {
                    stringValue = stringValue.Trim();
                    if (string.IsNullOrEmpty(stringValue))
                    {
                        return null;
                    }
                    else
                    {
                        return stringValue;
                    }
                }
            }

            // return original value
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                        System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}