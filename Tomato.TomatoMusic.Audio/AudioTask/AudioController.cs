using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.Rpc.Json;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Plugins;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.Storage;
using Windows.Media;
using Tomato.Mvvm;
using Windows.Media.Playback;
using Tomato.Media.Toolkit;

namespace Tomato.TomatoMusic.AudioTask
{
    public class AudioController : BindableBase, IAudioController
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly IPlayModeManager _playModeManager;
        private readonly MediaEnvironment _mediaEnvironment = new MediaEnvironment();
        private IPlayModeProvider _currentPlayMode;

        private IList<TrackInfo> _playlist;
        private TrackInfo _currentTrack;

        private bool _autoPlay;
        private readonly ILog _logger = LogManager.GetLog(typeof(AudioController));
        private readonly MediaTransportService _mtService;

        private readonly AudioControllerDispatcher _controllerDisp;
        private readonly IAudioControllerHandler _controllerHandler;

        public AudioController()
        {
            _controllerDisp = new AudioControllerDispatcher(this);
            _controllerHandler = _controllerDisp.ControllerHandler;
            _mediaEnvironment.RegisterDefaultCodecs();
            _playModeManager = IoC.Get<IPlayModeManager>();

            _mediaPlayer = BackgroundMediaPlayer.Current;
            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            _mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            _mediaPlayer.SeekCompleted += MediaPlayer_SeekCompleted;
            _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            _mtService = new MediaTransportService(_mediaPlayer.SystemMediaTransportControls);
            _mtService.IsEnabled = _mtService.IsPauseEnabled = _mtService.IsPlayEnabled = true;
            _mtService.ButtonPressed += _mtService_ButtonPressed;
            InitializeEffects();
        }

        private void InitializeEffects()
        {
            //_equalizerEffect = new EqualizerEffectTransform();
        }

        private void MediaPlayer_MediaFailed(MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            _logger.Warn(args.Error.ToString());
            _logger.Error(args.ExtendedErrorCode);
            var currentTrack = _currentTrack;
            var nextTrack = GetNextTrack();
            if (nextTrack != currentTrack && nextTrack != null)
            {
                _autoPlay = true;
                SetCurrentTrack(nextTrack);
            }
        }

        private void MediaPlayer_SeekCompleted(MediaPlayer sender, object args)
        {
            _controllerHandler.NotifySeekCompleted();
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            var currentTrack = _currentTrack;
            var nextTrack = GetNextTrack();
            if (nextTrack != null)
            {
                if (nextTrack != currentTrack)
                {
                    _autoPlay = true;
                    SetCurrentTrack(nextTrack);
                }
                else
                    Play();
            }
        }

        TrackInfo GetNextTrack()
        {
            var playlist = _playlist;
            var currentTrack = _currentTrack;
            var playMode = _currentPlayMode;
            if (playMode != null && playlist != null && currentTrack != null)
            {
                return playMode.SelectNextTrack(playlist, currentTrack);
            }
            return currentTrack;
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            _controllerHandler.NotifyControllerStateChanged(_mediaPlayer.CurrentState);

            switch (_mediaPlayer.CurrentState)
            {
                case MediaPlayerState.Closed:
                    CanPlay = CanPause = false;
                    ShowPause = false;
                    ShowPlay = true;
                    PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                case MediaPlayerState.Opening:
                    CanPlay = CanPause = false;
                    ShowPause = false;
                    ShowPlay = true;
                    PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlayerState.Buffering:
                    CanPlay = CanPause = false;
                    ShowPause = false;
                    ShowPlay = true;
                    PlaybackStatus = MediaPlaybackStatus.Changing;
                    break;
                case MediaPlayerState.Playing:
                    CanPause = true;
                    CanPlay = false;
                    ShowPause = true;
                    ShowPlay = false;
                    PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlayerState.Paused:
                    CanPause = false;
                    CanPlay = true;
                    ShowPause = false;
                    ShowPlay = true;
                    PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaPlayerState.Stopped:
                    PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                default:
                    break;
            }
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            if (_autoPlay)
            {
                _autoPlay = false;
                Play();
            }
            _controllerHandler.NotifyMediaOpened();
            _controllerHandler.NotifyDuration(_mediaPlayer.NaturalDuration);
        }

        public void Play()
        {
            _mediaPlayer.Play();
        }

        private async void SetMediaSource(Uri uri)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                StorageFile file;
                if (uri.Scheme == "ms-appx")
                    file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                else if (uri.IsFile)
                    file = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
                else
                    throw new NotSupportedException("Not supported uri.");
                
                _mediaPlayer.SetFileSource(file);
            }
            catch
            {
                PlaybackStatus = MediaPlaybackStatus.Closed;
            }
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        public void SetPlaylist(IList<TrackInfo> tracks)
        {
            _playlist = tracks;
        }

        public void SetCurrentTrack(TrackInfo track)
        {
            if (_currentTrack != track)
            {
                _currentTrack = track;
                if (_currentTrack != null)
                {
                    SetMediaSource(track.Source);
                    _controllerHandler.NotifyCurrentTrackChanged(track);
                    OnCurrentTrackChanged();
                }
                _mtService.SetCurrentTrack(track);
            }
        }

        public void MoveNext()
        {
            var cntIdx = GetCurrentTrackIndex();
            if (cntIdx != -1)
            {
                var nextIdx = cntIdx + 1;
                if (nextIdx >= 0 && nextIdx < _playlist.Count)
                {
                    _autoPlay = true;
                    SetCurrentTrack(_playlist[nextIdx]);
                }
            }
        }

        public void MovePrevious()
        {
            var cntIdx = GetCurrentTrackIndex();
            if (cntIdx != -1)
            {
                var prevIdx = cntIdx - 1;
                if (prevIdx >= 0 && prevIdx < _playlist.Count)
                {
                    _autoPlay = true;
                    SetCurrentTrack(_playlist[prevIdx]);
                }
            }
        }

        private int GetCurrentTrackIndex()
        {
            if (_playlist != null && _currentTrack != null)
            {
                return _playlist.IndexOf(_currentTrack);
            }
            return -1;
        }

        public void SetPlayMode(Guid id)
        {
            _currentPlayMode = _playModeManager.GetProvider(id);
        }

        public void AskPosition()
        {
            _controllerHandler.NotifyPosition(_mediaPlayer.Position);
        }

        public void SetPosition(TimeSpan position)
        {
            _mediaPlayer.Position = position;
            _logger.Info($"Set Position: {position}");
        }

        public void SetVolume(double value)
        {
            _mediaPlayer.Volume = value;
        }

        #region SMTC

        private bool _canPrevious;
        public bool CanPrevious
        {
            get { return _canPrevious; }
            private set
            {
                if (SetProperty(ref _canPrevious, value))
                    _mtService.CanPrevious = value;
            }
        }

        private bool _canPause;
        public bool CanPause
        {
            get { return _canPause; }
            private set { SetProperty(ref _canPause, value); }
        }

        private bool _canPlay;
        public bool CanPlay
        {
            get { return _canPlay; }
            private set { SetProperty(ref _canPlay, value); }
        }

        private bool _showPause;
        public bool ShowPause
        {
            get { return _showPause; }
            private set { SetProperty(ref _showPause, value); }
        }

        private bool _showPlay = true;
        public bool ShowPlay
        {
            get { return _showPlay; }
            private set { SetProperty(ref _showPlay, value); }
        }
        private bool _canNext;
        public bool CanNext
        {
            get { return _canNext; }
            private set
            {
                if (SetProperty(ref _canNext, value))
                    _mtService.CanNext = value;
            }
        }

        private MediaPlaybackStatus _playbackStatus = MediaPlaybackStatus.Closed;
        public MediaPlaybackStatus PlaybackStatus
        {
            get { return _playbackStatus; }
            private set
            {
                if (SetProperty(ref _playbackStatus, value))
                {
                    _mtService.PlaybackStatus = value;
                    OnCurrentTrackChanged();
                }
            }
        }

        private void OnCurrentTrackChanged()
        {
            if (_currentTrack != null && _playlist != null)
            {
                var idx = _playlist.IndexOf(_currentTrack);
                if (idx != -1)
                {
                    CanPrevious = idx > 0 && PlaybackStatus != MediaPlaybackStatus.Changing;
                    CanNext = idx < _playlist.Count - 1 && PlaybackStatus != MediaPlaybackStatus.Changing;
                }
                else
                    CanPrevious = CanNext = false;
            }
            else
                CanPlay = CanPause = CanPrevious = CanNext = false;
        }

        public void RequestPlay()
        {
            if (CanPlay)
                Play();
        }

        public void RequestPause()
        {
            if (CanPause)
                Pause();
        }

        public void RequestNext()
        {
            if (CanNext)
                MoveNext();
        }

        public void RequestPrevious()
        {
            if (CanPrevious)
                MovePrevious();
        }

        private void _mtService_ButtonPressed(object sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case Windows.Media.SystemMediaTransportControlsButton.Play:
                    RequestPlay();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Pause:
                    RequestPause();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Stop:
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Record:
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.FastForward:
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Rewind:
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Next:
                    RequestNext();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Previous:
                    RequestPrevious();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.ChannelUp:
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.ChannelDown:
                    break;
                default:
                    break;
            }
        }

        #endregion

        public void OnCanceled()
        {
            _controllerDisp.OnCanceled();
            _mtService.Dispose();
        }

        public void AskPlaylist()
        {
            if (_playlist != null)
                _controllerHandler.NotifyPlaylist(_playlist);
        }

        public void AskCurrentTrack()
        {
            _controllerHandler.NotifyCurrentTrackChanged(_currentTrack);
        }

        public void AskCurrentState()
        {
            _controllerHandler.NotifyControllerStateChanged(_mediaPlayer?.CurrentState ?? MediaPlayerState.Closed);
        }

        public void AskDuration()
        {
            _controllerHandler.NotifyDuration(_currentTrack?.Duration);
        }

        public void SetEqualizerParameter(float frequency, float bandWidth, float gain)
        {
            //_equalizerEffect.AddOrUpdateFilter(frequency, bandWidth, gain);
        }

        public void ClearEqualizerParameter(float frequency)
        {
            //_equalizerEffect.RemoveFilter(frequency);
        }
        
        public void SetupHandler()
        {
            _controllerHandler.NotifyControllerReady();
        }
    }
}
