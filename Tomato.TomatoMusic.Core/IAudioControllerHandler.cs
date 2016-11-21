using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc;
using Tomato.TomatoMusic.Primitives;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.Core
{
    [RpcPeer]
    public interface IAudioControllerHandler
    {
        void NotifyReady();
        void OnMediaPlaybackStateChanged(MediaPlaybackState state);
        void OnCurrentTrackChanged(TrackInfo track);
        void OnNaturalDurationChanged(TimeSpan duration);
    }
}
