using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc;
using Tomato.TomatoMusic.Primitives;
using Windows.Media;

namespace Tomato.TomatoMusic.Core
{
    [RpcPeer]
    public interface IAudioController
    {
        void AskIfReady();
        void SetPlaylist(IReadOnlyList<TrackInfo> tracks, TrackInfo nextTrack, bool autoPlay = true);
        void SetCurrentTrack(TrackInfo track);
        void OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton button);
        Task<TimeSpan> GetPosition();
        Task Seek(TimeSpan position);
        void SetVolume(double volume);
        void SetPlayMode(Guid id);
    }
}
