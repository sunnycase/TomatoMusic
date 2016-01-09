using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.System;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class AboutViewModel : BindableBase
    {
        public IThemeService ThemeService { get; }

        public AboutViewModel(IThemeService themeService)
        {
            ThemeService = themeService;
        }

        public async void LaunchIYinYong()
        {
            await Launcher.LaunchUriAsync(new Uri("ayywin:"));
        }
    }
}
