using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker
{
    public class QuantityValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string)value))
            {
                return new ValidationResult(false, "");
            }
            int quantity;
            bool brc = int.TryParse((string)value, out quantity);
            if (!brc)
            {
                return new ValidationResult(false, GlblRes.Not_a_valid_number);
            }
            if (quantity < 1)
            {
                return new ValidationResult(false, GlblRes.Quantity_must_be_1_or_larger);
            }
            return ValidationResult.ValidResult;
        }
    }
}