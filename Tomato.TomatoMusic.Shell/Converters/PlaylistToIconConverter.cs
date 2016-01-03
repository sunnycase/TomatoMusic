using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Tomato.TomatoMusic.Shell.Converters
{
    class PlaylistToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var anchor = value as IPlaylistAnchor;
            var guid = anchor?.Placeholder.Key;
            if (guid == Primitives.Playlist.MusicLibraryPlaylistKey)
                return Symbol.Library;
            return Symbol.MusicInfo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
