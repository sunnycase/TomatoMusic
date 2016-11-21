using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Shell.Models;
using Tomato.TomatoMusic.Shell.ViewModels.Playing;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class NavigationConstants
    {
        public static readonly NavigateMainMenuMessage Settings;
        public static readonly NavigateMainMenuMessage Playing;

        static NavigationConstants()
        {
            Settings = new NavigateMainMenuMessage
            {
                Uri = new NavigateHelper<SettingsViewModel>().BuildUri(),
                ViewModelType = typeof(SettingsViewModel)
            };
            Playing = new NavigateMainMenuMessage
            {
                Uri = new NavigateHelper<PlayingViewModel>().BuildUri(),
                ViewModelType = typeof(PlayingViewModel)
            };
        }
    }
}
