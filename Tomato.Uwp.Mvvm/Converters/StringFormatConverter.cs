using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Tomato.Uwp.Mvvm.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public string Format { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format(Format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
