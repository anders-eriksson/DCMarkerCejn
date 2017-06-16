using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GlblRes = global::DCAdmin.Properties.Resources;

namespace DCAdmin
{
    public class FixtureValidator : ValidationRule
    {
        public override ValidationResult Validate
          (object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace((string)value))
            {
                return new ValidationResult(false, GlblRes.value_cant_be_emtpy);
            }

            return ValidationResult.ValidResult;
        }
    }
}