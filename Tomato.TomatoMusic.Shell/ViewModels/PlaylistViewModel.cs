using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Shell.ViewModels.Playlist;
using Tomato.Mvvm;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    public class PlaylistViewModel : BindableBase
    {
        private IPlaylistAnchor _anchor;
        public IPlaylistAnchor Anchor
        {
            get { return _anchor; }
            private set
            {
                if (SetProperty(ref _anchor, value))
                    OnAnchorChanged();
            }
        }

        private Guid _key;
        public Guid Key
        {
            get { return _key; }
            set
            {
                if (SetProperty(ref _key, value))
                    OnKeyChanged();
            }
        }

        public bool IsValid => Anchor != null;

        private MusicsViewModel _musicsViewModel;
        internal MusicsViewModel MusicsViewModel
        {
            get { return _musicsViewModel; }
            private set { SetProperty(ref _musicsViewModel, value); }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { SetProperty(ref _isRefreshing, value); }
        }

        public bool CanEditName => IsValid ? Anchor.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey &&
            Anchor.Placeholder.Key != Primitives.Playlist.MusicLibraryPlaylistKey : false;
        public bool CanEditContent => IsValid ? Anchor.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey : false;

        private IPlaylistContentProvider _playlistContentProvider;

        public PlaylistViewModel()
        {
        }

        private void Provider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (IsValid)
            {
                switch (e.PropertyName)
                {
                    case nameof(IPlaylistContentProvider.IsRefreshing):
                        IsRefreshing = _playlistContentProvider.IsRefreshing;
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnAnchorChanged()
        {
            if (_playlistContentProvider != null)
                _playlistContentProvider.PropertyChanged -= Provider_PropertyChanged;

            if (Anchor != null)
            {
                MusicsViewModel = new MusicsViewModel(Anchor);
                _playlistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(Anchor);
                IsRefreshing = _playlistContentProvider.IsRefreshing;
                _playlistContentProvider.PropertyChanged += Provider_PropertyChanged;
            }
            else
            {
                MusicsViewModel = null;
                IsRefreshing = false;
            }
        }

        private void OnKeyChanged()
        {
            Anchor = IoC.Get<IPlaylistManager>().GetAnchorByKey(Key);
        }

        public async void OnRequestManageFolders()
        {
            if(IsValid)
            {
                var viewModel = new ManageWatchedFoldersViewModel(Anchor);
                if (await viewModel.ShowAsync() == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                    _playlistContentProvider.UpdateFolders(viewModel.Folders);
            }
        }
    }
}
