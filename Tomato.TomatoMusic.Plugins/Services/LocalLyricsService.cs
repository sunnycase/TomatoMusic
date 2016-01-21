using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Plugins.Providers;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Tomato.TomatoMusic.Plugins.Services
{
    class LocalLyricsService : ILocalLyricsService
    {
        private readonly LocalLyricsProvider _provider;

        public LocalLyricsService()
        {
            _provider = new LocalLyricsProvider();
        }

        public void ClearLocalLyricsPath(TrackInfo track)
        {
            try
            {
                _provider.ClearLocalLyricsPath(track);
            }
            catch { }
        }

        public void SetLocalLyrics(TrackInfo track, IStorageFile storageFile)
        {
            try
            {
                StorageApplicationPermissions.FutureAccessList.Add(storageFile);
                _provider.SetLocalLyricsPath(track, storageFile.Path);
            }
            catch { }
        }

        public async Task<IStorageFile> TryGetLocalLyrics(TrackInfo track)
        {
            try
            {
                var path = _provider.TryGetLocalLyricsPath(track);
                if (!string.IsNullOrEmpty(path))
                    return await StorageFile.GetFileFromPathAsync(path);
            }
            catch { }
            return null;
        }
    }
}
