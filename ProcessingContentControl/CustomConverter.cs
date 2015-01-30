using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace System.Windows.Data
{
    public delegate object ConvertHandler(object value, Type targetType, object parameter, CultureInfo culture);

    public class CustomConverter : IValueConverter
    {
        public event ConvertHandler Convert;
        public event ConvertHandler ConvertBack;

        public CustomConverter() { }

        public CustomConverter(ConvertHandler convertHandler)
            : this(convertHandler, null)
        {
        }

        public CustomConverter(ConvertHandler convertHandler, ConvertHandler convertBackHandler)
        {
            if (convertHandler != null)
                Convert += convertHandler;

            if (convertBackHandler != null)
                ConvertBack += convertBackHandler;
        }

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Convert == null)
                throw new NotImplementedException();

            return Convert(value, targetType, parameter, culture);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (ConvertBack == null)
                throw new NotImplementedException();

            return ConvertBack(value, targetType, parameter, culture);
        }

        #endregion
    }
}
