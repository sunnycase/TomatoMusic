using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Mvvm;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.TomatoMusic.Shell.Views.Playlist;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Shell.ViewModels.Playlist
{
    class ManageWatchedFoldersViewModel : BindableBase
    {
        private StorageFolder _selectedFolder;
        public StorageFolder SelectedFolder
        {
            get { return _selectedFolder; }
            set { SetProperty(ref _selectedFolder, value); }
        }

        private ObservableCollection<StorageFolder> _folders;
        public ObservableCollection<StorageFolder> Folders
        {
            get { return _folders; }
            private set { SetProperty(ref _folders, value); }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            private set
            {
                if (SetProperty(ref _isRefreshing, value))
                    OnPropertyChanged(nameof(CanOperate));
            }
        }

        public bool CanOperate => !IsRefreshing;

        private readonly IPlaylistContentProvider _contentProvider;
        private ListView _foldersList;

        public ManageWatchedFoldersViewModel(IPlaylistAnchor anchor)
        {
            _contentProvider = IoC.Get<IPlaylistManager>().GetPlaylistContentProvider(anchor);
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                IsRefreshing = true;
                Folders = new ObservableCollection<StorageFolder>(await _contentProvider.GetFolders());
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task<ContentDialogResult> ShowAsync()
        {
            var dialog = new ManageWatchedFoldersDialog(this);
            return await dialog.ShowAsync();
        }

        public void SetupFoldersList(object sender, RoutedEventArgs e)
        {
            _foldersList = (ListView)sender;
        }

        public async void AddFolder()
        {
            var folder = await DefaultPickers.FolderPicker.PickSingleFolderAsync();
            if (folder != null && Folders.All(o => !o.IsEqual(folder)))
                Folders.Add(folder);
        }

        public void RemoveFolders()
        {
            var folders = _foldersList?.SelectedItems.Cast<StorageFolder>();
            if (folders != null)
                folders.Sink(o => Folders.Remove(o));
        }
    }
}
