using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;
using Tomato.TomatoMusic.AudioTask;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Services;
using Tomato.Mvvm;
using Windows.Media;
using Windows.Media.Playback;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Plugins;
using System.Threading;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Audio.Config;
using Tomato.TomatoMusic.Messages;
using Windows.Storage;
using Newtonsoft.Json;
using Tomato.TomatoMusic.Rpc;

namespace Tomato.TomatoMusic.Audio.Services
{
    class PlaySessionService : BindableBase, IAudioControllerHandler, IPlaySessionService, IHandle<SuspendStateMessage>, IHandle<ResumeStateMessage>
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

        public MediaPlaybackStatus PlaybackStatus => _mtService.PlaybackStatus;

        private IReadOnlyList<TrackInfo> _playlist = new List<TrackInfo>();
        public IReadOnlyList<TrackInfo> Playlist
        {
            get { return _playlist; }
            private set { SetProperty(ref _playlist, value); }
        }

        private TrackInfo _currentTrack;
        public TrackInfo CurrentTrack
        {
            get { return _currentTrack; }
            private set
            {
                if (SetProperty(ref _currentTrack, value))
                {
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
                if (SetProperty(ref _position, value))
                    OnPositionChanged(value);
            }
        }

        private bool _isMuted;
        public bool IsMuted
        {
            get { return _isMuted; }
            set { SetProperty(ref _isMuted, value); }
        }

        private double _volume;
        public double Volume
        {
            get { return _volume; }
            set
            {
                if (SetProperty(ref _volume, value))
                    OnVolumeChanged();
            }
        }

        private IPlayModeProvider _playMode;
        public IPlayModeProvider PlayMode
        {
            get { return _playMode; }
            set
            {
                if (SetProperty(ref _playMode, value))
                    OnPlayModeChanged();
            }
        }

        private readonly IMediaTransportService _mtService;
        private readonly IPlayModeManager _playModeManager;
        private readonly Timer _askPositionTimer;
        private static readonly TimeSpan _askPositionPeriod = TimeSpan.FromSeconds(0.25);
        private PlayerConfiguration _playerConfig;
        private readonly ILog _logger;
        private readonly IAudioController _audioController;

        #region Rpc
        private readonly AudioControllerHandlerRpcServer _achRpcServer;
        private readonly AudioControllerRpcClient _acRpcClient;
        #endregion

        public PlaySessionService(AudioModuleConfig config, IEventAggregator eventAggregator)
        {
            #region Rpc
            _achRpcServer = new AudioControllerHandlerRpcServer(this);
            _acRpcClient = new AudioControllerRpcClient();
            _audioController = _acRpcClient.Service;
            #endregion
            eventAggregator.Subscribe(this);
            _logger = LogManager.GetLog(typeof(PlaySessionService));
            _playModeManager = IoC.Get<IPlayModeManager>();

            _mtService = new MediaTransportService();
            _mtService.IsEnabled = _mtService.IsPauseEnabled = _mtService.IsPlayEnabled = true;
            _mtService.ButtonPressed += _mtService_ButtonPressed;
            _askPositionTimer = new Timer(OnAskPosition, null, Timeout.InfiniteTimeSpan, _askPositionPeriod);

            _audioController.AskIfReady();
        }

        private void TryAskPreviousStates()
        {
            try
            {
            }
            catch { }
        }

        private void LoadState()
        {
            var configService = IoC.Get<IConfigurationService>();
            _playerConfig = configService.Player;
            PlayMode = _playModeManager.GetProvider(_playerConfig.PlayMode);
            Volume = _playerConfig.Volume;
            //_playerConfig.EqualizerParameters.CollectionChanged += EqualizerParameters_CollectionChanged;

            ResumeState();
        }

        public void RequestPlay()
        {
            _audioController.OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton.Play);
        }

        public void RequestPause()
        {
            _audioController.OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton.Pause);
        }

        public void RequestNext()
        {
            _audioController.OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton.Next);
        }

        public void RequestPrevious()
        {
            _audioController.OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton.Previous);
        }

        public void SetPlaylist(IReadOnlyList<TrackInfo> tracks, TrackInfo current)
        {
            Playlist = tracks;
            _currentTrack = current;
            OnPropertyChanged(nameof(CurrentTrack));
            _audioController.SetPlaylist(tracks, current);
        }

        private async void OnPositionChanged(TimeSpan value)
        {
            if (PlaybackStatus != MediaPlaybackStatus.Changing)
            {
                SuspendAskPosition();
                await _audioController.Seek(value);
                ResumeAskPosition();
            }
        }

        private void OnAskPosition(object state)
        {
            Execute.OnUIThread(async () =>
            {
                _position = await _audioController.GetPosition();
                OnPropertyChanged(nameof(Position));
            });
        }

        private void OnCurrentTrackChanged()
        {
            if (CurrentTrack != null)
            {
                CanPrevious = CanNext = true;
            }
            else
                CanPlay = CanPause = CanPrevious = CanNext = false;
        }

        private void _mtService_ButtonPressed(object sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            _audioController.OnMediaTransportControlsButtonPressed(e.Button);
        }

        public void PlayWhenOpened()
        {

        }

        private void EqualizerParameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void SuspendAskPosition()
        {
            _askPositionTimer.Change(Timeout.InfiniteTimeSpan, _askPositionPeriod);
        }

        private void ResumeAskPosition()
        {
            _askPositionTimer.Change(TimeSpan.Zero, _askPositionPeriod);
        }

        public void ScrollPlayMode()
        {
            var playModes = _playModeManager.Providers;
            var nextIdx = _playModeManager.Providers.IndexOf(PlayMode) + 1;
            if (nextIdx >= playModes.Count)
                nextIdx = 0;
            PlayMode = playModes[nextIdx];
        }

        private void OnPlayModeChanged()
        {
            // _audioController.SetPlayMode(PlayMode.Id);
            _playerConfig.PlayMode = PlayMode.Id;
            _playerConfig.Save();
        }

        private void OnVolumeChanged()
        {
            _audioController.SetVolume(Math.Min(Math.Max(0.0, Volume / 100), 1.0));
            _playerConfig.Volume = Volume;
            _playerConfig.Save();
        }

        public void NotifyPlaylist(IReadOnlyList<TrackInfo> playlist)
        {
            Execute.BeginOnUIThread(() =>
            {
                //SetProperty(ref _playlist, playlist ?? new List<TrackInfo>(), nameof(Playlist));
            });
        }

        private void OnMediaPlaybackStateChanged(MediaPlaybackState state)
        {
            switch (state)
            {
                case MediaPlaybackState.None:
                    break;
                case MediaPlaybackState.Opening:
                    ShowPause = CanPause = true;
                    ShowPlay = CanPlay = false;
                    break;
                case MediaPlaybackState.Buffering:
                    break;
                case MediaPlaybackState.Playing:
                    ShowPause = CanPause = true;
                    ShowPlay = CanPlay = false;
                    ResumeAskPosition();
                    break;
                case MediaPlaybackState.Paused:
                    ShowPause = CanPause = false;
                    ShowPlay = CanPlay = true;
                    SuspendAskPosition();
                    break;
                default:
                    break;
            }
        }

        private void ResumeState()
        {
            try
            {
                if (BackgroundMediaPlayer.IsMediaPlaying()) return;
                object stateObj;
                if (_stateContainer.Value.Values.TryGetValue("State", out stateObj))
                {
                    var state = JsonConvert.DeserializeObject<State>(stateObj.ToString());
                    Playlist = state.Playlist;
                    CurrentTrack = state.CurrentTrack;
                    _audioController.SetPlaylist(Playlist, CurrentTrack, false);
                    Position = state.Position;
                }
            }
            catch { }
        }

        private void SuspendState()
        {
            _stateContainer.Value.Values["State"] = JsonConvert.SerializeObject(new State
            {
                Playlist = Playlist,
                CurrentTrack = CurrentTrack,
                Position = Position
            });
        }

        private class State
        {
            public IReadOnlyList<TrackInfo> Playlist { get; set; }
            public TrackInfo CurrentTrack { get; set; }
            public TimeSpan Position { get; set; }
        }

        private Lazy<ApplicationDataContainer> _stateContainer = new Lazy<ApplicationDataContainer>(() =>
            ApplicationData.Current.LocalSettings.CreateContainer("Tomato.TomatoMusic.PlaySession", ApplicationDataCreateDisposition.Always));

        void IHandle<SuspendStateMessage>.Handle(SuspendStateMessage message)
        {
            SuspendState();
        }

        void IHandle<ResumeStateMessage>.Handle(ResumeStateMessage message)
        {
            ResumeState();
        }

        void IAudioControllerHandler.OnMediaPlaybackStateChanged(MediaPlaybackState state)
        {
            _mtService.SetPlaybackState(state);
            Execute.BeginOnUIThread(() => OnMediaPlaybackStateChanged(state));
        }

        void IAudioControllerHandler.OnCurrentTrackChanged(TrackInfo track)
        {
            Execute.BeginOnUIThread(() => CurrentTrack = track);
        }

        void IAudioControllerHandler.OnNaturalDurationChanged(TimeSpan duration)
        {
            Execute.BeginOnUIThread(() => Duration = duration);
        }

        void IAudioControllerHandler.NotifyReady()
        {
            Execute.BeginOnUIThread(() =>
            {
                LoadState();
                TryAskPreviousStates();
            });
        }
    }
}
