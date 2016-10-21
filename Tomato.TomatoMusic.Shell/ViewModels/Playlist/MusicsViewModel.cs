using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Mvvm;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    class MusicsTrackViewModel : BindableBase
    {
        public TrackInfo Track { get; set; }
        
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public event EventHandler PlayRequested;

        public void Play()
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    class MusicsViewModel : Screen, IMusicsPresenterViewModel
    {
        private readonly PlaylistPlaceholder _playlist;
        private readonly IPlaylistContentProvider _playlistContentProvider;
        private readonly IPlaySessionService _playSession;

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { this.SetProperty(ref _isRefreshing, value); }
        }

        public IReadOnlyReactiveCollection<MusicsTrackViewModel> Tracks { get; }

        public MusicsOrderRule[] OrderRules { get; } = new MusicsOrderRule[]
        {

        };

        public event Action<MusicsTrackViewModel> RequestViewSelect;

        public MusicsViewModel(PlaylistPlaceholder playlist)
        {
            _playlist = playlist;
            _playlistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(_playlist);
            _playlistContentProvider.PropertyChanged += playlistContentProvider_PropertyChanged;
            Tracks = _playlistContentProvider.Tracks.CreateDerivedCollection(WrapTrackInfo);
            _playSession = IoC.Get<IPlaySessionService>();
            _playSession.PropertyChanged += _playSession_PropertyChanged;
        }

        protected override void OnViewLoaded(object view)
        {
            LoadData();
        }

        private void LoadData()
        {
            OnPlaySessionCurrentTrackChanged();
        }

        private void _playSession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaySessionService.CurrentTrack):
                    Execute.BeginOnUIThread(OnPlaySessionCurrentTrackChanged);
                    break;
                default:
                    break;
            }
        }

        private void playlistContentProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaylistContentProvider.IsRefreshing):
                    IsRefreshing = _playlistContentProvider.IsRefreshing;
                    break;
                default:
                    break;
            }
        }

        private void OnPlaySessionCurrentTrackChanged()
        {
            var track = _playSession.CurrentTrack;
            var trackVM = Tracks.FirstOrDefault(o => o.Track == track);
            if (trackVM != null)
                RequestViewSelect?.Invoke(trackVM);
        }

        private MusicsTrackViewModel WrapTrackInfo(TrackInfo track)
        {
            var trackViewModel = new MusicsTrackViewModel
            {
                Track = track
            };
            trackViewModel.PlayRequested += TrackViewModel_PlayRequested;
            return trackViewModel;
        }

        private void TrackViewModel_PlayRequested(object sender, EventArgs e)
        {
            var trackVM = (MusicsTrackViewModel)sender;
            _playSession.SetPlaylist(_playlistContentProvider.Tracks.ToList(), trackVM.Track);
            _playSession.PlayWhenOpened();
        }
    }
}
