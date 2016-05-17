using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Services;
using Tomato.Mvvm;
using Windows.System;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class AboutViewModel : BindableBase
    {
        public AboutViewModel()
        {
        }

        public async void LaunchIYinYong()
        {
            await Launcher.LaunchUriAsync(new Uri("ayywin:"));
        }
    }
}
