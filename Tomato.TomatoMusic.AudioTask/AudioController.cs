using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;
using Tomato.Rpc.Json;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Primitives;
using Windows.Storage;

namespace Tomato.TomatoMusic.AudioTask
{
    class AudioController : IAudioController
    {
        private readonly JsonServer<IAudioController> _audioControllerServer;
        private readonly BackgroundMediaPlayer _mediaPlayer;

        private readonly JsonClient<IAudioControllerHandler> _controllerHandlerClient;
        private readonly IAudioControllerHandler _controllerHandler;

        private IList<TrackInfo> _playlist;
        private TrackInfo _currentTrack;

        public AudioController(BackgroundMediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
            _audioControllerServer = new JsonServer<IAudioController>(this, AudioRpcPacketBuilders.AudioController);
            _controllerHandlerClient = new JsonClient<IAudioControllerHandler>(AudioRpcPacketBuilders.AudioControllerHandler);
            _controllerHandler = _controllerHandlerClient.Proxy;

            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
        }

        private void MediaPlayer_CurrentStateChanged(IMediaPlayer sender, object args)
        {
            _controllerHandler.NotifyControllerStateChanged(_mediaPlayer.State);
        }

        private void MediaPlayer_MediaOpened(IMediaPlayer sender, object args)
        {
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
            _currentTrack = track;
            if (_currentTrack != null)
                SetMediaSource(track.Source);
        }
    }
}
