using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Services;
using Windows.Storage;

namespace Tomato.TomatoMusic.Shell.Services
{
    class ConfigurationService : IConfigurationService
    {
        public PlayerConfiguration Player { get; } = new PlayerConfiguration();
        public ThemeConfiguration Theme { get; } = new ThemeConfiguration();
        public MetadataConfiguration Metadata { get; } = new MetadataConfiguration();

        private readonly ApplicationDataContainer _configContainer;
        private const string _configContainerName = "Tomato.TomatoMusic.Config";

        private readonly List<WeakReference> _attachedConfigs = new List<WeakReference>();

        public ConfigurationService()
        {
            _configContainer = ApplicationData.Current.RoamingSettings.CreateContainer(_configContainerName,
                 ApplicationDataCreateDisposition.Always);
            Load();
        }

        private void Load()
        {
            TryPopulate(Player);
            TryPopulate(Theme);
            TryPopulate(Metadata);
        }

        private void Config_OnSaved(object sender, EventArgs e)
        {
            var config = sender as ConfigurationBase;
            if (config != null)
            {
                _configContainer.Values[config.RuntimeKey] = JsonConvert.SerializeObject(sender);
            }
        }

        public void TryPopulate(ConfigurationBase config, bool attachSaveEvent = true)
        {
            try
            {
                object obj;
                if (_configContainer.Values.TryGetValue(config.RuntimeKey, out obj))
                    JsonConvert.PopulateObject(obj?.ToString(), config);
            }
            catch (Exception) { }
            if (attachSaveEvent)
            {
                _attachedConfigs.RemoveAll(o => !o.IsAlive);
                if (!_attachedConfigs.Any(o => o.IsAlive && o.Target == config))
                {
                    config.OnSaved += Config_OnSaved;
                    _attachedConfigs.Add(new WeakReference(config));
                }
            }
        }
    }
}
