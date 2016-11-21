using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Shell.Models;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class MenuItem
    {
        public string Glyph { get; set; }
        public string Text { get; set; }
        public Uri Uri { get; set; }
        public Type ViewModelType { get; set; }

        public MenuItem(Type viewModelType, Uri uri)
        {
            ViewModelType = viewModelType;
            Uri = uri;
        }

        public void OnClick()
        {
            IoC.Get<IEventAggregator>().BeginPublishOnUIThread(new NavigateMainMenuMessage
            {
                Uri = Uri,
                ViewModelType = ViewModelType
            });
        }
    }
}
