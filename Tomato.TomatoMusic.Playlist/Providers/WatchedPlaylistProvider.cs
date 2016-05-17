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
using Tomato.Mvvm;

namespace Tomato.TomatoMusic.Playlist.Providers
{
    class WatchedPlaylistProvider : BindableBase
    {
        private WatchedFolderDispatcher _folderDispatcher = new WatchedFolderDispatcher();
        private readonly Primitives.Playlist _playlist;
        private readonly List<WatchedFolder> _knownFolders = new List<WatchedFolder>();
        private readonly List<WatchedFolder> _folders = new List<WatchedFolder>();
        private readonly List<TrackInfo> _individualTracks = new List<TrackInfo>();
        private readonly Dictionary<WatchedFolder, FolderCacheProvider> _knownFolderCaches = new Dictionary<WatchedFolder, FolderCacheProvider>();
        private readonly Dictionary<WatchedFolder, FolderCacheProvider> _folderCaches = new Dictionary<WatchedFolder, FolderCacheProvider>();

        public event EventHandler Updated;

        public bool IsRefreshing => _folderDispatcher.IsRefreshing;

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
            }
            finally
            {
                _folderDispatcher.ResumeRefresh();
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

        public IEnumerable<StorageFolder> GetFolders()
        {
            return _folders.Select(o => o.Folder);
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
                var folder = new WatchedFolder((StorageFolder)typeof(KnownFolders).GetRuntimeProperty(name).GetValue(null));
                folders.Add(folder);
                var cache = new FolderCacheProvider(_playlist.Key, name);
                _knownFolderCaches.Add(folder, cache);
                await cache.LoadCache();
                folder.FileUpdateRequested += Folder_FileUpdateRequested;
                return folder;
            }
            catch (Exception)
            {
                Debug.WriteLine($"Error in adding knwon folder: {name}.");
                return null;
            }
        }

        private void Folder_FileUpdateRequested(WatchedFolder folder)
        {
            _folderDispatcher.RequestFileUpdate(folder);
        }

        private async Task<WatchedFolder> TryAddFolder(string path, ICollection<WatchedFolder> folders)
        {
            try
            {
                var folder = new WatchedFolder(await StorageFolder.GetFolderFromPathAsync(path));
                folders.Add(folder);
                var cache = new FolderCacheProvider(_playlist.Key, path);
                _folderCaches.Add(folder, cache);
                await cache.LoadCache();
                folder.FileUpdateRequested += Folder_FileUpdateRequested;
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
                case nameof(WatchedFolderDispatcher.IsRefreshing):
                    OnPropertyChanged(nameof(IsRefreshing));
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
                try
                {
                    var watcher = await TryAddFolder(folder.Path, _folders);
                    if (watcher != null)
                    {
                        watcher.Refresh();
                        NotifyUpdated();
                    }
                }
                finally
                {
                    _folderDispatcher.ResumeRefresh();
                }
            }
        }

        public async void AddFolders(List<StorageFolder> folders)
        {
            _folderDispatcher.SuspendRefresh();
            bool updated = false;
            try
            {
                foreach (var folder in folders)
                {
                    if (_folders.All(o => o.Folder != folder))
                    {
                        var watcher = await TryAddFolder(folder.Path, _folders);
                        if (watcher != null)
                        {
                            updated = true;
                            watcher.Refresh();
                        }
                    }
                }
            }
            finally
            {
                if(updated)
                    NotifyUpdated();
                _folderDispatcher.ResumeRefresh();
            }
        }

        public void RemoveFolder(StorageFolder folder)
        {
            _folderDispatcher.SuspendRefresh();
            try
            {
                var watch = _folders.Find(o => o.Folder.IsEqual(folder));
                if (watch != null)
                {
                    watch.FileUpdateRequested -= Folder_FileUpdateRequested;
                    _folderDispatcher.CancelFileUpdate(watch);
                    _folders.Remove(watch);
                    _folderCaches.Remove(watch);
                }
                NotifyUpdated();
            }
            finally
            {
                _folderDispatcher.ResumeRefresh();
            }
        }

        public void RemoveFolders(IEnumerable<StorageFolder> folders)
        {
            _folderDispatcher.SuspendRefresh();
            try
            {
                foreach (var folder in folders)
                {
                    var watch = _folders.Find(o => o.Folder.IsEqual(folder));
                    if (watch != null)
                    {
                        watch.FileUpdateRequested -= Folder_FileUpdateRequested;
                        _folderDispatcher.CancelFileUpdate(watch);
                        _folders.Remove(watch);
                        _folderCaches.Remove(watch);
                    }
                }
                NotifyUpdated();
            }
            finally
            {
                _folderDispatcher.ResumeRefresh();
            }
        }
    }
}
