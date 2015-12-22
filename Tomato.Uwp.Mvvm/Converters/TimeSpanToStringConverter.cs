using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Tomato.Uwp.Mvvm.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public string Format { get; set; } = string.Empty;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;
            return ((TimeSpan)value).ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
