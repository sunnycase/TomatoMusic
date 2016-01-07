using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Shell.ViewModels.Playlist;
using Tomato.Uwp.Mvvm;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    class PlaylistViewModel : BindableBase
    {
        private IPlaylistAnchor _anchor;
        public IPlaylistAnchor Anchor
        {
            get { return _anchor; }
            set
            {
                if (SetProperty(ref _anchor, value))
                {
                    OnPropertyChanged(nameof(CanEditName));
                    OnPropertyChanged(nameof(CanEditContent));
                    OnPlaylistAnchorChanged(value);
                }
            }
        }

        private MusicsViewModel _musicsViewModel;
        public MusicsViewModel MusicsViewModel
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

        public bool CanEditName => _anchor?.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey &&
            _anchor?.Placeholder.Key != Primitives.Playlist.MusicLibraryPlaylistKey;
        public bool CanEditContent => _anchor?.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey;

        private IPlaylistContentProvider _playlistContentProvider;
        public IThemeService ThemeService { get; }

        public PlaylistViewModel(IThemeService themeService)
        {
            ThemeService = themeService;
        }

        private void Provider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IPlaylistContentProvider.IsRefreshing):
                    IsRefreshing = _playlistContentProvider?.IsRefreshing ?? false;
                    break;
                default:
                    break;
            }
        }

        public async void OnRequestAddFolder()
        {
            if(_playlistContentProvider != null)
            {
                var folder = await FolderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    _playlistContentProvider.AddFolder(folder);
                }
            }
        }

        public void OnRequestAddFiles()
        {

        }

        private void OnPlaylistAnchorChanged(IPlaylistAnchor anchor)
        {
            MusicsViewModel = new MusicsViewModel(anchor);
            _playlistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(anchor);
            IsRefreshing = _playlistContentProvider.IsRefreshing;
            _playlistContentProvider.PropertyChanged += Provider_PropertyChanged;
        }

        private static readonly Lazy<FolderPicker> _folderPicker = new Lazy<FolderPicker>(() =>
        {
            var picker = new FolderPicker()
            {
                ViewMode = PickerViewMode.List
            };
            picker.FileTypeFilter.Add("*");
            return picker;
        });

        private FolderPicker FolderPicker => _folderPicker.Value;
    }
}
