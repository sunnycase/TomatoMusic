using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Tomato.TomatoMusic.Shell.Converters
{
    [ContentProperty(Name = nameof(Brushes))]
    class IndexToBrushConverter : IValueConverter
    {
        public Collection<Brush> Brushes { get; } = new Collection<Brush>();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var index = (value as int?) ?? 0;
            return Brushes[index % Brushes.Count];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
