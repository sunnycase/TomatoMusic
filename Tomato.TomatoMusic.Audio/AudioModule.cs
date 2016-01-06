using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Audio.Services;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic
{
    public static class AudioModule
    {
        public static void UseAudio(this SimpleContainer container)
        {
            container.Singleton<IPlaySessionService, PlaySessionService>();
        }
    }
}
