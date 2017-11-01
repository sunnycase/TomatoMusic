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
using Tomato.TomatoMusic.Shell.Models;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Tomato.TomatoMusic.Messages;
using System.Reflection;
using Tomato.TomatoMusic.Shell.ViewModels.Playing;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    partial class MainViewModel : Screen, IHandle<NavigateMainMenuMessage>, IHandle<ResumeStateMessage>, IHandle<SuspendStateMessage>
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private INavigationService _navigationService;

        public IPlaySessionService PlaySession { get; }
        public IPlaylistManager PlaylistManager { get; }
        public IThemeService ThemeService { get; }

        public static MainViewModel Current { get; private set; }

        private bool _resumeStateOnSetupNavigation = false;

        public MainViewModel(WinRTContainer container, IEventAggregator eventAggregator,
            IPlaySessionService playSessionService, IPlaylistManager playlistManager, IThemeService themeService)
        {
            Current = this;
            eventAggregator.Subscribe(this);
            _container = container;
            _eventAggregator = eventAggregator;
            PlaySession = playSessionService;
            PlaylistManager = playlistManager;
            ThemeService = themeService;
            SolidMenuItems = LoadSolidMenuItems();
            SetupStatusBarOrTitleBar();
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
            _navigationService = _container.RegisterNavigationService((Frame)sender);
            _navigationService.Navigated += navigationService_Navigated;
            if (_resumeStateOnSetupNavigation || _navigationService.CurrentSourcePageType == null)
            {
                ResumeNavigationState();
                _resumeStateOnSetupNavigation = false;
            }
        }

        private void navigationService_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            SendNavigatedMessage(e.Parameter as string);
        }

        private void SendNavigatedMessage(string absoluteUri)
        {
            _eventAggregator.BeginPublishOnUIThread(new MainMenuNavigatedMessage
            {
                AbsoluteUri = absoluteUri
            });
            SetupBackButton(_navigationService?.CanGoBack ?? false);
        }

        private static readonly Lazy<Func<INavigationService, string>> _currentParameterGetter = new Lazy<Func<INavigationService, string>>(() =>
        {
            var property = typeof(FrameAdapter).GetProperty("CurrentParameter", BindingFlags.Instance | BindingFlags.NonPublic);
            var getter = property.GetMethod;
            return o => getter.Invoke(o, null) as string;
        });

        private void ResumeNavigationState()
        {
            if (!_navigationService.ResumeState() || _navigationService.CurrentSourcePageType == null)
                SolidMenuItems.First().OnClick();
            else
            {
                SendNavigatedMessage(_currentParameterGetter.Value(_navigationService));
            }
        }

        private void SuspendNavigationState()
        {
            _navigationService?.SuspendState();
        }

        public void NavigateToSettings()
        {
            _eventAggregator.BeginPublishOnUIThread(NavigationConstants.Settings);
        }

        private async void SetupStatusBarOrTitleBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = Color.FromArgb(255, 0x6B, 0xCC, 0xAD);
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.BackgroundColor = Color.FromArgb(255, 0x6B, 0xCC, 0xAD);
                    titleBar.ForegroundColor = Colors.White;
                }
            }
        }

        private void SetupBackButton(bool canGoBack)
        {
            var navManager = SystemNavigationManager.GetForCurrentView();
            navManager.AppViewBackButtonVisibility = canGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        void IHandle<NavigateMainMenuMessage>.Handle(NavigateMainMenuMessage message)
        {
            _navigationService?.NavigateToViewModel(message.ViewModelType, message.Uri.AbsoluteUri);
        }

        void IHandle<ResumeStateMessage>.Handle(ResumeStateMessage message)
        {
            if (_navigationService == null)
                _resumeStateOnSetupNavigation = true;
            else
                ResumeNavigationState();
        }

        void IHandle<SuspendStateMessage>.Handle(SuspendStateMessage message)
        {
            SuspendNavigationState();
        }
    }
}