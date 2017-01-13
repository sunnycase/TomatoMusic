using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Mvvm;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.UI.Xaml.Media;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playing
{
    class CoverViewModel : Screen
    {
        private TrackInfo _track;
        public TrackInfo Track
        {
            get { return _track; }
            private set
            {
                if (this.SetProperty(ref _track, value))
                    OnTrackChanged();
            }
        }

        private TrackMetadataViewModel _metadata;
        public TrackMetadataViewModel Metadata
        {
            get { return _metadata; }
            private set { this.SetProperty(ref _metadata, value); }
        }

        private readonly IPlaySessionService _playSession;

        public CoverViewModel(IPlaySessionService playSession)
        {
            _playSession = playSession;
            Track = playSession.CurrentTrack;
            playSession.PropertyChanged += PlaySession_PropertyChanged;
        }

        private void PlaySession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPlaySessionService.CurrentTrack))
                Track = _playSession.CurrentTrack;
        }

        private void OnTrackChanged()
        {
            var track = Track;
            if (track != null)
                Metadata = new TrackMetadataViewModel(track);
            else
                Metadata = null;
        }

        internal class TrackMetadataViewModel : BindableBase
        {
            private ImageSource _cover;
            public ImageSource Cover
            {
                get { return _cover; }
                private set { SetProperty(ref _cover, value); }
            }

            private readonly TrackInfo _track;

            public TrackMetadataViewModel(TrackInfo track)
            {
                _track = track;
                LoadMetadata(track);
            }

            private async void LoadMetadata(TrackInfo track)
            {
                var metaService = IoC.Get<IMediaMetadataService>();
                var meta = await metaService.GetMetadata(track);
                meta.PropertyChanged += Meta_PropertyChanged;
                Cover = meta.Cover;
            }

            private void Meta_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(ITrackMediaMetadata.Cover):
                        Cover = ((ITrackMediaMetadata)sender).Cover;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
