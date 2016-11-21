using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomato.TomatoMusic.Shell.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Reflection.Emit;
using Tomato.TomatoMusic.Shell.Models;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Tomato.TomatoMusic.Shell.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainView : Page, IHandle<MainMenuNavigatedMessage>
    {
        internal MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainView()
        {
            this.InitializeComponent();
            IoC.Get<IEventAggregator>().Subscribe(this);
        }

        private void hm_Menu_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((MenuItem)e.ClickedItem).OnClick();
        }

        private void hm_Menu_OptionsItemClick(object sender, ItemClickEventArgs e)
        {
            ((OptionMenuItem)e.ClickedItem).OnClick();
        }

        void IHandle<MainMenuNavigatedMessage>.Handle(MainMenuNavigatedMessage message)
        {
            hm_Menu.SelectMenuItem(o =>
            {
                var item = (MenuItem)o;
                return item.Uri.AbsoluteUri == message.AbsoluteUri;
            });
        }
    }
}
