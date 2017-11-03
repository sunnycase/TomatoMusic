using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using TomatoMusic.Messages;
using Weingartner.ReactiveCompositeCollections;
using Windows.UI.Xaml.Controls;

namespace TomatoMusic.Widgets
{
    internal class PlaylistNavigationMenuWidget : NavigationMenuWidget
    {
        public PlaylistNavigationMenuWidget()
        {
            MenuItems = new CompositeSourceList<NavigationViewItemBase>(new NavigationViewItemBase[]
                {
                    new NavigationViewItem { Icon = new SymbolIcon(Symbol.MusicInfo), Content = new NavigationMenuItemContent { Text = "正在播放", NavigationRequest = _navigationPlayingRequest } }
                });
        }

        private static readonly RootNavigationRequest _navigationPlayingRequest = new RootNavigationRequest
        {
            ViewModelType = typeof(ViewModels.PlayingViewModel)
        };
    }
}
