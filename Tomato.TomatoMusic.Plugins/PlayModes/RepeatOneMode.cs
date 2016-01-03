using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Plugins.PlayModes
{
    class RepeatOneMode : IPlayModeProvider
    {
        public string DisplayName => "单曲模式";

        private static readonly Guid _id = new Guid("8513750B-6423-4B68-8DA1-98538EFE2493");
        public Guid Id => _id;

        public Symbol Icon => Symbol.RepeatOne;

        public TrackInfo SelectNextTrack(IList<TrackInfo> playlist, TrackInfo current)
        {
            return current;
        }
    }
}
