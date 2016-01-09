using Caliburn.Micro;
using Kfstorm.LrcParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playing
{
    class PlayingViewModel : Screen
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

        public IThemeService Theme { get; }

        private readonly IPlaySessionService _playSession;
        private ListBox _lyricsListBox;

        public PlayingViewModel(IPlaySessionService playSession, IThemeService themeService)
        {
            Theme = themeService;
            _playSession = playSession;
            Track = playSession.CurrentTrack;
            playSession.PropertyChanged += PlaySession_PropertyChanged;
        }

        private void PlaySession_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPlaySessionService.CurrentTrack))
                Track = _playSession.CurrentTrack;
            else if (e.PropertyName == nameof(IPlaySessionService.Position))
            {
                var listBox = _lyricsListBox;
                if (listBox != null)
                    Metadata?.OnPositionChanged(listBox, _playSession.Position);
            }
        }

        private void OnTrackChanged()
        {
            var track = Track;
            if (track != null)
                Metadata = new TrackMetadataViewModel(track);
            else
                Metadata = null;
        }

        public void SetLyricsListBox(ListBox listBox)
        {
            _lyricsListBox = listBox;
        }

        private static readonly WeakReference<PlayingViewModel> _viewModel = new WeakReference<PlayingViewModel>(null);
        public static PlayingViewModel Activate()
        {
            PlayingViewModel viewModel;
            if (!_viewModel.TryGetTarget(out viewModel))
            {
                viewModel = IoC.Get<PlayingViewModel>();
                _viewModel.SetTarget(viewModel);
            }
            return viewModel;
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

        private ILrcFile _lyricModel;
        public ILrcFile LyricModel
        {
            get { return _lyricModel; }
            private set
            {
                if (SetProperty(ref _lyricModel, value))
                    OnPropertyChanged(nameof(Lyrics));
            }
        }

        public IList<IOneLineLyric> Lyrics => _lyricModel?.Lyrics;

        public TrackMetadataViewModel(TrackInfo track)
        {
            LoadMetadata(track);
        }

        private async void LoadMetadata(TrackInfo track)
        {
            var metaService = IoC.Get<IMediaMetadataService>();
            var meta = await metaService.GetMetadata(track);
            meta.PropertyChanged += Meta_PropertyChanged;
            Cover = meta.Cover;
            TryAnalyzeLyrics(meta.Lyrics);
        }

        private void Meta_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ITrackMediaMetadata.Cover):
                    Cover = ((ITrackMediaMetadata)sender).Cover;
                    break;
                case nameof(ITrackMediaMetadata.Lyrics):
                    TryAnalyzeLyrics(((ITrackMediaMetadata)sender).Lyrics);
                    break;
                default:
                    break;
            }
        }

        private void TryAnalyzeLyrics(string lyrics)
        {
            if (!string.IsNullOrEmpty(lyrics))
            {
                try
                {
                    LyricModel = LrcFile.FromText(lyrics);
                }
                catch { }
            }
        }

        public void OnPositionChanged(ListBox listBox, TimeSpan position)
        {
            var lyrics = LyricModel;
            if (lyrics != null && lyrics.Lyrics != null)
            {
                var selected = lyrics.BeforeOrAt(position);
                if (selected != null)
                {
                    listBox.SelectedItem = selected;
                    var display = lyrics.Lyrics.SkipWhile(o => o != selected).Skip(3).FirstOrDefault();
                    if (display != null)
                        listBox.ScrollIntoView(display);
                    else
                        listBox.ScrollIntoView(selected);
                }
            }
        }
    }
}
