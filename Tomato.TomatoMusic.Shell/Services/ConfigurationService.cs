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

        public ConfigurationService()
        {
            _configContainer = ApplicationData.Current.RoamingSettings.CreateContainer(_configContainerName,
                 ApplicationDataCreateDisposition.Always);
            Load();
        }

        private void Load()
        {
            TryLoad(Player);
            TryLoad(Theme);
            TryLoad(Metadata);

            Player.OnSaved += Config_OnSaved;
            Theme.OnSaved += Config_OnSaved;
            Metadata.OnSaved += Config_OnSaved;
        }

        private void Config_OnSaved(object sender, EventArgs e)
        {
            var config = sender as ConfigurationBase;
            if (config != null)
            {
                _configContainer.Values[config.RuntimeKey] = JsonConvert.SerializeObject(sender);
            }
        }

        private void TryLoad<T>(T config) where T : ConfigurationBase
        {
            try
            {
                object obj;
                if (_configContainer.Values.TryGetValue(config.RuntimeKey, out obj))
                    JsonConvert.PopulateObject(obj?.ToString(), config);
            }
            catch (Exception) { }
        }
    }
}
