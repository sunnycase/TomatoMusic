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
    class LyricsCache : CacheProvider
    {
        private readonly MetadataConfiguration _metadataConfiguration;
        protected override string DownloadGroupName => "Tomato.TomatoMusic.Download.LyricsCache";
        protected override string CacheFolderName => "LyricsCache";
        protected override bool CanUseByteBasis => _metadataConfiguration.UpdateLyricsEvenByteBasis;

        public LyricsCache()
        {
            _metadataConfiguration = IoC.Get<IConfigurationService>().Metadata;
        }

        public async Task<string> TryGetCache(string trackName, string artist)
        {
            try
            {
                var file = await TryGetCache(GetFileName(trackName, artist));
                if (file != null)
                    return await FileIO.ReadTextAsync(file);
            }
            catch { }
            return null;
        }

        public async Task<string> Download(string trackName, string artist, Uri source)
        {
            var file = await Download(GetFileName(trackName, artist), source);
            if (file != null)
                return await FileIO.ReadTextAsync(file);
            return null;
        }

        private string GetFileName(string trackName, string artist)
        {
            return $"{trackName}-{artist}.png";
        }
    }
}
