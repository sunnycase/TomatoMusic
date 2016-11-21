using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.Media;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.Services
{
    public interface IMediaTransportService
    {
        bool IsEnabled { get; set; }
        bool IsPlayEnabled { get; set; }
        bool IsPauseEnabled { get; set; }
        MediaPlaybackStatus PlaybackStatus { get; }
        bool CanNext { get; set; }
        bool CanPrevious { get; set; }

        event EventHandler<SystemMediaTransportControlsButtonPressedEventArgs> ButtonPressed;

        void SetCurrentTrack(TrackInfo value);
        void SetPlaybackState(MediaPlaybackState state);
    }
}
