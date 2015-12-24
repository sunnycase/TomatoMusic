using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Tomato.Media;
using Tomato.Rpc.Json;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Plugins;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.Storage;

namespace Tomato.TomatoMusic.AudioTask
{
    class AudioController : IAudioController
    {
        private readonly JsonServer<IAudioController> _audioControllerServer;
        private readonly BackgroundMediaPlayer _mediaPlayer;

        private readonly JsonClient<IAudioControllerHandler> _controllerHandlerClient;
        private readonly IAudioControllerHandler _controllerHandler;
        private readonly IPlayModeManager _playModeManager;
        private IPlayModeProvider _currentPlayMode;

        private IList<TrackInfo> _playlist;
        private TrackInfo _currentTrack;

        private bool _autoPlay;

        public AudioController(BackgroundMediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
            _audioControllerServer = new JsonServer<IAudioController>(this, AudioRpcPacketBuilders.AudioController);
            _controllerHandlerClient = new JsonClient<IAudioControllerHandler>(AudioRpcPacketBuilders.AudioControllerHandler);
            _controllerHandler = _controllerHandlerClient.Proxy;
            _playModeManager = IoC.Get<IPlayModeManager>();

            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
        }

        private void MediaPlayer_MediaEnded(IMediaPlayer sender, object args)
        {
            var playlist = _playlist;
            var currentTrack = _currentTrack;
            var playMode = _currentPlayMode;
            if(playMode != null && playlist != null && currentTrack != null)
            {
                var nextTrack = playMode.SelectNextTrack(playlist, currentTrack);
                if(nextTrack != null)
                {
                    _autoPlay = true;
                    SetCurrentTrack(nextTrack);
                }
            }
        }

        private void MediaPlayer_CurrentStateChanged(IMediaPlayer sender, object args)
        {
            _controllerHandler.NotifyControllerStateChanged(_mediaPlayer.State);
        }

        private void MediaPlayer_MediaOpened(IMediaPlayer sender, object args)
        {
            if(_autoPlay)
            {
                _autoPlay = false;
                Play();
            }
            _controllerHandler.NotifyMediaOpened();
        }

        public void OnReceiveMessage(string message)
        {
            _audioControllerServer.OnReceive(message);
        }

        public void Play()
        {
            _mediaPlayer.Play();
        }

        public void SetupHandler()
        {
            _controllerHandlerClient.OnSendMessage = m => _mediaPlayer.SendMessage(AudioRpcPacketBuilders.RpcMessageTag, m);
            _controllerHandler.NotifyControllerReady();
        }

        private async void SetMediaSource(Uri uri)
        {
            StorageFile file;
            if (uri.Scheme == "ms-appx")
                file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            else if (uri.IsFile)
                file = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
            else
                throw new NotSupportedException("Not supported uri.");
            var stream = await file.OpenReadAsync();
            var mediaSource = await MediaSource.CreateFromStream(stream);
            Debug.WriteLine($"Title: {mediaSource.Title}");
            Debug.WriteLine($"Album: {mediaSource.Album}");
            _mediaPlayer.SetMediaSource(mediaSource);
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
            if(_currentTrack != track)
            {
                _currentTrack = track;
                if (_currentTrack != null)
                {
                    SetMediaSource(track.Source);
                    _controllerHandler.NotifyCurrentTrackChanged(track);
                }
            }
        }

        public void MoveNext()
        {
            var cntIdx = GetCurrentTrackIndex();
            if (cntIdx != -1)
            {
                var nextIdx = cntIdx + 1;
                if (nextIdx >= 0 && nextIdx < _playlist.Count)
                    SetCurrentTrack(_playlist[nextIdx]);
            }
        }

        public void MovePrevious()
        {
            var cntIdx = GetCurrentTrackIndex();
            if (cntIdx != -1)
            {
                var prevIdx = cntIdx - 1;
                if (prevIdx >= 0 && prevIdx < _playlist.Count)
                    SetCurrentTrack(_playlist[prevIdx]);
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
    }
}
