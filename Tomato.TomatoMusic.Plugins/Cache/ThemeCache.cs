using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Services;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Tomato.TomatoMusic.Plugins.Cache
{
    class ThemeCache : CacheProvider
    {
        private readonly ThemeConfiguration _themeConfiguration;
        protected override string DownloadGroupName => "Tomato.TomatoMusic.Download.ThemeCache";
        protected override string CacheFolderName => "ThemeCache";
        protected override bool CanUseByteBasis => _themeConfiguration.UpdateBackgroundEvenByteBasis;

        private const string _backgroundCacheFileName = "backgroundCache.jpg";
        private const string _networkBackgroundDownloadFileName = "backgroundDownload.jpg";
        private static readonly Uri _networkBackgroundUri = new Uri("http://area.sinaapp.com/bingImg");

        public ThemeCache()
        {
            _themeConfiguration = IoC.Get<IConfigurationService>().Theme;
        }


        public async Task<IRandomAccessStream> TryGetBackground()
        {
            try
            {
                var file = await TryGetCache(_backgroundCacheFileName);
                if (file != null)
                    return await file.OpenReadAsync();
            }
            catch { }
            return null;
        }

        public async Task<IRandomAccessStream> DownloadBackground()
        {
            var file = await Download(_networkBackgroundDownloadFileName, _networkBackgroundUri);
            if (file != null)
            {
                await file.RenameAsync(_backgroundCacheFileName, NameCollisionOption.ReplaceExisting);
                return await TryGetBackground();
            }
            return null;
        }
    }
}
