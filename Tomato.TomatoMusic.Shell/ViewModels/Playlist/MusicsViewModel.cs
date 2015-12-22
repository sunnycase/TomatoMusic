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
    class MusicsViewModel : Screen
    {
        private readonly IPlaylistAnchor _anchor;
        private IPlaylistContentProvider _playlistContentProvider;
        private IObservableCollection<TrackInfo> _tracksSource;

        public ICollection<TrackInfo> Tracks => _tracksSource;

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
            if(!_loadStarted)
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
                NotifyOfPropertyChange(nameof(Tracks));
                _tracksSource.CollectionChanged += tracksSource_CollectionChanged;
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void tracksSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }
    }
}
