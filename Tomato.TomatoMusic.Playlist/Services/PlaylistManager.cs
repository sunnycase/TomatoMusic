using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Playlist.Models;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.Storage;

namespace Tomato.TomatoMusic.Playlist.Services
{
    interface IPlaylistAnchorHandler
    {
        void OnPlaylistSelectionChanged(IPlaylistAnchor anchor, bool? selected);
    }

    class PlaylistAnchor : BindableBase, IPlaylistAnchor
    {
        private bool? _isSelected;
        public bool? IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    IPlaylistAnchorHandler target;
                    if (_handler.TryGetTarget(out target))
                        target?.OnPlaylistSelectionChanged(this, value);
                }
            }
        }

        private readonly PlaylistPlaceholder _placeholder;
        public PlaylistPlaceholder Placeholder => _placeholder;

        private readonly WeakReference<IPlaylistAnchorHandler> _handler;
        public PlaylistAnchor(PlaylistPlaceholder placeholder, WeakReference<IPlaylistAnchorHandler> handler)
        {
            _placeholder = placeholder;
            _handler = handler;
        }
    }

    class PlaylistManager : BindableBase, IPlaylistManager, IPlaylistAnchorHandler
    {
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { SetProperty(ref _isRefreshing, value); }
        }

        private readonly ObservableCollection<IPlaylistAnchor> _customPlaylists = new ObservableCollection<IPlaylistAnchor>();
        public IReadOnlyCollection<IPlaylistAnchor> CustomPlaylists
        {
            get { return _customPlaylists; }
        }

        private IPlaylistAnchor _selectedPlaylist;
        public IPlaylistAnchor SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            private set { SetProperty(ref _selectedPlaylist, value); }
        }

        public IPlaylistAnchor MusicLibrary { get; private set; }
        public IPlaylistAnchor Default { get; private set; }

        private PlaylistIndexFile _playlistIndexFile;

        public PlaylistManager()
        {
            Initialize();
        }

        private async void Initialize()
        {
            IsRefreshing = true;
            MusicLibrary = WrapPlaylistAnchor(PlaylistPlaceholder.MusicLibrary);
            Default = WrapPlaylistAnchor(PlaylistPlaceholder.Default);
            await LoadPlaylists();
            IsRefreshing = false;
        }

        private async Task LoadPlaylists()
        {
            _playlistIndexFile = await PlaylistIndexFile.OpenAsync();
            _customPlaylists.Clear();
            _playlistIndexFile.PlaylistIndex.Playlists.Apply(o => _customPlaylists.Add(WrapPlaylistAnchor(o)));
        }

        public async Task AddCustomPlaylist(string name)
        {
            var newPlaylist = new PlaylistPlaceholder
            {
                Key = Guid.NewGuid(),
                Name = name
            };
            _customPlaylists.Add(WrapPlaylistAnchor(newPlaylist));
        }

        private IPlaylistAnchor WrapPlaylistAnchor(PlaylistPlaceholder placeholder)
        {
            return new PlaylistAnchor(placeholder, new WeakReference<IPlaylistAnchorHandler>(this));
        }

        public void OnPlaylistSelectionChanged(IPlaylistAnchor anchor, bool? selected)
        {
            if (selected ?? false)
                SelectedPlaylist = anchor;
            else if (anchor == SelectedPlaylist)
                SelectedPlaylist = null;
        }
    }
}
