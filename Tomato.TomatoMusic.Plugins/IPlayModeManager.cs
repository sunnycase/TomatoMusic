using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic.Plugins
{
    public interface IPlayModeManager
    {
        IPlayModeProvider[] Providers { get; }

        IPlayModeProvider GetProvider(Guid id);
        void RegisterProvider(IPlayModeProvider provider);
        void UnregisterProvider(IPlayModeProvider provider);
    }
}
