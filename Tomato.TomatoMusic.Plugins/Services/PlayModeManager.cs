using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            new PlayModes.ListMode(),
            new PlayModes.RepeatOneMode(),
            new PlayModes.RepeatAllMode(),
            new PlayModes.ShuffleMode()
        };

        private ReadOnlyCollection<IPlayModeProvider> _providersCache;
        public ReadOnlyCollection<IPlayModeProvider> Providers
        {
            get
            {
                if(_providersCache == null)
                    _providersCache = new ReadOnlyCollection<IPlayModeProvider>(_providers.ToList());
                return _providersCache;
            }
        }

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
