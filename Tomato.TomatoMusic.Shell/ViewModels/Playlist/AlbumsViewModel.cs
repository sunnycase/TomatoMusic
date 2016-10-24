using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    class AlbumsViewModel : Screen, IMusicsPresenterViewModel
    {

        public MusicsOrderRule[] OrderRules { get; } = new MusicsOrderRule[]
        {

        };

        public MusicsOrderRule SelectedOrderRule
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
