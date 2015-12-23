using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.Media;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Playlist.Cache;
using Tomato.TomatoMusic.Primitives;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.System.Threading;
using Windows.System.Threading.Core;

namespace Tomato.TomatoMusic.Playlist.Providers
{
    class WatchedPlaylistProvider
    {
        private WatchedFolderDispatcher _folderDispatcher = new WatchedFolderDispatcher();
        private readonly Primitives.Playlist _playlist;
        private readonly List<WatchedFolder> _knownFolders = new List<WatchedFolder>();
        private readonly List<WatchedFolder> _folders = new List<WatchedFolder>();
        private readonly List<TrackInfo> _individualTracks = new List<TrackInfo>();
        private readonly Dictionary<WatchedFolder, FolderCacheProvider> _knownFolderCaches = new Dictionary<WatchedFolder, FolderCacheProvider>();
        private readonly Dictionary<WatchedFolder, FolderCacheProvider> _folderCaches = new Dictionary<WatchedFolder, FolderCacheProvider>();

        public event EventHandler Updated;

        public WatchedPlaylistProvider(Primitives.Playlist playlist)
        {
            _playlist = playlist;
            _folderDispatcher.PropertyChanged += _folderDispatcher_PropertyChanged;
        }

        public async void LoadPlaylist()
        {
            try
            {
                _folderDispatcher.SuspendRefresh();
                // KnownFolders
                foreach (var folder in _playlist.KnownFolders)
                    await TryAddKnownFolder(folder, _knownFolders);
                // Folders
                foreach (var folder in _playlist.Folders)
                    await TryAddFolder(folder, _folders);
                // Tracks
                _individualTracks.AddRange(_playlist.Tracks);
                UpdateAll();
                _folderDispatcher.ResumeRefresh();
            }
            finally
            {
                NotifyUpdated();
            }
        }

        public IEnumerable<TrackInfo> GetTrackInfos()
        {
            foreach (var track in _individualTracks)
                yield return track;
            foreach (var folder in _knownFolderCaches)
                foreach (var track in folder.Value.Tracks)
                    yield return track;
            foreach (var folder in _folderCaches)
                foreach (var track in folder.Value.Tracks)
                    yield return track;
        }

        private void NotifyUpdated()
        {
            Execute.BeginOnUIThread(() => Updated?.Invoke(this, EventArgs.Empty));
        }

        private void UpdateAll()
        {
            foreach (var watcher in _knownFolders)
                watcher.Refresh();
            foreach (var watcher in _folders)
                watcher.Refresh();
        }

        private async Task<WatchedFolder> TryAddKnownFolder(string name, ICollection<WatchedFolder> folders)
        {
            try
            {
                var folder = new WatchedFolder(_folderDispatcher, (StorageFolder)typeof(KnownFolders).GetRuntimeProperty(name).GetValue(null));
                folders.Add(folder);
                var cache = new FolderCacheProvider(_playlist.Key, name);
                _knownFolderCaches.Add(folder, cache);
                await cache.LoadCache();
                return folder;
            }
            catch (Exception)
            {
                Debug.WriteLine($"Error in adding knwon folder: {name}.");
                return null;
            }
        }

        private async Task<WatchedFolder> TryAddFolder(string path, ICollection<WatchedFolder> folders)
        {
            try
            {
                var folder = new WatchedFolder(_folderDispatcher, await StorageFolder.GetFolderFromPathAsync(path));
                folders.Add(folder);
                var cache = new FolderCacheProvider(_playlist.Key, path);
                _folderCaches.Add(folder, cache);
                await cache.LoadCache();
                return folder;
            }
            catch (Exception)
            {
                Debug.WriteLine($"Error in adding folder: {path}.");
                return null;
            }
        }

        private void _folderDispatcher_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WatchedFolderDispatcher.FolderContents):
                    OnFolderContentsChanged();
                    break;
            }
        }

        private async void OnFolderContentsChanged()
        {
            var contents = _folderDispatcher.FolderContents;
            foreach (var folder in contents)
            {
                FolderCacheProvider cache;
                if (_knownFolderCaches.TryGetValue(folder.Key, out cache))
                    await cache.Update(folder.Value);
                if (_folderCaches.TryGetValue(folder.Key, out cache))
                    await cache.Update(folder.Value);
            }
            NotifyUpdated();
        }

        public async void AddFolder(StorageFolder folder)
        {
            if (_folders.All(o => o.Folder != folder))
            {
                _folderDispatcher.SuspendRefresh();
                var watcher = await TryAddFolder(folder.Path, _folders);
                _folderDispatcher.ResumeRefresh();
                watcher.Refresh();
                NotifyUpdated();
            }
        }
    }
}
