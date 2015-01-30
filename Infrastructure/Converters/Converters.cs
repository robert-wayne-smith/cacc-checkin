using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Infrastructure
{
    [ValueConversion(typeof(string), typeof(string))]
    public class PhoneNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value) { return DependencyProperty.UnsetValue; }
            
            StringBuilder unMaskedPhoneNumber = new StringBuilder();

            string maskedPhoneNumber = value as string;
            char[] maskedPhoneNumberChars = maskedPhoneNumber.Trim().ToCharArray();

            foreach (char c in maskedPhoneNumberChars)
            {
                if (Char.IsDigit(c))
                {
                    unMaskedPhoneNumber.Append(c);
                }
            }
            return unMaskedPhoneNumber.ToString();
        }
    }
}
