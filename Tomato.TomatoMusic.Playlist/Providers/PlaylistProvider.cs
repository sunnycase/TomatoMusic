using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomato.Media;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Primitives;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;
using Windows.System.Threading.Core;

namespace Tomato.TomatoMusic.Playlist.Providers
{
    class PlaylistProvider
    {
        public PlaylistProvider()
        {
            Start();
        }

        private void Start()
        {
            var folders = new[] {
                KnownFolders.MusicLibrary
            };
            var dispatcher = new WatchedFolderDispatcher();
            var watched = folders.Select(o => new WatchedFolder(dispatcher, o)).ToList();
            foreach (var watch in watched)
                watch.Refresh();
        }
    }
}
