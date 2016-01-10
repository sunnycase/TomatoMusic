using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;

namespace Tomato.TomatoMusic.Plugins.Cache
{
    abstract class CacheProvider
    {
        private const string LyricsCacheFolderName = "LyricsCache";
        private readonly Task<StorageFolder> _lyricsCacheFolder;
        private readonly BackgroundTransferGroup _transferGroup;
        private readonly BackgroundDownloader _downloader;
        private readonly Dictionary<string, Task<IStorageFile>> _activeDownloads = new Dictionary<string, Task<IStorageFile>>();
        private readonly object _activeDownloadLocker = new object();
        private CancellationTokenSource _netCts;
        private readonly object _netCtsLocker = new object();

        protected abstract string DownloadGroupName { get; }
        protected abstract string CacheFolderName { get; }
        protected abstract bool CanUseByteBasis { get; }

        public CacheProvider()
        {
            _lyricsCacheFolder = CreateCacheFolder();
            _transferGroup = BackgroundTransferGroup.CreateGroup(DownloadGroupName);
            _downloader = new BackgroundDownloader() { TransferGroup = _transferGroup };
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            AttachDownloadOperations();
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            if (!EnvironmentHelper.HasInternetConnection(!CanUseByteBasis))
            {
                lock (_netCtsLocker)
                {
                    if (_netCts != null)
                    {
                        _netCts.Cancel();
                        _netCts.Dispose();
                        _netCts = null;
                    }
                }
            }
        }

        private async void AttachDownloadOperations()
        {
            var operations = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(_transferGroup);
            lock (_activeDownloadLocker)
            operations.Apply(o =>
            {
                var fileName = Path.GetFileName(o.ResultFile.Path);
                if (!_activeDownloads.ContainsKey(fileName))
                    _activeDownloads.Add(fileName, HandleOperation(o, false));
            });
        }

        private async Task<IStorageFile> HandleOperation(DownloadOperation operation, bool start)
        {
            try
            {
                CancellationTokenSource cts;
                lock (_netCtsLocker)
                {
                    if (_netCts == null)
                    {
                        cts = new CancellationTokenSource();
                        _netCts = cts;
                    }
                    else
                        cts = _netCts;
                }
                if (start)
                    await operation.StartAsync().AsTask(cts.Token);
                else
                    await operation.AttachAsync().AsTask(cts.Token);
                var response = operation.GetResponseInformation();
                if (response.StatusCode == 200)
                    return operation.ResultFile;
            }
            catch { }
            finally
            {
                lock (_activeDownloadLocker)
                    _activeDownloads.Remove(Path.GetFileName(operation.ResultFile.Path));
            }
            return null;
        }

        protected async Task<IStorageFile> TryGetCache(string fileName)
        {
            try
            {
                Task<IStorageFile> task = null;
                lock (_activeDownloadLocker)
                    _activeDownloads.TryGetValue(fileName, out task);
                if (task != null)
                    return await task;
                else
                {
                    var file = await (await _lyricsCacheFolder).TryGetItemAsync(fileName) as StorageFile;
                    return file;
                }
            }
            catch { }
            return null;
        }

        protected Task<IStorageFile> Download(string fileName, Uri source)
        {
            if (EnvironmentHelper.HasInternetConnection(!CanUseByteBasis))
            {
                Task<IStorageFile> task;
                lock (_activeDownloadLocker)
                {
                    if (!_activeDownloads.TryGetValue(fileName, out task))
                    {
                        task = DownloadCore(fileName, source);
                        _activeDownloads.Add(fileName, task);
                    }
                }
                return task;
            }
            return Task.FromResult<IStorageFile>(null);
        }

        private async Task<IStorageFile> DownloadCore(string fileName, Uri source)
        {
            try
            {
                var file = await (await _lyricsCacheFolder).CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                var operation = _downloader.CreateDownload(source, file);
                return await HandleOperation(operation, true);
            }
            finally
            {
                lock (_activeDownloadLocker)
                    _activeDownloads.Remove(fileName);
            }
        }

        private async Task<StorageFolder> CreateCacheFolder()
        {
            return await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(LyricsCacheFolderName, CreationCollisionOption.OpenIfExists);
        }
    }
}
