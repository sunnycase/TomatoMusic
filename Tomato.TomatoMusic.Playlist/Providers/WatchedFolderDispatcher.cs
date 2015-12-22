using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.Media;
using Tomato.TomatoMusic.Primitives;
using Tomato.Uwp.Mvvm;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.System.Threading.Core;
using Windows.UI.Core;

namespace Tomato.TomatoMusic.Playlist.Providers
{
    class WatchedFolderDispatcher : BindableBase
    {
        private readonly HashSet<WatchedFolder> _wantUpdateFolders = new HashSet<WatchedFolder>();

        private readonly PreallocatedWorkItem _updateWorker;
        private readonly object _countdownLocker = new object();
        private ThreadPoolTimer _countdown;
        private static readonly TimeSpan CountdownTime = new TimeSpan(0, 0, 15);
        private CancellationTokenSource _updateWorkerCancelSource;
        private readonly object _updateWorkerCancelSourceLocker = new object();

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
            _updateWorker = new PreallocatedWorkItem(UpdateWorkerMain, WorkItemPriority.Low, WorkItemOptions.TimeSliced);
        }

        public void RequestFileUpdate(WatchedFolder folder)
        {
            lock (_wantUpdateFolders)
            _wantUpdateFolders.Add(folder);
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
                if(_wantUpdateFolders.Any())
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
            lock (_updateWorkerCancelSourceLocker)
            {
                _updateWorkerCancelSource = new CancellationTokenSource();
            }
            await _updateWorker.RunAsync();
        }

        private async void UpdateWorkerMain(IAsyncAction operation)
        {
            var myCancelSource = _updateWorkerCancelSource;
            try
            {
                Execute.OnUIThread(() => IsRefreshing = true);

                if (myCancelSource == null)
                    throw new OperationCanceledException();
                var folders = ConsumeAllWantUpdateFolders();
                var folderContents = await FindTrackInfos(folders, myCancelSource);

                if (myCancelSource.IsCancellationRequested)
                    throw new OperationCanceledException();
                UpdateFolderContents(folderContents);

                Debug.WriteLine($"WatchedFolderDispatcher: Update Completed.");
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"WatchedFolderDispatcher: Cancel Update.");
            }
            finally
            {
                Execute.OnUIThread(() => IsRefreshing = false);
                operation.Close();
            }
        }

        private void UpdateFolderContents(Dictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>> folderContents)
        {
            var readOnly = new ReadOnlyDictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>(folderContents);
            Execute.OnUIThread(() => FolderContents = readOnly);
        }

        private WatchedFolder[] ConsumeAllWantUpdateFolders()
        {
            WatchedFolder[] folders;
            lock (_wantUpdateFolders)
            {
                folders = _wantUpdateFolders.ToArray();
                _wantUpdateFolders.Clear();
            }
            return folders;
        }

        private async Task<Dictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>> FindTrackInfos(IEnumerable<WatchedFolder> folders, CancellationTokenSource cancelSource)
        {
            var folderContents = new Dictionary<WatchedFolder, IReadOnlyCollection<TrackInfo>>();
            foreach (var folder in folders)
            {
                var tracks = new List<TrackInfo>();
                var files = await folder.GetFilesAsync().ConfigureAwait(false);
                foreach (var file in files)
                {
                    if (cancelSource.IsCancellationRequested)
                        throw new OperationCanceledException();
                    var mediaSource = await MediaSource.CreateFromStream(await file.OpenReadAsync().AsTask().ConfigureAwait(false)).AsTask().ConfigureAwait(false);
                    tracks.Add(new TrackInfo
                    {
                        Source = new Uri(file.Path),
                        Title = mediaSource.Title,
                        Album = mediaSource.Album,
                        Artist = mediaSource.Artist,
                        AlbumArtist = mediaSource.AlbumArtist,
                        Duration = mediaSource.Duration
                    });
                }
                folderContents.Add(folder, tracks);
            }
            return folderContents;
        }
    }
}
