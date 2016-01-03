using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Shell.ViewModels;
using Windows.ApplicationModel.Resources;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Shell.Services;
using Tomato.TomatoMusic.Shell.ViewModels.Playing;

namespace Tomato.TomatoMusic
{
    static class ShellModule
    {
        public static void UseShell(this SimpleContainer container)
        {
            container.PerRequest<MainViewModel>();
            container.PerRequest<PlaylistViewModel>();
            container.PerRequest<PlayingViewModel>();
            container.Instance(ResourceLoader.GetForCurrentView());
            container.Singleton<IConfigurationService, ConfigurationService>();
        }

        private static void ModuleReferences()
        {
            var type = new[]
            {
                typeof(Caliburn.Micro.ViewModelBinder)
            };
        }
    }
}
