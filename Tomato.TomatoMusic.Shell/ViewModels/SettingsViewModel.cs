using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class SettingsViewModel : BindableBase
    {
        public ThemeConfiguration ThemeConfiguration { get; }
        public MetadataConfiguration MetadataConfiguration { get; }
        public IThemeService ThemeService { get; }

        public SettingsViewModel(IConfigurationService configurationService, IThemeService themeService)
        {
            ThemeService = themeService;
            ThemeConfiguration = configurationService.Theme;
            MetadataConfiguration = configurationService.Metadata;

            ThemeConfiguration.PropertyChanged += Configuration_PropertyChanged;
            MetadataConfiguration.PropertyChanged += Configuration_PropertyChanged;
        }

        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ((ConfigurationBase)sender).Save();
        }
    }
}
