using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using GlblRes = global::ValidationToolkit.Properties.Resources;

namespace ValidationToolkit
{
    public class IntegerRangeRule : ValidationRule
    {
        public string Name
        {
            get;
            set;
        }

        private int min = int.MinValue;

        public int Min
        {
            get { return min; }
            set { min = value; }
        }

        private int max = int.MaxValue;

        public int Max
        {
            get { return max; }
            set { max = value; }
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (!String.IsNullOrEmpty((string)value))
            {
                if (Name.Length == 0)
                    Name = "Field";
                try
                {
                    if (((string)value).Length > 0)
                    {
                        int val = Int32.Parse((String)value);
                        if (val > max)
                            return new ValidationResult(false, Name + GlblRes._must_be_less_or_equal_than_ + Max + ".");
                        if (val < min)
                            return new ValidationResult(false, Name + GlblRes._must_be_greater_or_equal_thane_ + Min + ".");
                    }
                }
                catch (Exception)
                {
                    // Try to match the system generated error message so it does not look out of place.
                    return new ValidationResult(false, Name + GlblRes._is_not_in_correct_number_format);
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}