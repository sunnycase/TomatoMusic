using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tomato.TomatoMusic.Primitives;
using Tomato.Threading;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Tomato.TomatoMusic.Playlist.Cache
{
    class FolderCacheProvider
    {
        private readonly Guid _playlistKey;
        private readonly string _folderName;
        private readonly Task<StorageFile> _cacheFile;
        private readonly List<TrackInfo> _tracks = new List<TrackInfo>();
        private readonly OperationQueue _operationQueue = new OperationQueue(1);

        private ReadOnlyCollection<TrackInfo> _cachedTracks;
        private readonly object _cachedTracksLocker = new object();
        public IReadOnlyCollection<TrackInfo> Tracks
        {
            get
            {
                lock (_cachedTracksLocker)
                {
                    if (_cachedTracks == null)
                    {
                        lock (_tracks)
                            _cachedTracks = new ReadOnlyCollection<TrackInfo>(_tracks.ToList());
                    }
                    return _cachedTracks;
                }
            }
        }

        public event EventHandler TracksUpdated;

        public FolderCacheProvider(Guid playlistKey, string folderName)
        {
            _playlistKey = playlistKey;
            _folderName = folderName;
            _cacheFile = OpenCacheFile();
        }

        public async Task LoadCache()
        {
            await _operationQueue.Queue(async () =>
            {
                var content = await FileIO.ReadTextAsync(await _cacheFile);
                lock (_tracks)
                {
                    _tracks.Clear();
                    var toAdd = JsonConvert.DeserializeObject<FolderCache>(content)?.Tracks;
                    if (toAdd != null)
                        _tracks.AddRange(toAdd.Distinct());
                }
                OnTracksUpdated();
            });
        }

        public async Task Update(IEnumerable<TrackInfo> value)
        {
            await _operationQueue.Queue(() =>
            {
                lock (_tracks)
                {
                    var comparer = new TrackInfo.ExistenceEqualityComparer();
                    var toRemove = _tracks.Except(value, comparer).ToList();
                    var toAdd = value.Except(_tracks, comparer).ToList();
                    foreach (var track in toRemove)
                        _tracks.Remove(track);
                    _tracks.AddRange(toAdd);
                }
                OnTracksUpdated();
                QueueSychronize();
            });
        }

        private async void QueueSychronize()
        {
            await _operationQueue.Queue(async () =>
            {
                string content;
                lock (_tracks)
                {
                    content = JsonConvert.SerializeObject(new FolderCache
                    {
                        Tracks = _tracks.Distinct().ToList()
                    });
                }
                using (var writer = await (await _cacheFile).OpenTransactedWriteAsync())
                {
                    var stream = writer.Stream;
                    stream.Size = 0;
                    stream.Seek(0);
                    using (var dw = new DataWriter(stream))
                    {
                        dw.WriteString(content);
                        await dw.StoreAsync();
                        await dw.FlushAsync();
                        dw.DetachStream();
                    }
                    await writer.CommitAsync();
                }
            });
        }

        private void OnTracksUpdated()
        {
            lock (_cachedTracksLocker)
                _cachedTracks = null;
            TracksUpdated?.Invoke(this, EventArgs.Empty);
        }

        private async Task<StorageFile> OpenCacheFile()
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(_playlistKey.ToString("N"), CreationCollisionOption.OpenIfExists);
            return await folder.CreateFileAsync(Uri.EscapeDataString(HashFolderName(_folderName)), CreationCollisionOption.OpenIfExists);
        }

        private static readonly HashAlgorithmProvider _sha1 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);

        private string HashFolderName(string path)
        {
            var bufPath = CryptographicBuffer.ConvertStringToBinary(path, BinaryStringEncoding.Utf8);
            var hash = _sha1.HashData(bufPath);
            return CryptographicBuffer.EncodeToBase64String(hash);
        }
    }
}
