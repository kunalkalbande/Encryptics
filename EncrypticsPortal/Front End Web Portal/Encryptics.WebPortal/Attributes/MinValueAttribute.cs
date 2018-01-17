using System;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Attributes
{
    public class MinValueAttribute : ValidationAttribute
    {
        private readonly int _minValue;

        public MinValueAttribute(int minValue, string errorMessage) : base(errorMessage)
        {
            _minValue = minValue;
        }

        public override bool IsValid(object value)
        {
            try
            {
                return Convert.ToInt32(value) >= _minValue;
            }
            catch
            {
                return false;
            }
        }
    }
}