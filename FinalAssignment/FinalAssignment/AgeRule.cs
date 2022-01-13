using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FinalAssignment
{
    public class AgeRule : ValidationRule
    {
        private int minAge;
        private int maxAge;

        public int MinAge
        {
            get => minAge;
            set => minAge = value;
        }

        public int MaxAge
        {
            get => maxAge;
            set => maxAge = value;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int ageInput = 0;
            ValidationResult validationCheck = null;
            if (!int.TryParse((string)value, out ageInput))
            {
                validationCheck = new ValidationResult(false, "Please enter valid Age!");
            }
            
            if (ageInput < minAge || ageInput > maxAge)
            {
                validationCheck = new ValidationResult(false, string.Format("Age should be between ({0}-{1})", minAge, maxAge));
            }
            else
            {
                validationCheck = ValidationResult.ValidResult;
            }
            return validationCheck;
        }
    }
}
