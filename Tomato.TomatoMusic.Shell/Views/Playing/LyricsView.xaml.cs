using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomato.TomatoMusic.Shell.ViewModels.Playing;
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

namespace Tomato.TomatoMusic.Shell.Views.Playing
{
    public sealed partial class LyricsView : PivotItem
    {
        internal LyricsViewModel ViewModel { get; } = LyricsViewModel.Activate();

        public LyricsView()
        {
            this.InitializeComponent();
            ViewModel.SetLyricsListBox(lb_Lyrics);
        }
    }
}
