using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class MenuItem
    {
        public string Glyph { get; set; }
        public string Text { get; set; }

        private readonly Action<INavigationService> _action;

        public MenuItem(Action<INavigationService> action)
        {
            _action = action;
        }

        public void OnClick()
        {
            var nav = IoC.Get<INavigationService>();
            if (nav != null)
                _action?.Invoke(nav);
        }
    }
}
