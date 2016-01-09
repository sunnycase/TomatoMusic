using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Primitives
{
    public interface IPlaylistAnchor
    {
        PlaylistPlaceholder Placeholder { get; }
        bool? IsSelected { get; set; }
        void ResetViewToMe();
    }
}
