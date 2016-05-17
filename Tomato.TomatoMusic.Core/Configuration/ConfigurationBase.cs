using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Mvvm;

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
}
