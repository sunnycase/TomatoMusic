using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    public interface IMusicsPresenterViewModel
    {
        MusicsOrderRule[] OrderRules { get; }
        MusicsOrderRule SelectedOrderRule { get; set; }
    }
}
