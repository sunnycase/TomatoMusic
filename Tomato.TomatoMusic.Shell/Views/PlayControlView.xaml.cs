using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Shell.ViewModels;
using Tomato.Uwp.Mvvm.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Tomato.TomatoMusic.Shell.Views
{
    public sealed partial class PlayControlView : UserControl
    {
        public static DependencyProperty PlaySessionProperty { get; } = DependencyProperty.Register(nameof(PlaySession),
            typeof(IPlaySessionService), typeof(PlayControlView), new PropertyMetadata(DependencyProperty.UnsetValue));

        public static DependencyProperty HamburgerMenuProperty { get; } = DependencyProperty.Register(nameof(HamburgerMenu),
            typeof(HamburgerMenu), typeof(PlayControlView), new PropertyMetadata(DependencyProperty.UnsetValue));

        public IPlaySessionService PlaySession
        {
            get { return (IPlaySessionService)GetValue(PlaySessionProperty); }
            set { SetValue(PlaySessionProperty, value); }
        }

        public HamburgerMenu HamburgerMenu
        {
            get { return (HamburgerMenu)GetValue(HamburgerMenuProperty); }
            set { SetValue(HamburgerMenuProperty, value); }
        }

        public PlayControlView()
        {
            this.InitializeComponent();
        }

        public void SwitchHamburger()
        {
            HamburgerMenu?.SwitchPane();
        }

        public void SwitchPlayingView()
        {
            var mainViewModel = MainViewModel.Current;
            mainViewModel?.SwitchPlayingView();
        }
    }
}
