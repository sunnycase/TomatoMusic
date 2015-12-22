using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Shell.ViewModels.Playlist;
using Tomato.Uwp.Mvvm;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class PlaylistViewModel : BindableBase
    {
        private IPlaylistAnchor _anchor;
        public IPlaylistAnchor Anchor
        {
            get { return _anchor; }
            set
            {
                if (SetProperty(ref _anchor, value))
                    OnPlaylistAnchorChanged(value);
            }
        }

        private MusicsViewModel _musicsViewModel;
        public MusicsViewModel MusicsViewModel
        {
            get { return _musicsViewModel; }
            private set { SetProperty(ref _musicsViewModel, value); }
        }

        public PlaylistViewModel()
        {

        }

        private void OnPlaylistAnchorChanged(IPlaylistAnchor anchor)
        {
            MusicsViewModel = new MusicsViewModel(anchor);
        }
    }
}
