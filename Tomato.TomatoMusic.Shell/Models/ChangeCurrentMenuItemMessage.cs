using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Shell.ViewModels;

namespace Tomato.TomatoMusic.Shell.Models
{
    public class ChangeCurrentMenuItemMessage
    {
        internal MenuItem MenuItem { get; set; }
    }
}
