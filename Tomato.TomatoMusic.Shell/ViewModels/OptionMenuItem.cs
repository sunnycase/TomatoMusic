using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class OptionMenuItem
    {
        public string Glyph { get; set; }
        public string Text { get; set; }
    }

    class OptionMenuItemCollection : Collection<OptionMenuItem>
    {

    }
}
