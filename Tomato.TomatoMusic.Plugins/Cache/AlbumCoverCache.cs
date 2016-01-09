using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Services;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Tomato.TomatoMusic.Plugins.Cache
{
    class AlbumCoverCache : CacheProvider
    {
        private readonly MetadataConfiguration _metadataConfiguration;
        protected override string DownloadGroupName => "Tomato.TomatoMusic.Download.AlbumCache";
        protected override string CacheFolderName => "AlbumCache";
        protected override bool CanUseByteBasis => _metadataConfiguration.UpdateLyricsEvenByteBasis;

        public AlbumCoverCache()
        {
            _metadataConfiguration = IoC.Get<IConfigurationService>().Metadata;
        }

        public async Task<IRandomAccessStream> TryGetCache(string albumName, string artist)
        {
            try
            {
                var file = await TryGetCache(GetFileName(albumName, artist));
                if (file != null)
                    return await file.OpenReadAsync();
            }
            catch { }
            return null;
        }

        public async Task<IRandomAccessStream> Download(string albumName, string artist, Uri source)
        {
            return await (await Download(GetFileName(albumName, artist), source))?.OpenReadAsync();
        }

        private string GetFileName(string albumName, string artist)
        {
            return $"{albumName}-{artist}.png";
        }
    }
}
