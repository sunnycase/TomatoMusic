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
using Tomato.Mvvm;
using Windows.Storage;
using Tomato.TomatoMusic.Playlist.Providers;
using Windows.Storage.AccessCache;
using Tomato.Media.Codec;

namespace Tomato.TomatoMusic.Playlist.Services
{
    interface IPlaylistAnchorHandler
    {
        void OnPlaylistSelectionChanged(IPlaylistAnchor anchor, bool? selected);
        void SetPlaylist(IPlaylistAnchor anchor);
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

        public void ResetViewToMe()
        {
            IPlaylistAnchorHandler handler;
            if (_handler.TryGetTarget(out handler))
                handler.SetPlaylist(this);
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
        private readonly CodecManager _codecManager;

        public PlaylistManager()
        {
            _codecManager = new CodecManager();
            _codecManager.RegisterDefaultCodecs();
            Initialize();
        }

        private async void Initialize()
        {
            IsRefreshing = true;
            MusicLibrary = WrapPlaylistAnchor(PlaylistPlaceholder.MusicLibrary);
            Default = WrapPlaylistAnchor(PlaylistPlaceholder.Default);
            await LoadPlaylists();
            IsRefreshing = false;
            MusicLibrary.IsSelected = true;
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

        public void SetPlaylist(IPlaylistAnchor anchor)
        {
            if (SelectedPlaylist != anchor)
                SelectedPlaylist = anchor;
            else
                OnPropertyChanged(nameof(SelectedPlaylist));
        }

        public IPlaylistAnchor GetAnchorByKey(Guid key)
        {
            return EnumAnchors().FirstOrDefault(o => o.Placeholder.Key == key);
        }

        private IEnumerable<IPlaylistAnchor> EnumAnchors()
        {
            yield return MusicLibrary;
            yield return Default;
            foreach (var item in CustomPlaylists)
                yield return item;
        }

        class PlaylistContentProvider : BindableBase, IPlaylistContentProvider
        {
            public Task<IObservableCollection<TrackInfo>> Result { get; }
            private WatchedPlaylistProvider _watchedProvider;
            private PlaylistFile _playlistFile;
            private BindableCollection<TrackInfo> _tracks;

            private readonly Task _openPlaylistTask;

            private bool _isRefreshing = true;
            public bool IsRefreshing
            {
                get { return _isRefreshing; }
                private set { SetProperty(ref _isRefreshing, value); }
            }

            public PlaylistContentProvider(IPlaylistAnchor anchor)
            {
                _openPlaylistTask = OpenPlaylist(anchor);
                Result = LoadContent(anchor);
            }

            public async void AddFolder(StorageFolder folder)
            {
                var futureAccessList = StorageApplicationPermissions.FutureAccessList;
                if (!futureAccessList.CheckAccess(folder))
                    futureAccessList.Add(folder);
                await _openPlaylistTask;
                _playlistFile.AddFolder(folder);
                _watchedProvider.AddFolder(folder);
            }

            private async Task OpenPlaylist(IPlaylistAnchor anchor)
            {
                _playlistFile = await PlaylistFile.OpenAsync(anchor.Placeholder);
                _watchedProvider = new WatchedPlaylistProvider(_playlistFile.Playlist);
                IsRefreshing = _watchedProvider.IsRefreshing;
                _watchedProvider.PropertyChanged += _watchedProvider_PropertyChanged;
            }

            private void _watchedProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(WatchedPlaylistProvider.IsRefreshing):
                        IsRefreshing = _watchedProvider.IsRefreshing;
                        break;
                    default:
                        break;
                }
            }

            private async Task<IObservableCollection<TrackInfo>> LoadContent(IPlaylistAnchor anchor)
            {
                await _openPlaylistTask;
                var completion = new TaskCompletionSource<IObservableCollection<TrackInfo>>();
                EventHandler firstUpdate = null;
                firstUpdate = (s, e) =>
                {
                    _watchedProvider.Updated -= firstUpdate;
                    _tracks = new BindableCollection<TrackInfo>(_watchedProvider.GetTrackInfos().Distinct());
                    _watchedProvider.Updated += _watchedProvider_Updated;
                    completion.SetResult(_tracks);
                };
                _watchedProvider.Updated += firstUpdate;
                _watchedProvider.LoadPlaylist();
                return await completion.Task;
            }

            private void _watchedProvider_Updated(object sender, EventArgs e)
            {
                var newTracks = _watchedProvider.GetTrackInfos().Distinct().ToList();
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
