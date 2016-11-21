using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Services
{
    public interface IPlayModeProvider
    {
        Guid Id { get; }

        string DisplayName { get; }

        Symbol Icon { get; }

        TrackInfo SelectNextTrack(IReadOnlyList<TrackInfo> playlist, TrackInfo current);
    }
}
