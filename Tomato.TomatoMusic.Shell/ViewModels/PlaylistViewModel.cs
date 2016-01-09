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
        public IPlaylistAnchor Anchor { get; }

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

        public bool CanEditName => Anchor.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey &&
            Anchor.Placeholder.Key != Primitives.Playlist.MusicLibraryPlaylistKey;
        public bool CanEditContent => Anchor.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey;

        private IPlaylistContentProvider _playlistContentProvider;
        public IThemeService ThemeService { get; }

        public PlaylistViewModel(IPlaylistAnchor anchor)
        {
            ThemeService = IoC.Get<IThemeService>();
            Anchor = anchor;
            MusicsViewModel = new MusicsViewModel(anchor);
            _playlistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(anchor);
            IsRefreshing = _playlistContentProvider.IsRefreshing;
            _playlistContentProvider.PropertyChanged += Provider_PropertyChanged;
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

        private static readonly Dictionary<IPlaylistAnchor, WeakReference<PlaylistViewModel>> _viewModels = new Dictionary<IPlaylistAnchor, WeakReference<PlaylistViewModel>>();
        public static PlaylistViewModel Activate(IPlaylistAnchor anchor)
        {
            PlaylistViewModel viewModel;
            WeakReference<PlaylistViewModel> weak;
            if(_viewModels.TryGetValue(anchor, out weak))
            {
                if (weak.TryGetTarget(out viewModel))
                    return viewModel;
                else
                {
                    viewModel = new PlaylistViewModel(anchor);
                    weak.SetTarget(viewModel);
                    return viewModel;
                }
            }
            else
            {
                viewModel = new PlaylistViewModel(anchor);
                _viewModels.Add(anchor, new WeakReference<PlaylistViewModel>(viewModel));
                return viewModel;
            }
        }
    }
}
