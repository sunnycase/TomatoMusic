using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
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

        public PlaylistViewModel()
        {

        }

        private void OnPlaylistAnchorChanged(IPlaylistAnchor anchor)
        {

        }
    }
}
