using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace DDCInteligence.Converters
{
    public class LoadingAndItemsCountToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return Visibility.Collapsed;

            bool isLoading = values[0] is bool loading && loading;
            int itemCount = values[1] is int count ? count : 0;

            // Show "No data found" only when NOT loading AND no items
            return !isLoading && itemCount == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}