using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Playlist.Services;

namespace Tomato.TomatoMusic
{
    public static class PlaylistModule
    {
        public static void UsePlaylist(this SimpleContainer container)
        {
            container.Singleton<IPlaylistManager, PlaylistManager>();
        }
    }
}
