using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Tomato.TomatoMusic.Shell.Converters
{
    class MusicsViewTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return IoC.Get<ResourceLoader>().GetString($"MusicsViewType/{((MusicsViewType)value).ToString()}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
