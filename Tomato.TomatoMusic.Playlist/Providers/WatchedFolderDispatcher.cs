using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.Media;
using Tomato.TomatoMusic.Primitives;
using Tomato.Uwp.Mvvm;
using Tomato.Uwp.Mvvm.Threading;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Threading;
using Windows.System.Threading.Core;
using Windows.UI.Core;

namespace Tomato.TomatoMusic.Playlist.Providers
{
    class WatchedFolderDispatcher : BindableBase
    {
        private readonly HashSet<WatchedFolder> _wantUpdateFolders = new HashSet<WatchedFolder>();

        private readonly object _countdownLocker = new object();
        private ThreadPoolTimer _countdown;
        private static readonly TimeSpan CountdownTime = new TimeSpan(0, 0, 3);
        private CancellationTokenSource _updateWorkerCancelSource;
        private readonly object _updateWorkerCancelSourceLocker = new object();
        private readonly TaskFactory _updateWorkerFactory;

        private IReadOnlyDictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>> _folderContents =
            new ReadOnlyDictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>(new Dictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>());
        public IReadOnlyDictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>> FolderContents
        {
            get { return _folderContents; }
            private set { SetProperty(ref _folderContents, value); }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { SetProperty(ref _isRefreshing, value); }
        }

        private volatile bool _refreshSuspended = false;

        public WatchedFolderDispatcher()
        {
            _updateWorkerFactory = new TaskFactory(new CancellationToken(), TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning, new LimitedConcurrencyLevelTaskScheduler(1));
        }

        public void RequestFileUpdate(WatchedFolder folder)
        {
            lock (_wantUpdateFolders)
            {
                _wantUpdateFolders.Add(folder);
                Execute.BeginOnUIThread(() => IsRefreshing = true);
            }
            if (!_refreshSuspended)
                RestartCountdown();
        }

        public void SuspendRefresh()
        {
            _refreshSuspended = true;
        }

        public void ResumeRefresh()
        {
            _refreshSuspended = false;
            lock (_wantUpdateFolders)
            {
                if (_wantUpdateFolders.Any())
                    RestartCountdown();
            }
        }

        private void RestartCountdown()
        {
            lock (_countdownLocker)
            {
                CancelUpdateWorker();
                if (_countdown != null)
                    _countdown.Cancel();
                _countdown = ThreadPoolTimer.CreateTimer(OnStartUpdateWorker, CountdownTime);
            }
        }

        private void CancelUpdateWorker()
        {
            lock (_updateWorkerCancelSourceLocker)
            {
                _updateWorkerCancelSource?.Cancel();
                _updateWorkerCancelSource = null;
            }
        }

        private async void OnStartUpdateWorker(ThreadPoolTimer timer)
        {
            CancellationToken token;
            lock (_updateWorkerCancelSourceLocker)
            {
                _updateWorkerCancelSource = new CancellationTokenSource();
                token = _updateWorkerCancelSource.Token;
            }
            await _updateWorkerFactory.StartNew(UpdateWorkerMain, token, token);
        }

        private async Task UpdateWorkerMain(object state)
        {
            var token = (CancellationToken)state;
            try
            {
                var folders = ConsumeAllWantUpdateFolders();
                var folderContents = await FindTrackInfos(folders, token);

                token.ThrowIfCancellationRequested();
                UpdateFolderContents(folderContents);
                lock (_wantUpdateFolders)
                {
                    _wantUpdateFolders.Clear();
                    Execute.BeginOnUIThread(() => IsRefreshing = _wantUpdateFolders.Any());
                }
                GC.Collect();
                Debug.WriteLine($"WatchedFolderDispatcher: Update Completed.");
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"WatchedFolderDispatcher: Cancel Update.");
            }
        }

        private void UpdateFolderContents(Dictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>> folderContents)
        {
            var readOnly = new ReadOnlyDictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>(folderContents);
            Execute.OnUIThread(() => FolderContents = readOnly);
        }

        private WatchedFolder[] ConsumeAllWantUpdateFolders()
        {
            lock (_wantUpdateFolders)
            {
                return _wantUpdateFolders.ToArray();
            }
        }

        private async Task<Dictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>> FindTrackInfos(IEnumerable<WatchedFolder> folders, CancellationToken cancelToken)
        {
            var folderContents = new Dictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>();
            int i = 0;
            foreach (var folder in folders)
            {
                var tracks = new List<TrackInfo>();
                var files = await folder.GetFilesAsync().ConfigureAwait(false);
                foreach (var file in files)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    await TryAddTrackInfo(file, tracks);
                    if (i++ >= 10)
                    {
                        i = 0;
                        GC.Collect();
                    }
                }
                folderContents.Add(folder, tracks);
            }
            return folderContents;
        }

        private async Task TryAddTrackInfo(StorageFile file, List<TrackInfo> tracks)
        {
            try
            {
                using (var stream = await file.OpenReadAsync())
                {
                    var mediaSource = await MediaSource.CreateFromStream(stream);
                    var title = mediaSource.Title;
                    var trackInfo = new TrackInfo
                    {
                        Source = new Uri(file.Path),
                        Title = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(file.Path) : title,
                        Album = mediaSource.Album,
                        Artist = mediaSource.Artist,
                        AlbumArtist = mediaSource.AlbumArtist,
                        Duration = mediaSource.Duration
                    };
                    tracks.Add(trackInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Falied to acquire track info ({file.Path}): {ex.Flatten()}");
            }
        }
    }
}
