using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    partial class MainViewModel
    {
        private Button _addPlaylistButton;
        public void SetupAddPlaylistButton(object sender, object e)
        {
            _addPlaylistButton = (Button)sender;
        }

        private string _newPlaylistName;
        public string NewPlaylistName
        {
            get { return _newPlaylistName; }
            set
            {
                if (this.SetProperty(ref _newPlaylistName, value))
                    NotifyOfPropertyChange(nameof(CanSubmitNewPlaylist));
            }
        }

        public bool CanSubmitNewPlaylist
        {
            get { return !string.IsNullOrEmpty(NewPlaylistName); }
        }

        public async void OnSubmitNewPlaylist()
        {
            if(CanSubmitNewPlaylist)
            {
                await PlaylistManager.AddCustomPlaylist(NewPlaylistName);
                _addPlaylistButton?.Flyout.Hide();
            }
        }

        public void OnCancelNewPlaylist()
        {
            _addPlaylistButton?.Flyout.Hide();
        }

        public void OnSelectedPlaylistChanged()
        {
            _navigationService?.Navigate(typeof(Views.PlaylistView), PlaylistManager.SelectedPlaylist);
        }

        private void NavigateToSelectedPlaylist()
        {
            OnSelectedPlaylistChanged();
        }
    }
}
