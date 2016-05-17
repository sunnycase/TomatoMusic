using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Mvvm;

namespace Tomato.TomatoMusic.Configuration
{
    public struct EqualizerParam
    {
        public float Frequency { get; set; }
        public float BandWidth { get; set; }
        public float Gain { get; set; }
    }

    public class PlayerConfiguration : ConfigurationBase
    {
        public const string Key = "PlayerConfiguration";
        public override string RuntimeKey => Key;

        public Guid PlayMode { get; set; } = new Guid("AC3A41B1-58C9-48D3-A4FD-D01CC548B29B");
        public double Volume { get; set; } = 100;
        public ObservableCollection<EqualizerParam> EqualizerParameters { get; set; } = new ObservableCollection<EqualizerParam>();
    }
}
