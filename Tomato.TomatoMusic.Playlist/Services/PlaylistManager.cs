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
using Tomato.TomatoMusic.Playlist.Providers;

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

        private readonly Dictionary<IPlaylistAnchor, WeakReference<IPlaylistContentProvider>> _playlistContentProviders = new Dictionary<IPlaylistAnchor, WeakReference<IPlaylistContentProvider>>();

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

        public IPlaylistContentProvider GetPlaylistContentProvider(IPlaylistAnchor anchor)
        {
            lock (_playlistContentProviders)
            {
                IPlaylistContentProvider target;
                WeakReference<IPlaylistContentProvider> weak;
                if (_playlistContentProviders.TryGetValue(anchor, out weak))
                {
                    if (weak.TryGetTarget(out target) && target != null)
                        return target;
                    else
                        _playlistContentProviders.Remove(anchor);
                }
                target = new PlaylistContentProvider(anchor);
                _playlistContentProviders.Add(anchor, new WeakReference<IPlaylistContentProvider>(target));
                return target;
            }
        }

        class PlaylistContentProvider : IPlaylistContentProvider
        {
            public Task<IObservableCollection<TrackInfo>> Result { get; }
            private WatchedPlaylistProvider _watchedProvider;
            private PlaylistFile _playlistFile;
            private BindableCollection<TrackInfo> _tracks;

            public PlaylistContentProvider(IPlaylistAnchor anchor)
            {
                Result = LoadContent(anchor);
            }

            private async Task<IObservableCollection<TrackInfo>> LoadContent(IPlaylistAnchor anchor)
            {
                _playlistFile = await PlaylistFile.OpenAsync(anchor.Placeholder);
                _watchedProvider = new WatchedPlaylistProvider(_playlistFile.Playlist);
                var completion = new TaskCompletionSource<IObservableCollection<TrackInfo>>();
                EventHandler firstUpdate = null;
                firstUpdate = (s, e) =>
                {
                    _watchedProvider.Updated -= firstUpdate;
                    _tracks = new BindableCollection<TrackInfo>(_watchedProvider.GetTrackInfos());
                    _watchedProvider.Updated += _watchedProvider_Updated;
                    completion.SetResult(_tracks);
                };
                _watchedProvider.Updated += firstUpdate;
                _watchedProvider.LoadPlaylist();
                return await completion.Task;
            }

            private void _watchedProvider_Updated(object sender, EventArgs e)
            {
                var newTracks = _watchedProvider.GetTrackInfos().ToList();
                var comparer = new TrackInfo.ExistenceEqualityComparer();
                var toAdd = newTracks.Except(_tracks, comparer).ToList();
                var toRemove = _tracks.Except(_tracks, comparer).ToList();

                Execute.BeginOnUIThread(() =>
                {
                    if (toAdd.Any())
                        _tracks.AddRange(toAdd);
                    if (toRemove.Any())
                        _tracks.RemoveRange(toRemove);
                });
            }
        }
    }
}
