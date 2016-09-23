using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;
using Tomato.TomatoMusic.Services;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    partial class MainViewModel : Screen
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private INavigationService _navigationService;

        public IPlaySessionService PlaySession { get; }
        public IPlaylistManager PlaylistManager { get; }
        public IThemeService ThemeService { get; }

        public static MainViewModel Current { get; private set; }

        public MainViewModel(WinRTContainer container, IEventAggregator eventAggregator,
            IPlaySessionService playSessionService, IPlaylistManager playlistManager, IThemeService themeService)
        {
            Current = this;
            _container = container;
            _eventAggregator = eventAggregator;
            PlaySession = playSessionService;
            PlaylistManager = playlistManager;
            ThemeService = themeService;
            SolidMenuItems = LoadSolidMenuItems();
            SetupStatusBar();
        }

        protected override void OnActivate()
        {
            _eventAggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
        }

        public void SetupNavigationService(object sender, object e)
        {
            _navigationService = _container.RegisterNavigationService((Frame)sender, false, true);
        }

        public void NavigateToSettings()
        {
            _navigationService?.For<SettingsViewModel>()?.Navigate();
        }

        private async void SetupStatusBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Colors.Black;
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundOpacity = 1.0;
                await statusBar.ShowAsync();
            }
        }
    }
}