using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class SettingsViewModel : BindableBase
    {
        public IThemeService ThemeService { get; }

        public SettingsViewModel(IThemeService themeService)
        {
            ThemeService = themeService;
        }
    }
}
