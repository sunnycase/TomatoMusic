using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Plugins;
using Tomato.TomatoMusic.Plugins.Services;

namespace Tomato.TomatoMusic
{
    public static class PluginsModule
    {
        public static void UsePlugins(this SimpleContainer container)
        {
            container.Singleton<IPlayModeManager, PlayModeManager>();
        }
    }
}
