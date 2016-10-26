using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace Tomato.TomatoMusic.Shell.Converters
{
    [ContentProperty(Name = nameof(Entries))]
    class OrderRuleToTrackTemplateConverter : IValueConverter
    {
        public Dictionary<string, DataTemplate> Entries { get; } = new Dictionary<string, DataTemplate>();
        public DataTemplate Default { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DataTemplate template;
            if (Entries.TryGetValue(Enum.GetName(typeof(MusicsOrderRule), value), out template))
                return template;
            return Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
