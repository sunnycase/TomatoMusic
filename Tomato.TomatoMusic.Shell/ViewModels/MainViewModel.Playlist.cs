using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Resources;

namespace Tomato.TomatoMusic.Shell.ViewModels
{
    partial class MainViewModel
    {
        public BindableCollection<MenuItem> PlaylistMenuItems { get; }
        public IReadOnlyList<MenuItem> SolidMenuItems { get; }

        private async void LoadPlaylistMenuItems()
        {
        }

        private IReadOnlyList<MenuItem> LoadSolidMenuItems()
        {
            var resourceLoader = IoC.Get<ResourceLoader>();
            return new List<MenuItem>
            {
                new MenuItem(n => n.For<PlaylistViewModel>().WithParam(o => o.Key, PlaylistManager.MusicLibrary.Key).Navigate())
                {
                    Glyph = "\uE8F1",
                    Text = resourceLoader.GetString("MusicLibrary")
                },
                new MenuItem(n => n.For<Playing.PlayingViewModel>().Navigate())
                {
                    Glyph = "\uE904",
                    Text = resourceLoader.GetString("Playing/Text")
                }
            };
        }

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
            if (CanSubmitNewPlaylist)
            {
                await PlaylistManager.AddCustomPlaylist(NewPlaylistName);
                _addPlaylistButton?.Flyout.Hide();
            }
        }

        public void OnCancelNewPlaylist()
        {
            _addPlaylistButton?.Flyout.Hide();
        }

        public void NavigateToAbout()
        {
            _navigationService?.Navigate<Views.AboutView>();
        }
    }
}
