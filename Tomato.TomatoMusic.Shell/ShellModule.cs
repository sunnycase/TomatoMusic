using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Shell.ViewModels;

namespace Tomato.TomatoMusic.Shell
{
    static class ShellModule
    {
        public static void UseShell(this SimpleContainer container)
        {
            container.PerRequest<IShell, MainViewModel>();
        }
    }
}
