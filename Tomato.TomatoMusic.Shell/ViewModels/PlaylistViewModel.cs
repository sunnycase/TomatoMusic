﻿using System;
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
        private PlaylistPlaceholder _playlist;
        public PlaylistPlaceholder Playlist
        {
            get { return _playlist; }
            private set
            {
                if (SetProperty(ref _playlist, value))
                    OnPlaylistChanged();
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

        public bool IsValid => Playlist != null;

        private object _musicsPresenterViewModel;
        public object MusicsPresenterViewModel
        {
            get { return _musicsPresenterViewModel; }
            private set { SetProperty(ref _musicsPresenterViewModel, value); }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set { SetProperty(ref _isRefreshing, value); }
        }

        public bool CanEditName => IsValid ? Playlist.Key != Primitives.Playlist.DefaultPlaylistKey &&
            Playlist.Key != Primitives.Playlist.MusicLibraryPlaylistKey : false;
        public bool CanEditContent => IsValid ? Playlist.Key != Primitives.Playlist.DefaultPlaylistKey : false;

        private IPlaylistContentProvider _playlistContentProvider;

        public MusicsViewType[] MusicsViewTypes { get; } = new MusicsViewType[]
        {
            MusicsViewType.Musics,
            MusicsViewType.Albums
        };

        private MusicsViewType _selectedMusicsViewType;
        public MusicsViewType SelectedMusicsViewType
        {
            get { return _selectedMusicsViewType; }
            set { SetProperty(ref _selectedMusicsViewType, value); }
        }

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

        private void OnPlaylistChanged()
        {
            if (_playlistContentProvider != null)
                _playlistContentProvider.PropertyChanged -= Provider_PropertyChanged;

            if (Playlist != null)
            {
                _playlistContentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(Playlist);
                IsRefreshing = _playlistContentProvider.IsRefreshing;
                _playlistContentProvider.PropertyChanged += Provider_PropertyChanged;
            }
            else
            {
                IsRefreshing = false;
            }
            OnMusicsViewChanged();
        }

        private void OnMusicsViewChanged()
        {
            if (Playlist != null)
            {
                MusicsPresenterViewModel = new MusicsViewModel(Playlist);
            }
            else
            {
                MusicsPresenterViewModel = null;
            }
        }

        private void OnKeyChanged()
        {
            Playlist = IoC.Get<IPlaylistManager>().GetPlaylistByKey(Key);
        }

        public async void OnRequestManageFolders()
        {
            if(IsValid)
            {
                var viewModel = new ManageWatchedFoldersViewModel(Playlist);
                if (await viewModel.ShowAsync() == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                    _playlistContentProvider.UpdateFolders(viewModel.Folders);
            }
        }
    }
}
