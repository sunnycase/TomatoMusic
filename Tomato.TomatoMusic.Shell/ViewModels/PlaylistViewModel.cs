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

        public bool CanEditName => _anchor?.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey &&
            _anchor?.Placeholder.Key != Primitives.Playlist.MusicLibraryPlaylistKey;
        public bool CanEditContent => _anchor?.Placeholder.Key != Primitives.Playlist.DefaultPlaylistKey;

        private Lazy<IPlaylistContentProvider> _playlistContentProvider;
        private IPlaylistContentProvider PlaylistContentProvider => _playlistContentProvider.Value;

        public PlaylistViewModel()
        {
            _playlistContentProvider = new Lazy<IPlaylistContentProvider>(() => IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(Anchor));
        }

        public async void OnRequestAddFolder()
        {
            var folder = await FolderPicker.PickSingleFolderAsync();
            if(folder != null)
            {
                PlaylistContentProvider.AddFolder(folder);
            }
        }

        public void OnRequestAddFiles()
        {

        }

        private void OnPlaylistAnchorChanged(IPlaylistAnchor anchor)
        {
            MusicsViewModel = new MusicsViewModel(anchor);
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
