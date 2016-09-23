using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    enum OptionMenuItemKind
    {
        Settings
    }

    class OptionMenuItem
    {
        public string Glyph { get; set; }
        public string Text { get; set; }
        public OptionMenuItemKind Kind { get; set; }

        public void OnClick()
        {
            var nav = IoC.Get<INavigationService>();
            switch (Kind)
            {
                case OptionMenuItemKind.Settings:
                    nav?.For<SettingsViewModel>().Navigate();
                    break;
                default:
                    break;
            }
        }
    }

    class OptionMenuItemCollection : Collection<OptionMenuItem>
    {

    }
}
