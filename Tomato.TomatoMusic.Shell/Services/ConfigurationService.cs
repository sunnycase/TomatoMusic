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
        public PlayerConfiguration Player { get; private set; } = new PlayerConfiguration();
        
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
            Player = TryLoad<PlayerConfiguration>(PlayerConfiguration.Key);
            Player.OnSaved += Player_OnSaved;
        }

        private void Player_OnSaved(object sender, EventArgs e)
        {
            var config = sender as ConfigurationBase;
            if (config != null)
            {
                _configContainer.Values[config.RuntimeKey] = JsonConvert.SerializeObject(sender);
            }
        }

        private T TryLoad<T>(string key) where T : class, new()
        {
            object obj;
            if (_configContainer.Values.TryGetValue(key, out obj))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(obj?.ToString()) ?? new T();
                }
                catch (Exception)
                {
                }
            }
            return new T();
        }
    }
}
