using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomato.TomatoMusic.Shell.ViewModels.Playlist;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Tomato.Mvvm;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Tomato.TomatoMusic.Shell.Views.Playlist
{
    public sealed partial class MusicsView : UserControl
    {
        internal MusicsViewModel ViewModel => (MusicsViewModel)DataContext;

        public MusicsView()
        {
            this.InitializeComponent();
        }

        public void TrackViewModelIsSelectedSetter(object model, bool value)
        {
            //((MusicsTrackViewModel)model).IsSelected = value;
        }

        private void lv_Tracks_PrepareContainerForItem(object sender, Controls.PrepareContainerForItemEventArgs e)
        {
            ((ListViewItem)e.Element).DoubleTapped += trackItem_DoubleTapped;
        }

        private void trackItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ((MusicsTrackViewModel)((ListViewItem)sender).Content)?.Play();
        }
    }
}
