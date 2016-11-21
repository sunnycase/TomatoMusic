using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.Media;

namespace Tomato.TomatoMusic.Services
{
    public interface IPlaySessionService : INotifyPropertyChanged
    {
        bool CanPrevious { get; }
        bool CanPause { get; }
        bool CanPlay { get; }
        bool ShowPause { get; }
        bool ShowPlay { get; }
        bool CanNext { get; }
        IReadOnlyList<TrackInfo> Playlist { get; }
        TrackInfo CurrentTrack { get; }
        MediaPlaybackStatus PlaybackStatus { get; }
        TimeSpan? Duration { get; }
        TimeSpan Position { get; set; }
        bool IsMuted { get; set; }
        double Volume { get; set; }
        IPlayModeProvider PlayMode { get; }

        void RequestPlay();
        void RequestPause();
        void RequestPrevious();
        void RequestNext();
        void SetPlaylist(IReadOnlyList<TrackInfo> tracks, TrackInfo current);
        void PlayWhenOpened();
        void ScrollPlayMode();
    }
}
