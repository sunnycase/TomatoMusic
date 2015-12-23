using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;

namespace Tomato.TomatoMusic.Core
{
    public interface IAudioController
    {
        void SetupHandler();
        void Play();
        void Pause();
        void SetPlaylist(IList<TrackInfo> tracks);
        void SetCurrentTrack(TrackInfo track);
    }
}
