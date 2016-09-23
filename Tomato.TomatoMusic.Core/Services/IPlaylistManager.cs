using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using System.Collections.Specialized;

namespace Tomato.TomatoMusic.Services
{
    public interface IPlaylistManager
    {
        IReadOnlyList<PlaylistPlaceholder> CustomPlaylists { get; }
        event NotifyCollectionChangedEventHandler CustomPlaylistsChanged;

        Task AddCustomPlaylist(string name);

        PlaylistPlaceholder MusicLibrary { get; }

        IPlaylistContentProvider GetPlaylistContentProvider(PlaylistPlaceholder playlist);

        PlaylistPlaceholder GetPlaylistByKey(Guid key);
    }
}
