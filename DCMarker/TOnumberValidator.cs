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
    public class TOnumberValidator : System.Windows.Controls.ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (this.Wrapper.HasTO)
            {
                if (string.IsNullOrWhiteSpace((string)value))
                {
                    return new ValidationResult(false, "");
                }

                var toNumber = value.ToString();
                if (toNumber.Length != DCConfig.Instance.ToNumberLength)
                {
                    return new ValidationResult(false, GlblRes.TO_number_must_be_7_number_long);
                }
            }
            return ValidationResult.ValidResult;
        }

        public Wrapper Wrapper { get; set; }
    }

    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
    }

    public class Wrapper : DependencyObject
    {
        public static readonly DependencyProperty HasTOnrProperty =
             DependencyProperty.Register("HasTO", typeof(bool),
             typeof(Wrapper), new FrameworkPropertyMetadata(false));

        public bool HasTO
        {
            get { return (bool)GetValue(HasTOnrProperty); }
            set { SetValue(HasTOnrProperty, value); }
        }
    }
}