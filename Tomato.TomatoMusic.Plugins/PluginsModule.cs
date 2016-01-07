using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Plugins;
using Tomato.TomatoMusic.Plugins.Config;
using Tomato.TomatoMusic.Plugins.Services;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic
{
    public static class PluginsModule
    {
        public static PluginsModuleConfig UsePlugins(this SimpleContainer container)
        {
            container.Singleton<IPlayModeManager, PlayModeManager>();
            container.Singleton<IMediaMetadataService, MediaMetadataService>();
            container.Singleton<IThemeService, ThemeService>();

            return new PluginsModuleConfig(container);
        }
    }

    public sealed class PluginsModuleConfig
    {
        private readonly SimpleContainer _container;

        internal PluginsModuleConfig(SimpleContainer container)
        {
            _container = container;
        }

        public PluginsModuleConfig AddLastFm(string apiKey)
        {
            _container.Instance(new LastFmConfig
            {
                ApiKey = apiKey
            });
            return this;
        }
    }
}
