using Caliburn.Micro;
using Kfstorm.LrcParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Mvvm;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playing
{
    class LyricsViewModel : Screen
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
        private ListBox _lyricsListBox;

        public LyricsViewModel(IPlaySessionService playSession)
        {
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
            if (LyricModel == null)
                TryAnalyzeLyrics(meta.Lyrics);
        }

        public async void SetupLocalLyric()
        {
            var picker = new FileOpenPicker() { ViewMode = PickerViewMode.List };
            picker.FileTypeFilter.Add(".lrc");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    var content = await FileIO.ReadTextAsync(file);
                    TryAnalyzeLyrics(content);
                    var service = IoC.Get<ILocalLyricsService>();
                    service.SetLocalLyrics(_track, file);
                }
                catch { }
            }
        }

        public async void ResetLyricSetting()
        {
            await Task.Run(() =>
            {
                var service = IoC.Get<ILocalLyricsService>();
                service.ClearLocalLyricsPath(_track);
            });
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
