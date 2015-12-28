using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.Core
{
    public interface IAudioControllerHandler
    {
        void NotifyControllerReady();
        void NotifyMediaOpened();
        void NotifyControllerStateChanged(MediaPlayerState state);
        void NotifyCurrentTrackChanged(TrackInfo track);
        void NotifyDuration(TimeSpan? duration);
        void NotifyPosition(TimeSpan position);
        void NotifySeekCompleted();
    }
}
