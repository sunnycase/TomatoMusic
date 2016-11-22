using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Plugins;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.Storage;
using Windows.Media;
using Tomato.Mvvm;
using Windows.Media.Playback;
using Tomato.Media.Toolkit;
using Stateless;
using MediaPlayerState = Tomato.TomatoMusic.Primitives.MediaPlayerState;
using Tomato.TomatoMusic.Rpc;
using Windows.Media.Core;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Net.Http;
using System.Threading;

namespace Tomato.TomatoMusic.AudioTask
{
    public class AudioController : BindableBase, IAudioController
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly MediaPlaybackSession _playbackSession;
        private readonly IPlayModeManager _playModeManager;
        private readonly MediaEnvironment _mediaEnvironment = new MediaEnvironment();
        private IPlayModeProvider _currentPlayMode;

        private IReadOnlyList<TrackInfo> _playlist;
        private TrackInfo _currentTrack;
        private IReadOnlyList<MediaPlaybackItem> _playbackItems;

        private bool _autoPlay;
        private readonly ILog _logger = LogManager.GetLog(typeof(AudioController));
        private readonly MediaTransportService _mtService;
        private readonly StateMachine<MediaPlayerState, Triggers> _stateMachine;
        private readonly IAudioControllerHandler _audioControllerHandler;
        private MediaPlaybackList _mediaPlaybackList = new MediaPlaybackList();
        private TaskCompletionSource<object> _seekTaskCompletion;
        private int _ready = 0;

        #region Rpc
        private readonly AudioControllerRpcServer _acRpcServer;
        private readonly AudioControllerHandlerRpcClient _achRpcClient;
        #endregion

        public AudioController()
        {
            App.Startup();

            #region Rpc
            _acRpcServer = new AudioControllerRpcServer(this);
            _achRpcClient = new AudioControllerHandlerRpcClient();
            _audioControllerHandler = _achRpcClient.Service;
            #endregion

            _stateMachine = BuildStateMachine();
            _mediaEnvironment.RegisterDefaultCodecs();
            _playModeManager = IoC.Get<IPlayModeManager>();

            _mediaPlayer = BackgroundMediaPlayer.Current;
            _mediaPlayer.AutoPlay = false;
            _mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
            _mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
            _mediaPlayer.MediaEnded += mediaPlayer_MediaEnded;
            _playbackSession = _mediaPlayer.PlaybackSession;
            _playbackSession.PlaybackStateChanged += playbackSession_PlaybackStateChanged;
            _playbackSession.NaturalDurationChanged += playbackSession_NaturalDurationChanged;
            _playbackSession.SeekCompleted += playbackSession_SeekCompleted;

            _mtService = new MediaTransportService(_mediaPlayer.SystemMediaTransportControls);
            _mtService.IsEnabled = _mtService.IsPauseEnabled = _mtService.IsPlayEnabled = true;
            _mtService.ButtonPressed += _mtService_ButtonPressed;

            NotifyReady();
        }

        private StateMachine<MediaPlayerState, Triggers> BuildStateMachine()
        {
            var stateMachine = new StateMachine<MediaPlayerState, Triggers>(MediaPlayerState.Closed);
            stateMachine.Configure(MediaPlayerState.Closed)
                .Permit(Triggers.SetMediaPlaybackList, MediaPlayerState.PlaybackListSet);
            stateMachine.Configure(MediaPlayerState.PlaybackListSet)
                .PermitReentry(Triggers.SetMediaPlaybackList)
                .Permit(Triggers.RaiseOpening, MediaPlayerState.MediaOpening);
            stateMachine.Configure(MediaPlayerState.MediaOpening)
                .Permit(Triggers.RaisePaused, MediaPlayerState.Paused)
                .Permit(Triggers.RaiseMediaOpened, MediaPlayerState.MediaOpened)
                .Permit(Triggers.RaiseError, MediaPlayerState.Error);
            stateMachine.Configure(MediaPlayerState.MediaOpened)
                .Permit(Triggers.RaisePaused, MediaPlayerState.Paused)
                .Permit(Triggers.Play, MediaPlayerState.StartPlaying);
            stateMachine.Configure(MediaPlayerState.StartPlaying)
                .Permit(Triggers.RaisePlaying, MediaPlayerState.Playing)
                .Permit(Triggers.RaiseBuffering, MediaPlayerState.Buffering)
                .Permit(Triggers.RaiseError, MediaPlayerState.Error);
            stateMachine.Configure(MediaPlayerState.Playing)
                .Permit(Triggers.Pause, MediaPlayerState.Pausing)
                .Permit(Triggers.RaisePaused, MediaPlayerState.Paused)
                .Permit(Triggers.RaiseEnded, MediaPlayerState.Ended)
                .Permit(Triggers.SetMediaPlaybackList, MediaPlayerState.PlaybackListSet)
                .Permit(Triggers.RaiseError, MediaPlayerState.Error);
            stateMachine.Configure(MediaPlayerState.Pausing)
                .Permit(Triggers.RaisePaused, MediaPlayerState.Paused)
                .Permit(Triggers.SetMediaPlaybackList, MediaPlayerState.PlaybackListSet)
                .Permit(Triggers.RaiseError, MediaPlayerState.Error);
            stateMachine.Configure(MediaPlayerState.Paused)
                .Permit(Triggers.RaiseEnded, MediaPlayerState.Ended)
                .Permit(Triggers.RaiseMediaOpened, MediaPlayerState.MediaOpened)
                .Permit(Triggers.Play, MediaPlayerState.StartPlaying)
                .Permit(Triggers.SetMediaPlaybackList, MediaPlayerState.PlaybackListSet);
            stateMachine.Configure(MediaPlayerState.Ended)
                .Permit(Triggers.Play, MediaPlayerState.StartPlaying)
                .Permit(Triggers.SetMediaPlaybackList, MediaPlayerState.MediaOpening);
            stateMachine.Configure(MediaPlayerState.Error)
                .Permit(Triggers.SetMediaPlaybackList, MediaPlayerState.MediaOpening);

            return stateMachine;
        }

        private void OnMediaEnded()
        {
            _stateMachine.Fire(Triggers.RaiseEnded);
        }

        private void OnMediaFailed(MediaPlayerError error)
        {
            _stateMachine.Fire(Triggers.RaiseError);

            var taskComp = Interlocked.Exchange(ref _seekTaskCompletion, null);
            if (taskComp != null)
                taskComp.SetException(new InvalidOperationException($"MediaPlayerError: {error}."));
        }

        private void OnMediaOpened()
        {
            _stateMachine.Fire(Triggers.RaiseMediaOpened);
            if (_autoPlay)
            {
                _autoPlay = false;
                Play();
            }

            _mtService.IsPauseEnabled = _playbackSession.CanPause;
        }

        private void playbackSession_SeekCompleted(MediaPlaybackSession sender, object args)
        {
            var taskComp = Interlocked.Exchange(ref _seekTaskCompletion, null);
            if (taskComp != null)
                taskComp.SetResult(null);
        }

        private void playbackSession_NaturalDurationChanged(MediaPlaybackSession sender, object args)
        {
            _audioControllerHandler.OnNaturalDurationChanged(_playbackSession.NaturalDuration);
        }

        private void OnPlaybackStateChanged()
        {
            var state = _playbackSession.PlaybackState;
            switch (state)
            {
                case MediaPlaybackState.None:
                    break;
                case MediaPlaybackState.Opening:
                    _stateMachine.Fire(Triggers.RaiseOpening);
                    break;
                case MediaPlaybackState.Buffering:
                    _stateMachine.Fire(Triggers.RaiseBuffering);
                    break;
                case MediaPlaybackState.Playing:
                    _stateMachine.Fire(Triggers.RaisePlaying);
                    break;
                case MediaPlaybackState.Paused:
                    _stateMachine.Fire(Triggers.RaisePaused);
                    break;
            }
            _mtService.SetPlaybackState(state);
            _audioControllerHandler.OnMediaPlaybackStateChanged(state);
        }

        #region Event Handlers

        private void mediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            Execute.BeginOnUIThread(OnMediaEnded);
        }

        private void mediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Execute.BeginOnUIThread(() => OnMediaFailed(args.Error));
        }

        private void mediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            Execute.BeginOnUIThread(OnMediaOpened);
        }

        private void playbackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            Execute.BeginOnUIThread(OnPlaybackStateChanged);
        }

        #endregion

        public void Play()
        {
            if (_stateMachine.CanFire(Triggers.Play))
            {
                _stateMachine.Fire(Triggers.Play);
                _mediaPlayer.Play();
            }
        }

        public void Pause()
        {
            if (_stateMachine.CanFire(Triggers.Pause))
            {
                _stateMachine.Fire(Triggers.Pause);
                _mediaPlayer.Pause();
            }
        }

        public void MoveNext()
        {
            if (_mediaPlaybackList != null)
                _mediaPlaybackList.MoveNext();
        }

        public void MovePrevious()
        {
            if (_mediaPlaybackList != null)
                _mediaPlaybackList.MovePrevious();
        }

        void IAudioController.SetPlayMode(Guid id)
        {
            _currentPlayMode?.Detach();
            _currentPlayMode = _playModeManager.GetProvider(id);
            if (_mediaPlaybackList != null)
                _currentPlayMode.Attach(_mediaPlaybackList);
        }

        public void SetPosition(TimeSpan position)
        {
            _playbackSession.Position = position;
        }

        public void SetVolume(double value)
        {
            _mediaPlayer.Volume = value;
        }

        private void _mtService_ButtonPressed(object sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            OnMediaTransportControlsButtonPressed(e.Button);
        }

        private void OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton button)
        {
            switch (button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Pause();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    break;
                case SystemMediaTransportControlsButton.Record:
                    break;
                case SystemMediaTransportControlsButton.FastForward:
                    break;
                case SystemMediaTransportControlsButton.Rewind:
                    break;
                case SystemMediaTransportControlsButton.Next:
                    MoveNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    MovePrevious();
                    break;
                case SystemMediaTransportControlsButton.ChannelUp:
                    break;
                case SystemMediaTransportControlsButton.ChannelDown:
                    break;
                default:
                    break;
            }
        }

        public void OnCanceled()
        {
            _achRpcClient.OnCanceled();
            _acRpcServer.OnCanceled();
            _mtService.Dispose();
        }

        async void IAudioController.SetPlaylist(IReadOnlyList<TrackInfo> tracks, TrackInfo nextTrack, bool autoPlay)
        {
            if (_stateMachine.CanFire(Triggers.SetMediaPlaybackList))
            {
                _stateMachine.Fire(Triggers.SetMediaPlaybackList);
                var mediaPlaybackList = new MediaPlaybackList();
                mediaPlaybackList.ItemOpened += MediaPlaybackList_ItemOpened;
                mediaPlaybackList.ItemFailed += MediaPlaybackList_ItemFailed;
                mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
                var items = tracks.Select(o =>
               {
                   var streamRef = new UriRandomAccessStreamReference(o.Source);
                   return new MediaPlaybackItem(MediaSource.CreateFromStreamReference(streamRef, "audio/mp3"));
               }).ToList();
                items.Sink(mediaPlaybackList.Items.Add);
                mediaPlaybackList.StartingItem = items[tracks.IndexOf(nextTrack)];
                _currentPlayMode?.Detach();
                _currentPlayMode?.Attach(mediaPlaybackList);

                _playlist = tracks;
                _currentTrack = nextTrack;
                _playbackItems = items;

                _mediaPlaybackList = mediaPlaybackList;
                _autoPlay = autoPlay;
                _mediaPlayer.Source = mediaPlaybackList;
            }
        }

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            int index;
            if ((index = _playbackItems?.IndexOf(args.NewItem) ?? -1) != -1)
            {
                var playlist = _playlist;
                if (index < playlist?.Count)
                {
                    _currentTrack = playlist[index];
                    Execute.BeginOnUIThread(() => _audioControllerHandler.OnCurrentTrackChanged(_currentTrack));
                }
            }
        }

        private void MediaPlaybackList_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
        {

        }

        private void MediaPlaybackList_ItemOpened(MediaPlaybackList sender, MediaPlaybackItemOpenedEventArgs args)
        {

        }

        private void NotifyReady()
        {
            //if (Interlocked.CompareExchange(ref _ready, 1, 0) == 1))
            _audioControllerHandler.NotifyReady();
        }

        void IAudioController.SetCurrentTrack(TrackInfo track)
        {
            if (_playlist != null && _playbackItems != null)
            {
                var idx = _playlist.IndexOf(track);
                if (idx != -1)
                {
                    _mediaPlaybackList.StartingItem = _playbackItems[idx];
                    _currentTrack = track;
                }
            }
        }

        void IAudioController.OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton button)
        {
            OnMediaTransportControlsButtonPressed(button);
        }

        Task<TimeSpan> IAudioController.GetPosition()
        {
            return Task.FromResult(_playbackSession.Position);
        }

        Task IAudioController.Seek(TimeSpan position)
        {
            if (_playbackSession.CanSeek)
            {
                var taskComp = new TaskCompletionSource<object>();
                var oldTaskComp = Interlocked.CompareExchange(ref _seekTaskCompletion, taskComp, null);
                if (oldTaskComp != null)
                    taskComp = oldTaskComp;

                _playbackSession.Position = position;
                return taskComp.Task;
            }
            else
                return Task.FromResult<object>(null);
        }

        void IAudioController.AskIfReady()
        {
            NotifyReady();
        }

        void IAudioController.SetVolume(double volume)
        {
            _mediaPlayer.Volume = volume;
        }

        private enum Triggers
        {
            SetMediaPlaybackList,
            RaiseOpening,
            Play,
            RaiseMediaOpened,
            RaiseBuffering,
            RaisePlaying,
            Pause,
            RaisePaused,
            RaiseEnded,
            RaiseError
        }

        private class UriRandomAccessStreamReference : IRandomAccessStreamReference
        {
            private readonly Uri _uri;
            public UriRandomAccessStreamReference(Uri uri)
            {
                _uri = uri;
            }

            public IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadAsync() => OpenReadAsyncTask().AsAsyncOperation();

            async Task<IRandomAccessStreamWithContentType> OpenReadAsyncTask()
            {
                if (_uri.IsFile)
                {
                    var file = await StorageFile.GetFileFromPathAsync(_uri.LocalPath);
                    return await file.OpenReadAsync();
                }
                else
                {
                    return await RandomAccessStreamReference.CreateFromUri(_uri).OpenReadAsync();
                }
            }
        }
    }
}
