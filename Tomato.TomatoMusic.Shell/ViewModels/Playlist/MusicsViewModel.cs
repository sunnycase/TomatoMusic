using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;

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

        public void OnRequestPlay()
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    class MusicsViewModel : Screen
    {
        private readonly IPlaylistAnchor _anchor;
        private IPlaylistContentProvider _playlistContentProvider;
        private IObservableCollection<TrackInfo> _tracksSource;
        private readonly IPlaySessionService _playSession;

        public BindableCollection<MusicsTrackViewModel> Tracks { get; private set; }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { this.SetProperty(ref _isRefreshing, value); }
        }

        private bool _loadStarted;

        public event Action<MusicsTrackViewModel> RequestViewSelect;

        public MusicsViewModel(IPlaylistAnchor anchor)
        {
            _anchor = anchor;
            _playSession = IoC.Get<IPlaySessionService>();
            _playSession.PropertyChanged += _playSession_PropertyChanged;
        }

        protected override void OnViewLoaded(object view)
        {
            if (!_loadStarted)
            {
                _loadStarted = true;
                LoadData();
            }
        }

        private async void LoadData()
        {
            IsRefreshing = true;
            try
            {
                _playlistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(_anchor);
                _tracksSource = await _playlistContentProvider.Result;
                Tracks = new BindableCollection<MusicsTrackViewModel>(_tracksSource.Select(WrapTrackInfo));
                NotifyOfPropertyChange(nameof(Tracks));
                _tracksSource.CollectionChanged += tracksSource_CollectionChanged;
                OnPlaySessionCurrentTrackChanged();
            }
            finally
            {
                IsRefreshing = false;
            }
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

        private void OnPlaySessionCurrentTrackChanged()
        {
            var track = _playSession.CurrentTrack;
            var trackVM = Tracks.SingleOrDefault(o => o.Track == track);
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
            _playSession.SetPlaylist(_tracksSource.ToList(), trackVM.Track);
            _playSession.PlayWhenOpened();
        }

        private void tracksSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    Tracks.AddRange(e.NewItems.Cast<TrackInfo>().Select(WrapTrackInfo));
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Tracks.RemoveRange(Tracks.Where(o => e.OldItems.Cast<TrackInfo>().Any(t => t == o.Track)).ToList());
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Tracks.Clear();
                    Tracks.AddRange(_tracksSource.Select(WrapTrackInfo));
                    break;
                default:
                    break;
            }
        }
    }
}
