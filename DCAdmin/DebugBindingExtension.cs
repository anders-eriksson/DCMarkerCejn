using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;
using System.Diagnostics;

/*
 * Usage
 *  SelectedItem="{Binding Path=SelectedLaserDataRow, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, Converter={l:DebugBinding}}"
 */

namespace DCAdmin
{
    /// <summary>
    /// Converter to debug the binding values
    /// </summary>
    public class DebugConvertor : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// ask the debugger to break
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debugger.Break();
            return Binding.DoNothing;
        }

        /// <summary>
        /// ask the debugger to break
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debugger.Break();
            return Binding.DoNothing;
        }

        #endregion IValueConverter Members
    }

    /// <summary>
    /// Markup extension to debug databinding
    /// </summary>
    public class DebugBindingExtension : MarkupExtension
    {
        /// <summary>
        /// Creates a new instance of the Convertor for debugging
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns>Return a convertor that can be debugged to see the values for the binding</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new DebugConvertor();
        }
    }
}