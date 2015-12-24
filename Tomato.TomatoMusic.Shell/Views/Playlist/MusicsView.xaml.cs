using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tomato.TomatoMusic.Shell.ViewModels.Playlist;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Tomato.TomatoMusic.Shell.Views.Playlist
{
    public sealed partial class MusicsView : PivotItem
    {
        internal static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(MusicsViewModel),
            typeof(MusicsView), new PropertyMetadata(null));

        internal MusicsViewModel ViewModel
        {
            get { return (MusicsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public MusicsView()
        {
            RegisterPropertyChangedCallback(ViewModelProperty, OnViewModelPropertyChanged);
            this.InitializeComponent();
        }

        private void OnViewModelPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            ViewModel.RequestViewSelect += ViewModel_RequestViewSelect;
        }

        private void ViewModel_RequestViewSelect(MusicsTrackViewModel obj)
        {
            lv_Tracks.SelectedItem = obj;
        }

        public void TrackViewModelIsSelectedSetter(object model, bool value)
        {
            ((MusicsTrackViewModel)model).IsSelected = value;
        }
    }
}
