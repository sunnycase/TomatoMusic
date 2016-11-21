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
    class RepeatAllMode : IPlayModeProvider
    {
        public string DisplayName => "循环列表";

        private static readonly Guid _id = new Guid("4009F9C9-B5B1-4FE6-AADF-B6278B1040CD");
        public Guid Id => _id;

        public Symbol Icon => Symbol.RepeatAll;

        public TrackInfo SelectNextTrack(IReadOnlyList<TrackInfo> playlist, TrackInfo current)
        {
            var idx = playlist.IndexOf(current);
            if (idx != -1)
            {
                var nextId = idx + 1;
                if (nextId > 0 && nextId < playlist.Count)
                    return playlist[nextId];
            }
            return playlist.FirstOrDefault();
        }
    }
}
