using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Tomato.Uwp.Mvvm.Converters
{
    public class TimeSpanToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var time = ((TimeSpan?)value) ?? TimeSpan.Zero;
            return time.TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var secs = ((double?)value) ?? 0.0;
            return TimeSpan.FromSeconds(secs);
        }
    }
}
