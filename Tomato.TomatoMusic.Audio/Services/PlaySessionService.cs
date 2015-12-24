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

        private bool _autoPlay = false;

        private readonly BackgroundMediaPlayerClient _client;
        private readonly JsonClient<IAudioController> _audioControllerClient = new JsonClient<IAudioController>(AudioRpcPacketBuilders.AudioController);
        private readonly IAudioController _audioController;
        private readonly JsonServer<IAudioControllerHandler> _audioControllerHandlerServer;
        private readonly IMediaTransportService _mtService;
        private readonly IPlayModeManager _playModeManager;
        private IPlayModeProvider _currentPlayMode;

        public PlaySessionService(IMediaTransportService mtService)
        {
            _audioControllerHandlerServer = new JsonServer<IAudioControllerHandler>(this, AudioRpcPacketBuilders.AudioControllerHandler);
            _client = new BackgroundMediaPlayerClient(typeof(BackgroundAudioPlayerHandler));
            _playModeManager = IoC.Get<IPlayModeManager>();

            _client.MessageReceived += _client_MessageReceived; ;
            _audioControllerClient.OnSendMessage = m => _client.SendMessage(AudioRpcPacketBuilders.RpcMessageTag, m);
            _audioController = _audioControllerClient.Proxy;
            _client.PlayerActivated += _client_PlayerActivated;
            _mtService = mtService;
            _mtService.IsEnabled = _mtService.IsPauseEnabled = _mtService.IsPlayEnabled = true;
            _mtService.ButtonPressed += _mtService_ButtonPressed;

            LoadState();
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

        private void _client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Tag == AudioRpcPacketBuilders.RpcMessageTag)
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
    }
}
