using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    partial class MainViewModel
    {
        private bool? _isPlayingViewSelected;
        public bool? IsPlayingViewSelected
        {
            get { return _isPlayingViewSelected; }
            set
            {
                if (this.SetProperty(ref _isPlayingViewSelected, value) && value.GetValueOrDefault())
                    NavigateToPlayingView();
            }
        }

        public void NavigateToPlayingView()
        {
            _navigationService?.Navigate(typeof(Views.Playing.PlayingView));
        }
    }
}
