using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    partial class MainViewModel : Screen
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private Frame _navigationService;

        public IPlaySessionService PlaySession { get; }
        public IPlaylistManager PlaylistManager { get; }

        public MainViewModel(WinRTContainer container, IEventAggregator eventAggregator,
            IPlaySessionService playSessionService, IPlaylistManager playlistManager)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            PlaySession = playSessionService;
            PlaylistManager = playlistManager;
            PlaylistManager.PropertyChanged += PlaylistManager_PropertyChanged;
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
            _navigationService = (Frame)sender;
            NavigateToSelectedPlaylist();
        }

        private void PlaylistManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaylistManager.SelectedPlaylist):
                    OnSelectedPlaylistChanged();
                    break;
                default:
                    break;
            }
        }
    }
}