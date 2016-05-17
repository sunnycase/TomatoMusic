using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Services;
using Tomato.Mvvm;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playing
{
    class EqualizerParamViewModel : BindableBase
    {
        public float Frequency { get; }
        public float BandWidth { get; }

        private double _gain;
        public double Gain
        {
            get { return _gain; }
            set { SetProperty(ref _gain, value); }
        }

        public EqualizerParamViewModel(float frequency, float bandWidth, float gain)
        {
            Frequency = frequency;
            BandWidth = bandWidth;
            _gain = gain;
        }
    }

    class EffectsViewModel
    {
        private readonly PlayerConfiguration _playerConfiguration;
        private readonly ObservableCollection<EqualizerParam> _equalizerParameters;

        public EqualizerParamViewModel Band1 { get; }
        public EqualizerParamViewModel Band2 { get; }
        public EqualizerParamViewModel Band3 { get; }
        public EqualizerParamViewModel Band4 { get; }
        public EqualizerParamViewModel Band5 { get; }
        public EqualizerParamViewModel Band6 { get; }
        public EqualizerParamViewModel Band7 { get; }
        public EqualizerParamViewModel Band8 { get; }
        public EqualizerParamViewModel Band9 { get; }
        public EqualizerParamViewModel Band10 { get; }

        public EffectsViewModel(IConfigurationService configureService)
        {
            _playerConfiguration = configureService.Player;
            _equalizerParameters = _playerConfiguration.EqualizerParameters;
            Band1 = LoadEqualizerParam(31);
            Band2 = LoadEqualizerParam(62);
            Band3 = LoadEqualizerParam(125);
            Band4 = LoadEqualizerParam(250);
            Band5 = LoadEqualizerParam(500);
            Band6 = LoadEqualizerParam(1000);
            Band7 = LoadEqualizerParam(2000);
            Band8 = LoadEqualizerParam(4000);
            Band9 = LoadEqualizerParam(8000);
            Band10 = LoadEqualizerParam(16000);
        }

        private EqualizerParamViewModel LoadEqualizerParam(float frequency)
        {
            EqualizerParamViewModel param;
            var state = _equalizerParameters.FirstOrDefault(o => o.Frequency == frequency);
            if (state.Frequency != 0)
                param = new EqualizerParamViewModel(frequency, state.BandWidth, state.Gain);
            else
                param = new EqualizerParamViewModel(frequency, 18, 0);
            param.PropertyChanged += Param_PropertyChanged;
            return param;
        }

        private void Param_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var param = (EqualizerParamViewModel)sender;
            if(e.PropertyName == nameof(EqualizerParamViewModel.Gain))
            {
                var state = _equalizerParameters.FirstOrDefault(o => o.Frequency == param.Frequency);
                if (state.Frequency != 0)
                {
                    var idx = _equalizerParameters.IndexOf(state);
                    _equalizerParameters[idx] = new EqualizerParam { Frequency = param.Frequency, BandWidth = param.BandWidth, Gain = (float)param.Gain };
                }
                else
                    _equalizerParameters.Add(new EqualizerParam { Frequency = param.Frequency, BandWidth = param.BandWidth, Gain = (float)param.Gain });
                _playerConfiguration.Save();
            }
        }
    }
}
