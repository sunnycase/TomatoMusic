using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.Media;

namespace Tomato.TomatoMusic.Audio.Services
{
    class MediaTransportService : BindableBase, IMediaTransportService
    {
        private readonly SystemMediaTransportControls _smtc;

        public bool IsEnabled
        {
            get { return _smtc.IsEnabled; }
            set { _smtc.IsEnabled = value; }
        }

        public bool IsPlayEnabled
        {
            get { return _smtc.IsPlayEnabled; }
            set { _smtc.IsPlayEnabled = value; }
        }

        public bool IsPauseEnabled
        {
            get { return _smtc.IsPauseEnabled; }
            set { _smtc.IsPauseEnabled = value; }
        }

        public MediaPlaybackStatus PlaybackStatus
        {
            get { return _smtc.PlaybackStatus; }
            set { _smtc.PlaybackStatus = value; }
        }

        public event EventHandler<SystemMediaTransportControlsButtonPressedEventArgs> ButtonPressed;

        public MediaTransportService()
        {
            _smtc = SystemMediaTransportControls.GetForCurrentView();
            PlaybackStatus = MediaPlaybackStatus.Closed;
            _smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            _smtc.DisplayUpdater.Update();
            _smtc.ButtonPressed += _smtc_ButtonPressed;
        }

        private void _smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            ButtonPressed?.Invoke(this, args);
        }

        public void SetCurrentTrack(TrackInfo value)
        {
            var updater = _smtc.DisplayUpdater;
            var musicProp = updater.MusicProperties;
            if (value != null)
            {
                musicProp.Title = value.Title;
                musicProp.Artist = value.Artist;
                musicProp.AlbumTitle = value.Album;
                musicProp.AlbumArtist = value.AlbumArtist;
            }
            else
            {
                updater.ClearAll();
            }
            updater.Update();
        }
    }
}
