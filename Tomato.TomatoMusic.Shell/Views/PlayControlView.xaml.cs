using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Shell.ViewModels;
using Tomato.Mvvm.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Caliburn.Micro;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tomato.TomatoMusic.Shell.ViewModels.Playing;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Tomato.TomatoMusic.Shell.Views
{
    public sealed partial class PlayControlView : UserControl, INotifyPropertyChanged
    {
        public IPlaySessionService PlaySession { get; } = Execute.InDesignMode ? null : IoC.Get<IPlaySessionService>();
        private readonly IMediaMetadataService _mediaMetadataService = IoC.Get<IMediaMetadataService>();

        private ImageSource _trackCover;
        public ImageSource TrackCover
        {
            get { return _trackCover; }
            set
            {
                if (_trackCover != value)
                {
                    _trackCover = value;
                    OnPropertyChanged();
                }
            }
        }

        public PlayControlView()
        {
            this.InitializeComponent();
            PlaySession.PropertyChanged += PlaySession_PropertyChanged;
            UpdateTrackCover();
        }

        private void PlaySession_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaySessionService.CurrentTrack):
                    UpdateTrackCover();
                    break;
                default:
                    break;
            }
        }

        private async void UpdateTrackCover()
        {
            var track = PlaySession.CurrentTrack;
            if (track == null)
                TrackCover = null;
            else
            {
                var metadata = await _mediaMetadataService.GetMetadata(track);
                TrackCover = metadata?.Cover;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnTrackTileClick()
        {
            IoC.Get<INavigationService>()?.For<PlayingViewModel>().Navigate();
        }
    }
}
