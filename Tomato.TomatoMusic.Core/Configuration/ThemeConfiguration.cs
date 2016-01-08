using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Configuration
{
    public sealed class ThemeConfiguration : ConfigurationBase
    {
        public const string Key = "ThemeConfiguration";
        public override string RuntimeKey => Key;

        private bool _updateBackgroundEvenByteBasis = false;
        public bool UpdateBackgroundEvenByteBasis
        {
            get { return _updateBackgroundEvenByteBasis; }
            set { SetProperty(ref _updateBackgroundEvenByteBasis, value); }
        }
    }
}
