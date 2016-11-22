using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Plugins.PlayModes
{
    class ListMode : IPlayModeProvider
    {
        public string DisplayName => "列表模式";

        private static readonly Guid _id = new Guid("AC3A41B1-58C9-48D3-A4FD-D01CC548B29B");
        public Guid Id => _id;

        public Symbol Icon => Symbol.List;

        public void Attach(MediaPlaybackList playbackList)
        {
            playbackList.AutoRepeatEnabled = false;
            playbackList.ShuffleEnabled = false;
        }

        public void Detach()
        {

        }
    }
}
