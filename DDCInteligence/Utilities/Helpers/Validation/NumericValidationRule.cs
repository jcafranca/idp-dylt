using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDCInteligence.Utilities.Helpers.Validation
{
    public class NumericValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;

            if (!decimal.TryParse(input, out _))
                return new ValidationResult(false, "Invalid number.");

            return ValidationResult.ValidResult;
        }
    }

}
