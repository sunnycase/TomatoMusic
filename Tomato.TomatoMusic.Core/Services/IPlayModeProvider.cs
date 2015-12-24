using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;

namespace Tomato.TomatoMusic.Services
{
    public interface IPlayModeProvider
    {
        Guid Id { get; }

        string DisplayName { get; }

        TrackInfo SelectNextTrack(IList<TrackInfo> playlist, TrackInfo current);
    }
}
