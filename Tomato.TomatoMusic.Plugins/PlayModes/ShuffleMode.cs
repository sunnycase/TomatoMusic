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
    class ShuffleMode : IPlayModeProvider
    {
        public string DisplayName => "随机播放";

        private static readonly Guid _id = new Guid("66776710-2E10-4B80-B7B5-082E02E92131");
        public Guid Id => _id;

        public Symbol Icon => Symbol.Shuffle;
        private readonly Random _rand = new Random();

        public TrackInfo SelectNextTrack(IReadOnlyList<TrackInfo> playlist, TrackInfo current)
        {
            if (playlist.Count == 0)
                return playlist.FirstOrDefault();
            return playlist[_rand.Next(playlist.Count)];
        }
    }
}
