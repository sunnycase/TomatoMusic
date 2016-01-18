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
        void MoveNext();
        void MovePrevious();
        void SetPlayMode(Guid id);
        void AskPosition();
        void SetPosition(TimeSpan position);
        void SetVolume(double value);
        void AskPlaylist();
        void AskCurrentTrack();
        void AskCurrentState();
        void AskDuration();
        void SetEqualizerParameter(float frequency, float bandWidth, float gain);
        void ClearEqualizerParameter(float frequency);
    }
}
