using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Plugins.Cache;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Tomato.TomatoMusic.Plugins.Services
{
    class ThemeService : BindableBase, IThemeService
    {
        private ImageSource _background;
        public ImageSource Background
        {
            get { return _background; }
            private set { SetProperty(ref _background, value); }
        }

        public ThemeConfiguration ThemeConfiguration { get; }

        private readonly ThemeCache _themeCache = new ThemeCache();

        public ThemeService(IConfigurationService configurationService)
        {
            ThemeConfiguration = configurationService.Theme;
            LoadBackground();
        }

        private static readonly Uri _packageBackground = new Uri(@"ms-appx:///Assets/background.jpg");

        private async void LoadBackground()
        {
            // 获取缓存
            if (!await TryLoadBackgroundCache())
                Background = new BitmapImage(_packageBackground);
            // 获取即时图片
            await TryLoadNetworkBackground();
        }

        private async Task TryLoadNetworkBackground()
        {
            try
            {
                var stream = await _themeCache.DownloadBackground();
                if (stream != null)
                    Background = await MediaHelper.CreateImage(stream);
            }
            catch { }
        }

        private async Task<bool> TryLoadBackgroundCache()
        {
            try
            {
                var stream = await _themeCache.TryGetBackground();
                if (stream != null)
                {
                    Background = await MediaHelper.CreateImage(stream);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
