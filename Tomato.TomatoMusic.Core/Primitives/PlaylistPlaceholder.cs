using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.ApplicationModel.Resources;

namespace Tomato.TomatoMusic.Primitives
{
    public class PlaylistPlaceholder
    {
        public Guid Key { get; set; }

        public string Name { get; set; }

        public static readonly PlaylistPlaceholder MusicLibrary = new PlaylistPlaceholder
        {
            Key = Playlist.MusicLibraryPlaylistKey,
            Name = IoC.Get<ResourceLoader>().GetString("MusicLibrary")
        };

        public static readonly PlaylistPlaceholder Default = new PlaylistPlaceholder
        {
            Key = Playlist.DefaultPlaylistKey,
            Name = IoC.Get<ResourceLoader>().GetString("Default")
        };
    }
}
