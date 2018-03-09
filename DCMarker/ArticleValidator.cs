using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DCMarker
{
    public class ArticleValidator : ValidationRule
    {
        public override ValidationResult Validate
          (object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace((string)value))
            {
                return new ValidationResult(false, "Article number can't be empty!");
            }

            return ValidationResult.ValidResult;
        }
    }
}