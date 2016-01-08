using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
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

        public ThemeService(IConfigurationService configurationService)
        {
            ThemeConfiguration = configurationService.Theme;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            LoadBackground();
        }

        private static readonly Uri _packageBackground = new Uri(@"ms-appx:///Assets/background.jpg");
        private const string _backgroundCacheFileName = "backgroundCache.jpg";
        private const string _networkBackgroundDownloadFileName = "backgroundDownload.jpg";
        private static readonly Uri _networkBackgroundUri = new Uri("http://area.sinaapp.com/bingImg");
        private CancellationTokenSource _netCts;
        private readonly object _netCtsLocker = new object();

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
                var downloader = new BackgroundDownloader();
                var existsTask = (await BackgroundDownloader.GetCurrentDownloadsAsync()).SingleOrDefault(o => o.RequestedUri == _networkBackgroundUri);
                if (existsTask != null)
                    HandleBackgroundDownload(existsTask, false);
                else
                {
                    if (EnvironmentHelper.HasInternetConnection(!ThemeConfiguration.UpdateBackgroundEvenByteBasis))
                    {
                        var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(_networkBackgroundDownloadFileName, CreationCollisionOption.ReplaceExisting);
                        var operation = downloader.CreateDownload(_networkBackgroundUri, file);
                        HandleBackgroundDownload(operation, true);
                    }
                }
            }
            catch { }
        }

        private async void HandleBackgroundDownload(DownloadOperation operation, bool start)
        {
            var cts = new CancellationTokenSource();
            lock (_netCtsLocker)
            _netCts = cts;
            try
            {
                if (start)
                    await operation.StartAsync().AsTask(cts.Token);
                else
                    await operation.AttachAsync().AsTask(cts.Token);
                var response = operation.GetResponseInformation();
                if (response.StatusCode == 200)
                    await SaveNetworkBackgroundToCache();
            }
            catch { }
        }

        private async Task SaveNetworkBackgroundToCache()
        {
            var folder = ApplicationData.Current.LocalCacheFolder;
            var file = await folder.TryGetItemAsync(_networkBackgroundDownloadFileName) as StorageFile;
            if (file != null)
            {
                await file.RenameAsync(_backgroundCacheFileName, NameCollisionOption.ReplaceExisting);
                await TryLoadBackgroundCache();
            }
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            if (!EnvironmentHelper.HasInternetConnection(!ThemeConfiguration.UpdateBackgroundEvenByteBasis))
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

        private async Task<bool> TryLoadBackgroundCache()
        {
            try
            {
                var file = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(_backgroundCacheFileName) as StorageFile;
                if (file != null)
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(await file.OpenReadAsync());
                    Background = bitmap;
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
