using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    abstract class MusicsPresenterViewModelBase<T> : Screen
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
        
        public MusicsPresenterViewModelBase(PlaylistPlaceholder playlist)
        {
            Playlist = playlist;
            PlaylistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(playlist);
            PlaylistContentProvider.PropertyChanged += playlistContentProvider_PropertyChanged;
            PlaySession = IoC.Get<IPlaySessionService>();
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
