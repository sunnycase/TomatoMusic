using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
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

        public ThemeService()
        {
            LoadBackground();
        }

        private static readonly Uri _packageBackground = new Uri(@"ms-appx:///Assets/background.jpg");
        private const string _backgroundCacheFileName = "backgroundCache.jpg";

        private async void LoadBackground()
        {
            Background = new BitmapImage(_packageBackground);

            // 获取缓存
            await TryLoadBackgroundCache();
            // 获取即时图片
            await TryLoadNetworkBackground();
        }

        private async Task TryLoadNetworkBackground()
        {
            try
            {
                var internet = NetworkInformation.GetInternetConnectionProfile();
                if(internet.GetConnectionCost().NetworkCostType == NetworkCostType.Unrestricted)
                {
                    
                }
            }
            catch
            {

            }
        }

        private async Task TryLoadBackgroundCache()
        {
            try
            {
                var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(_backgroundCacheFileName);
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(await file.OpenReadAsync());
                Background = bitmap;
            }
            catch
            {

            }
        }
    }
}
