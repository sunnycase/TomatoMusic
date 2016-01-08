using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Uwp.Mvvm;

namespace Tomato.TomatoMusic.Configuration
{
    public abstract class ConfigurationBase : BindableBase
    {
        public event EventHandler OnSaved;
        public abstract string RuntimeKey { get; }

        public void Save()
        {
            OnSaved?.Invoke(this, EventArgs.Empty);
        }
    }

    public class PlayerConfiguration : ConfigurationBase
    {
        public const string Key = "PlayerConfiguration";
        public override string RuntimeKey => Key;

        public Guid PlayMode { get; set; } = new Guid("AC3A41B1-58C9-48D3-A4FD-D01CC548B29B");
        public double Volume { get; set; } = 100;
    }
}
