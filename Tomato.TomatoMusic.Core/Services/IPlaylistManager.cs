using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;

namespace Tomato.TomatoMusic.Services
{
    public interface IPlaylistManager : INotifyPropertyChanged
    {
        IReadOnlyCollection<IPlaylistAnchor> CustomPlaylists { get; }

        Task AddCustomPlaylist(string name);

        IPlaylistAnchor MusicLibrary { get; }
        IPlaylistAnchor Default { get; }

        IPlaylistContentProvider GetPlaylistContentProvider(IPlaylistAnchor anchor);

        IPlaylistAnchor SelectedPlaylist { get; }
    }
}
