using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    class MusicsTrackViewModel
    {
        public TrackInfo Track { get; set; }

        public void OnRequestPlay()
        {

        }
    }

    class MusicsViewModel : Screen
    {
        private readonly IPlaylistAnchor _anchor;
        private IPlaylistContentProvider _playlistContentProvider;
        private IObservableCollection<TrackInfo> _tracksSource;

        public BindableCollection<MusicsTrackViewModel> Tracks { get; private set; }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { this.SetProperty(ref _isRefreshing, value); }
        }

        private bool _loadStarted;

        public MusicsViewModel(IPlaylistAnchor anchor)
        {
            _anchor = anchor;
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
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private MusicsTrackViewModel WrapTrackInfo(TrackInfo track)
        {
            return new MusicsTrackViewModel
            {
                Track = track
            };
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
                    Tracks.AddRange(e.NewItems.Cast<TrackInfo>().Select(WrapTrackInfo));
                    break;
                default:
                    break;
            }
        }
    }
}
