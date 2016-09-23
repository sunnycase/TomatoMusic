using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Shell.ViewModels;
using Tomato.Mvvm.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Caliburn.Micro;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Tomato.TomatoMusic.Shell.Views
{
    public sealed partial class PlayControlView : UserControl
    {
        public IPlaySessionService PlaySession { get; } = Execute.InDesignMode ? null : IoC.Get<IPlaySessionService>();

        public PlayControlView()
        {
            this.InitializeComponent();
        }
    }
}
