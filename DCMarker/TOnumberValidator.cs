using Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker
{
    public class TOnumberValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string)value))
            {
                return new ValidationResult(false, "");
            }

            return ValidationResult.ValidResult;
        }
    }
}