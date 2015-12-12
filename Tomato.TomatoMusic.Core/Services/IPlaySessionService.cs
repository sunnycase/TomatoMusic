using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        MediaPlaybackStatus PlaybackStatus { get; }

        void RequestPlay();
        void RequestPause();
    }
}
