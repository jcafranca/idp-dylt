using DDCInteligence.DataTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.Converters
{
    public class State2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is EState val)
            {
                switch (val)
                {
                    case EState.REQUEST:
                        return Visibility.Visible;
                    case EState.SAVING:
                        return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
