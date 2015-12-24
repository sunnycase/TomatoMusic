using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic.Plugins.Services
{
    class PlayModeManager : IPlayModeManager
    {
        private HashSet<IPlayModeProvider> _providers = new HashSet<IPlayModeProvider>
        {
            new PlayModes.ListMode()
        };

        public IPlayModeProvider[] Providers => _providers.ToArray();

        public PlayModeManager()
        {

        }

        public void RegisterProvider(IPlayModeProvider provider)
        {
            _providers.Add(provider);
        }

        public void UnregisterProvider(IPlayModeProvider provider)
        {
            _providers.Remove(provider);
        }

        public IPlayModeProvider GetProvider(Guid id)
        {
            return _providers.Single(o => o.Id == id);
        }
    }
}
