using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Audio.Services;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Audio.Config;

namespace Tomato.TomatoMusic
{
    public static class AudioModule
    {
        public static AudioModuleFluentConfig UseAudio(this SimpleContainer container)
        {
            container.Singleton<IPlaySessionService, PlaySessionService>();
            return new AudioModuleFluentConfig(container);
        }
    }

    public sealed class AudioModuleFluentConfig
    {
        private readonly SimpleContainer _container;

        internal AudioModuleFluentConfig(SimpleContainer container)
        {
            _container = container;
        }

        public AudioModuleFluentConfig AddBackgroundMedia(Type backgroundMediaHandlerType)
        {
            _container.Instance(new AudioModuleConfig
            {
                BackgroundMediaHandlerType = backgroundMediaHandlerType
            });
            return this;
        }
    }
}
