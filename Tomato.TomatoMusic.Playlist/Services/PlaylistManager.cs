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
using Tomato.Media.Toolkit;
using System.Collections.Specialized;

namespace Tomato.TomatoMusic.Playlist.Services
{

    class PlaylistManager : BindableBase, IPlaylistManager
    {
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { SetProperty(ref _isRefreshing, value); }
        }

        private readonly BindableCollection<PlaylistPlaceholder> _customPlaylists = new BindableCollection<PlaylistPlaceholder>();
        public IReadOnlyList<PlaylistPlaceholder> CustomPlaylists
        {
            get { return _customPlaylists; }
        }

        public PlaylistPlaceholder MusicLibrary => PlaylistPlaceholder.MusicLibrary;

        private PlaylistIndexFile _playlistIndexFile;

        private readonly WeakReferenceDictionary<PlaylistPlaceholder, IPlaylistContentProvider> _playlistContentProviders = new WeakReferenceDictionary<PlaylistPlaceholder, IPlaylistContentProvider>();
        private readonly MediaEnvironment _mediaEnvironment;

        public event NotifyCollectionChangedEventHandler CustomPlaylistsChanged
        {
            add { _customPlaylists.CollectionChanged += value; }
            remove { _customPlaylists.CollectionChanged -= value; }
        }

        public PlaylistManager()
        {
            _mediaEnvironment = new MediaEnvironment();
            _mediaEnvironment.RegisterDefaultCodecs();
            Initialize();
        }

        private async void Initialize()
        {
            IsRefreshing = true;
            await LoadPlaylists();
            IsRefreshing = false;
        }

        private async Task LoadPlaylists()
        {
            _playlistIndexFile = await PlaylistIndexFile.OpenAsync();
            _customPlaylists.Clear();
            _customPlaylists.AddRange(_playlistIndexFile.PlaylistIndex.Playlists);
        }

        public async Task AddCustomPlaylist(string name)
        {
            var newPlaylist = new PlaylistPlaceholder
            {
                Key = Guid.NewGuid(),
                Name = name
            };
            _customPlaylists.Add(newPlaylist);
            _playlistIndexFile.PlaylistIndex.Playlists.Add(newPlaylist);
            await _playlistIndexFile.Save();
        }

        public IPlaylistContentProvider GetPlaylistContentProvider(PlaylistPlaceholder playlist)
        {
            lock (_playlistContentProviders)
            {
                return _playlistContentProviders.GetOrAddValue(playlist, a => new PlaylistContentProvider(a));
            }
        }

        public PlaylistPlaceholder GetPlaylistByKey(Guid key)
        {
            return EnumAnchors().FirstOrDefault(o => o.Key == key);
        }

        private IEnumerable<PlaylistPlaceholder> EnumAnchors()
        {
            yield return MusicLibrary;
            foreach (var item in CustomPlaylists)
                yield return item;
        }

        class PlaylistContentProvider : BindableBase, IPlaylistContentProvider
        {
            private readonly BindableCollection<TrackInfo> _tracks = new BindableCollection<TrackInfo>();
            public ReadOnlyObservableCollection<TrackInfo> Tracks { get; }

            private WatchedPlaylistProvider _watchedProvider;
            private PlaylistFile _playlistFile;

            private readonly Task _openPlaylistTask;

            private bool _isRefreshing = true;
            public bool IsRefreshing
            {
                get { return _isRefreshing; }
                private set { SetProperty(ref _isRefreshing, value); }
            }

            public PlaylistContentProvider(PlaylistPlaceholder playlist)
            {
                Tracks = new ReadOnlyObservableCollection<TrackInfo>(_tracks);
                _openPlaylistTask = OpenPlaylist(playlist);
                LoadContent();
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

            private async Task OpenPlaylist(PlaylistPlaceholder playlist)
            {
                _playlistFile = await PlaylistFile.OpenAsync(playlist);
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

            private async void LoadContent()
            {
                await _openPlaylistTask;
                _watchedProvider.Updated += _watchedProvider_Updated;
                _tracks.AddRange(_watchedProvider.GetTrackInfos().Distinct());
                _watchedProvider.LoadPlaylist();
            }

            private void _watchedProvider_Updated(object sender, EventArgs e)
            {
                var newTracks = _watchedProvider.GetTrackInfos().Distinct().ToList();
                var comparer = new TrackInfo.ExistenceEqualityComparer();
                var toAdd = newTracks.Except(_tracks, comparer).ToList();
                var toRemove = _tracks.Except(newTracks, comparer).ToList();

                if (toAdd.Any())
                    _tracks.AddRange(toAdd);
                if (toRemove.Any())
                    _tracks.RemoveRange(toRemove);
            }

            public async Task<IReadOnlyList<StorageFolder>> GetFolders()
            {
                await _openPlaylistTask;
                return _watchedProvider.GetFolders().ToList();
            }

            public async void UpdateFolders(IReadOnlyList<StorageFolder> folders)
            {
                var futureAccessList = StorageApplicationPermissions.FutureAccessList;
                var oldFolders = await GetFolders();

                var toRemove = oldFolders.Except(folders).ToList();
                _watchedProvider.RemoveFolders(toRemove);

                var toAdd = folders.Except(oldFolders).ToList();
                foreach (var folder in toAdd)
                {
                    if (!futureAccessList.CheckAccess(folder))
                        futureAccessList.Add(folder);
                }
                _watchedProvider.AddFolders(toAdd);
                _playlistFile.SetFolders(folders);
            }
        }
    }
}
