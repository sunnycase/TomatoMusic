using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Shell.Controls
{
    [TemplatePart(Name = "PlaylistsButtonsListView", Type = typeof(ListViewBase))]
    class HamburgerMenu : Microsoft.Toolkit.Uwp.UI.Controls.HamburgerMenu
    {
        public static DependencyProperty PlaylistsItemsSourceProperty { get; } = DependencyProperty.Register(nameof(PlaylistsItemsSource), typeof(object),
            typeof(HamburgerMenu), new PropertyMetadata(null));

        public static DependencyProperty PlaylistsItemTemplateProperty { get; } = DependencyProperty.Register(nameof(PlaylistsItemTemplate), typeof(DataTemplate),
            typeof(HamburgerMenu), new PropertyMetadata(null));
        
        public object PlaylistsItemsSource
        {
            get { return GetValue(PlaylistsItemsSourceProperty); }
            set { SetValue(PlaylistsItemsSourceProperty, value); }
        }

        public DataTemplate PlaylistsItemTemplate
        {
            get { return (DataTemplate)GetValue(PlaylistsItemTemplateProperty); }
            set { SetValue(PlaylistsItemTemplateProperty, value); }
        }

        public event ItemClickEventHandler PlaylistsItemClick;

        public HamburgerMenu()
        {

        }

        public void SelectMenuItem(Func<object, bool> predicator)
        {
            if (TrySelectItem(_buttonsListView, predicator))
            {
                _optionsListView.SelectedIndex = -1;
                _playlistsButtonsListView.SelectedIndex = -1;
            }
            if (TrySelectItem(_playlistsButtonsListView, predicator))
            {
                _optionsListView.SelectedIndex = -1;
                _buttonsListView.SelectedIndex = -1;
            }
        }

        private bool TrySelectItem(ListViewBase listView, Func<object, bool> predicator)
        {
            var item = listView.Items.FirstOrDefault(predicator);
            if (item != null)
            {
                listView.SelectedItem = item;
                return true;
            }
            return false;
        }

        private ListViewBase _buttonsListView;
        private ListViewBase _optionsListView;
        private ListViewBase _playlistsButtonsListView;

        protected override void OnApplyTemplate()
        {
            if (_playlistsButtonsListView != null)
                _playlistsButtonsListView.ItemClick -= playlistsButtonsListView_ItemClick;
            if (_buttonsListView != null)
                _buttonsListView.ItemClick -= buttonsList_ItemClick;
            if (_optionsListView != null)
                _optionsListView.ItemClick -= optionsListView_ItemClick;

            _playlistsButtonsListView = (ListViewBase)GetTemplateChild("PlaylistsButtonsListView");
            _buttonsListView = (ListViewBase)GetTemplateChild("ButtonsListView");
            _optionsListView = (ListViewBase)GetTemplateChild("OptionsListView");

            _playlistsButtonsListView.ItemClick += playlistsButtonsListView_ItemClick;
            _buttonsListView.ItemClick += buttonsList_ItemClick;
            _optionsListView.ItemClick += optionsListView_ItemClick;

            base.OnApplyTemplate();
        }

        private void buttonsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_playlistsButtonsListView != null)
                _playlistsButtonsListView.SelectedIndex = -1;
        }

        private void optionsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_playlistsButtonsListView != null)
                _playlistsButtonsListView.SelectedIndex = -1;
        }

        private void playlistsButtonsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_buttonsListView != null)
                _buttonsListView.SelectedIndex = -1;
            if (_optionsListView != null)
                _optionsListView.SelectedIndex = -1;
            PlaylistsItemClick?.Invoke(this, e);
        }
    }
}
