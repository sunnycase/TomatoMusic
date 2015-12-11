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
using Tomato.Uwp.Mvvm;
using Windows.Media.Playback;

namespace Tomato.TomatoPlayer.Shell.Services
{
    class PlaySessionService : BindableBase, IAudioControllerHandler
    {
        private bool _canPrevious;
        public bool CanPrevious
        {
            get { return _canPrevious; }
            private set { SetProperty(ref _canPrevious, value); }
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
            private set { SetProperty(ref _canNext, value); }
        }

        private readonly BackgroundMediaPlayerClient _client;
        private readonly JsonClient<IAudioController> _audioControllerClient = new JsonClient<IAudioController>(AudioRpcPacketBuilders.AudioController);
        private readonly IAudioController _audioController;
        private readonly JsonServer<IAudioControllerHandler> _audioControllerHandlerServer;

        public PlaySessionService()
        {
            _audioControllerHandlerServer = new JsonServer<IAudioControllerHandler>(this, AudioRpcPacketBuilders.AudioControllerHandler);
            _client = new BackgroundMediaPlayerClient(typeof(BackgroundAudioPlayerHandler).FullName);
            _client.MessageReceived += _client_MessageReceived;
            _audioControllerClient.OnSendMessage = m => _client.SendMessage(AudioRpcPacketBuilders.RpcMessagePrefix + m);
            _audioController = _audioControllerClient.Proxy;
            _client.PlayerActivated += _client_PlayerActivated;
        }

        private void _client_PlayerActivated(object sender, object e)
        {
            _audioController.SetupHandler();
        }

        private void _client_MessageReceived(object sender, string message)
        {
            if (message.StartsWith(AudioRpcPacketBuilders.RpcMessagePrefix))
                OnReceiveMessage(message.Substring(AudioRpcPacketBuilders.RpcMessagePrefix.Length));
            else
            {
                Debug.WriteLine($"Player Message: {message}");
            }
        }

        public void NotifyControllerReady()
        {
            Debug.WriteLine($"Player Received: Controller Ready.");
            _audioController.SetMediaSource(new Uri("ms-appx:///Assets/04 - irony -TV Mix-.mp3"));
        }

        public void OnReceiveMessage(string message)
        {
            _audioControllerHandlerServer.OnReceive(message);
        }

        public void NotifyMediaOpened()
        {
            _audioController.Play();
        }

        public void NotifyControllerStateChanged(MediaPlayerState state)
        {
            Debug.WriteLine($"Player State Changed To: {state}.");
        }
    }
}
