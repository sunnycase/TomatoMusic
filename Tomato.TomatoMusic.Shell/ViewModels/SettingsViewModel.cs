using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.System;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class SettingsViewModel : BindableBase
    {
        public ThemeConfiguration ThemeConfiguration { get; }
        public MetadataConfiguration MetadataConfiguration { get; }

        public SettingsViewModel(IConfigurationService configurationService)
        {
            ThemeConfiguration = configurationService.Theme;
            MetadataConfiguration = configurationService.Metadata;

            ThemeConfiguration.PropertyChanged += Configuration_PropertyChanged;
            MetadataConfiguration.PropertyChanged += Configuration_PropertyChanged;
        }

        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ((ConfigurationBase)sender).Save();
        }

        public void NavigateToAbout()
        {
            MainViewModel.Current?.NavigateToAbout();
        }

        public async void LaunchReview()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9WZDNCRDM7JW"));
        }
    }
}
