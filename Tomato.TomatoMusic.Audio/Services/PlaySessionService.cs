using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;
using Tomato.Rpc.Json;
using Tomato.TomatoMusic.AudioTask;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Services;
using Tomato.Uwp.Mvvm;
using Windows.Media;
using Windows.Media.Playback;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Plugins;
using System.Threading;

namespace Tomato.TomatoMusic.Audio.Services
{
    class PlaySessionService : BindableBase, IAudioControllerHandler, IPlaySessionService
    {
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
                    Execute.BeginOnUIThread(OnCurrentTrackChanged);
                }
            }
        }

        private IList<TrackInfo> _playlist;
        public IList<TrackInfo> Playlist
        {
            get { return _playlist; }
            private set
            {
                if (SetProperty(ref _playlist, value))
                {
                    _audioController.SetPlaylist(value);
                }
            }
        }

        private TrackInfo _currentTrack;
        public TrackInfo CurrentTrack
        {
            get { return _currentTrack; }
            private set
            {
                if (SetProperty(ref _currentTrack, value))
                {
                    _audioController.SetCurrentTrack(value);
                    _mtService.SetCurrentTrack(value);
                    OnCurrentTrackChanged();
                }
            }
        }

        private TimeSpan? _duration;
        public TimeSpan? Duration
        {
            get { return _duration; }
            private set { SetProperty(ref _duration, value); }
        }

        private TimeSpan _position;
        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                var oldValue = _position;
                if (SetProperty(ref _position, value))
                    OnPositionChanged(oldValue, value);
            }
        }

        private bool _autoPlay = false;

        private readonly BackgroundMediaPlayerClient _client;
        private readonly IMediaTransportService _mtService;
        private readonly IPlayModeManager _playModeManager;
        private IPlayModeProvider _currentPlayMode;
        private readonly Timer _askPositionTimer;
        private static readonly TimeSpan _askPositionPeriod = TimeSpan.FromSeconds(0.25);

        #region Rpc
        private readonly JsonServer<IAudioControllerHandler> _audioControllerHandlerServer;
        private readonly JsonClient<IAudioController> _audioControllerClient;
        private readonly IAudioController _audioController;
        #endregion

        public PlaySessionService(IMediaTransportService mtService)
        {
            #region Rpc
            _audioControllerHandlerServer = new JsonServer<IAudioControllerHandler>(s => new RpcCalledProxies.IAudioControllerHandlerRpcCalledProxy(s), this);
            _audioControllerClient = new JsonClient<IAudioController>(s => new RpcCallingProxies.IAudioControllerRpcCallingProxy(s));
            _audioControllerClient.OnSendMessage = m => _client.SendMessage(AudioRpc.RpcMessageTag, m);
            _audioController = _audioControllerClient.Proxy;
            #endregion

            _client = new BackgroundMediaPlayerClient(typeof(BackgroundAudioPlayerHandler));
            _playModeManager = IoC.Get<IPlayModeManager>();

            _client.MessageReceived += _client_MessageReceived;
            _client.PlayerActivated += _client_PlayerActivated;
            _mtService = mtService;
            _mtService.IsEnabled = _mtService.IsPauseEnabled = _mtService.IsPlayEnabled = true;
            _mtService.ButtonPressed += _mtService_ButtonPressed;
            _askPositionTimer = new Timer(OnAskPosition, null, Timeout.InfiniteTimeSpan, _askPositionPeriod);

            LoadState();
        }

        private void OnAskPosition(object state)
        {
            _audioController.AskPosition();
        }

        private void LoadState()
        {
            _currentPlayMode = _playModeManager.Providers[0];
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

        public void SetPlaylist(IList<TrackInfo> tracks, TrackInfo current)
        {
            Playlist = tracks;
            CurrentTrack = current;
        }

        private void Play()
        {
            PlaybackStatus = MediaPlaybackStatus.Changing;
            _audioController.Play();
        }

        private void Pause()
        {
            PlaybackStatus = MediaPlaybackStatus.Changing;
            _audioController.Pause();
        }

        private void MoveNext()
        {
            PlaybackStatus = MediaPlaybackStatus.Changing;
            _autoPlay = true;
            _audioController.MoveNext();
        }

        private void MovePrevious()
        {
            PlaybackStatus = MediaPlaybackStatus.Changing;
            _autoPlay = true;
            _audioController.MovePrevious();
        }

        private void OnCurrentTrackChanged()
        {
            if (CurrentTrack != null)
            {
                var idx = Playlist.IndexOf(CurrentTrack);
                if (idx != -1)
                {
                    CanPrevious = idx > 0 && PlaybackStatus != MediaPlaybackStatus.Changing;
                    CanNext = idx < Playlist.Count - 1 && PlaybackStatus != MediaPlaybackStatus.Changing;
                }
                else
                    CanPrevious = CanNext = false;
            }
            else
                CanPlay = CanPause = CanPrevious = CanNext = false;
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

        private void _client_PlayerActivated(object sender, object e)
        {
            _audioController.SetupHandler();
        }

        void IAudioControllerHandler.NotifyControllerReady()
        {
            Debug.WriteLine($"Player Received: Controller Ready.");
            _audioController.SetPlayMode(_currentPlayMode.Id);
        }

        void IAudioControllerHandler.NotifyMediaOpened()
        {
            if (_autoPlay)
            {
                Play();
                _autoPlay = false;
            }
        }

        void IAudioControllerHandler.NotifyControllerStateChanged(MediaPlayerState state)
        {
            PlatformProvider.Current.BeginOnUIThread(() =>
            {
                switch (state)
                {
                    case MediaPlayerState.Closed:
                        CanPlay = CanPause = false;
                        ShowPause = false;
                        ShowPlay = true;
                        PlaybackStatus = MediaPlaybackStatus.Closed;
                        SuspendAskPosition();
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
                        ResumeAskPosition();
                        break;
                    case MediaPlayerState.Paused:
                        CanPause = false;
                        CanPlay = true;
                        ShowPause = false;
                        ShowPlay = true;
                        PlaybackStatus = MediaPlaybackStatus.Paused;
                        SuspendAskPosition();
                        break;
                    case MediaPlayerState.Stopped:
                        PlaybackStatus = MediaPlaybackStatus.Stopped;
                        SuspendAskPosition();
                        break;
                    default:
                        break;
                }
            });
            Debug.WriteLine($"Player State Changed To: {state}.");
        }

        public void PlayWhenOpened()
        {
            _autoPlay = true;
            if (CanPlay)
                Play();
        }

        void IAudioControllerHandler.NotifyCurrentTrackChanged(TrackInfo track)
        {
            if (_playlist != null)
            {
                var currentTrack = _playlist.SingleOrDefault(o => o == track);
                Execute.BeginOnUIThread(() => CurrentTrack = currentTrack);
            }
        }

        void IAudioControllerHandler.NotifyDuration(TimeSpan? duration)
        {
            Execute.BeginOnUIThread(() => Duration = duration);
        }

        void IAudioControllerHandler.NotifyPosition(TimeSpan position)
        {
            Execute.BeginOnUIThread(() =>
            {
                _position = position;
                OnPropertyChanged(nameof(Position));
            });
        }

        private void OnPositionChanged(TimeSpan oldValue, TimeSpan value)
        {
            if(Math.Abs(oldValue.Subtract(value).TotalMilliseconds) > 100)
            {
                SuspendAskPosition();
                _audioController.SetPosition(value);
            }
        }

        private void SuspendAskPosition()
        {
            _askPositionTimer.Change(Timeout.InfiniteTimeSpan, _askPositionPeriod);
        }

        private void ResumeAskPosition()
        {
            _askPositionTimer.Change(TimeSpan.Zero, _askPositionPeriod);
        }

        void IAudioControllerHandler.NotifySeekCompleted()
        {
            ResumeAskPosition();
        }

        #region Rpc
        private void _client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Tag == AudioRpc.RpcMessageTag)
                OnReceiveMessage(e.Message);
            else
            {
                Debug.WriteLine($"Player Message:{e.Tag}, {e.Message}");
            }
        }

        private void OnReceiveMessage(string message)
        {
            _audioControllerHandlerServer.OnReceive(message);
        }
        #endregion
    }
}
