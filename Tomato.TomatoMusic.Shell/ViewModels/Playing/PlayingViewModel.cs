using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.UI.Xaml.Media;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playing
{
    class PlayingViewModel : Screen
    {
        private TrackInfo _track;
        public TrackInfo Track
        {
            get { return _track; }
            private set { this.SetProperty(ref _track, value); }
        }

        private TrackMetadataViewModel _metadata;
        public TrackMetadataViewModel Metadata
        {
            get { return _metadata; }
            private set { this.SetProperty(ref _metadata, value); }
        }

        private readonly IPlaySessionService _playSession;

        public PlayingViewModel(IPlaySessionService playSession)
        {
            _playSession = playSession;
            OnCurrentTrackChanged();
            playSession.PropertyChanged += PlaySession_PropertyChanged;
        }

        private void PlaySession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPlaySessionService.CurrentTrack))
                OnCurrentTrackChanged();
        }

        private void OnCurrentTrackChanged()
        {
            var track = _playSession.CurrentTrack;
            Track = track;
            if (track != null)
                Metadata = new TrackMetadataViewModel(track);
            else
                Metadata = null;
        }
    }

    class TrackMetadataViewModel : BindableBase
    {
        private ImageSource _cover;
        public ImageSource Cover
        {
            get { return _cover; }
            private set { SetProperty(ref _cover, value); }
        }

        private string _lyrics;
        public string Lyrics
        {
            get { return _lyrics; }
            private set { SetProperty(ref _lyrics, value); }
        }

        public TrackMetadataViewModel(TrackInfo track)
        {
            LoadMetadata(track);
        }

        private async void LoadMetadata(TrackInfo track)
        {
            var metaService = IoC.Get<IMediaMetadataService>();
            var meta = await metaService.GetMetadata(track);
            Cover = meta.Cover;
            Lyrics = meta.Lyrics;
        }
    }
}
