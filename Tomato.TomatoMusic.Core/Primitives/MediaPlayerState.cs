using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Primitives
{
    public enum MediaPlayerState
    {
        Closed,
        PlaybackListSet,
        MediaOpening,
        MediaOpened,
        Buffering,
        StartPlaying,
        Playing,
        Pausing,
        Paused,
        Ended,
        Error
    }
}
