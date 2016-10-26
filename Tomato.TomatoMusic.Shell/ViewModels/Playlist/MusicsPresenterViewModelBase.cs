using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    abstract class MusicsPresenterViewModelBase<T> : Screen, IMusicsPresenterViewModel
    {
        protected PlaylistPlaceholder Playlist { get; }
        protected IPlaylistContentProvider PlaylistContentProvider { get; }
        protected IPlaySessionService PlaySession { get; }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { this.SetProperty(ref _isRefreshing, value); }
        }

        private object _itemsSource;
        public object ItemsSource
        {
            get { return _itemsSource; }
            protected set { this.SetProperty(ref _itemsSource, value); }
        }

        private bool _isItemsSourceGrouped;
        public bool IsItemsSourceGrouped
        {
            get { return _isItemsSourceGrouped; }
            protected set { this.SetProperty(ref _isItemsSourceGrouped, value); }
        }


        private MusicsOrderRule _selectedOrderRule;
        public MusicsOrderRule SelectedOrderRule
        {
            get { return _selectedOrderRule; }
            set
            {
                if (this.SetProperty(ref _selectedOrderRule, CoerceOrderRule(value)))
                {
                    SaveMusicsViewSetting();
                    OnSelectedOrderRuleChanged();
                }
            }
        }

        private void SaveMusicsViewSetting()
        {
            PlaylistViewConfig.OrderRule = SelectedOrderRule;
            PlaylistViewConfig.Save();
        }

        protected abstract void OnSelectedOrderRuleChanged();

        public abstract MusicsOrderRule[] OrderRules { get; }

        public PlaylistViewConfiguration PlaylistViewConfig { get; }

        public MusicsPresenterViewModelBase(PlaylistPlaceholder playlist, PlaylistViewConfiguration playlistViewConfig)
        {
            Playlist = playlist;
            PlaylistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(playlist);
            PlaylistContentProvider.PropertyChanged += playlistContentProvider_PropertyChanged;
            PlaySession = IoC.Get<IPlaySessionService>();
            PlaylistViewConfig = playlistViewConfig;
            LoadMusicsViewSetting();
        }

        private void LoadMusicsViewSetting()
        {
            SelectedOrderRule = PlaylistViewConfig.OrderRule;
        }

        private MusicsOrderRule CoerceOrderRule(MusicsOrderRule value)
        {
            if (OrderRules.Contains(value))
                return value;
            return OrderRules.First();
        }

        private void playlistContentProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaylistContentProvider.IsRefreshing):
                    IsRefreshing = PlaylistContentProvider.IsRefreshing;
                    break;
                default:
                    break;
            }
        }
    }
}
