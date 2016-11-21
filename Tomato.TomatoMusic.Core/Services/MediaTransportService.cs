using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Tomato.Mvvm;
using Windows.Media;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.Services
{
    public class MediaTransportService : BindableBase, IMediaTransportService, IDisposable
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
            private set { _smtc.PlaybackStatus = value; }
        }

        public bool CanNext
        {
            get { return _smtc.IsNextEnabled; }
            set { _smtc.IsNextEnabled = value; }
        }

        public bool CanPrevious
        {
            get { return _smtc.IsPreviousEnabled; }
            set { _smtc.IsPreviousEnabled = value; }
        }

        public event EventHandler<SystemMediaTransportControlsButtonPressedEventArgs> ButtonPressed;

        public MediaTransportService(SystemMediaTransportControls smtc)
        {
            _smtc = smtc;
            PlaybackStatus = MediaPlaybackStatus.Closed;
            _smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            _smtc.DisplayUpdater.Update();
            _smtc.ButtonPressed += _smtc_ButtonPressed;
        }

        public MediaTransportService()
            :this(SystemMediaTransportControls.GetForCurrentView())
        {

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
                PlaybackStatus = MediaPlaybackStatus.Closed;
            }
            updater.Update();
        }

        public void SetPlaybackState(MediaPlaybackState state)
        {
            switch (state)
            {
                case MediaPlaybackState.None:
                    PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaPlaybackState.Opening:
                    PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlaybackState.Buffering:
                    PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlaybackState.Playing:
                    PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                default:
                    break;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    PlaybackStatus = MediaPlaybackStatus.Closed;
                    _smtc.ButtonPressed -= _smtc_ButtonPressed;
                }
                disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
