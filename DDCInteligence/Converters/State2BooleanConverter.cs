using System;
using System.Globalization;
using System.Windows.Data;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.Converters
{
    public class State2BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is EState val)
            {
                switch (val)
                {
                    case EState.REQUEST:
                        return false;
                    case EState.SAVING:
                        return false;
                }
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
