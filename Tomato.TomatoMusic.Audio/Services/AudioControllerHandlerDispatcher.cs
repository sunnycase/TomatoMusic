using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc.Json;
using Tomato.TomatoMusic.Core;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.Audio.Services
{
    class AudioControllerHandlerDispatcher
    {
        private readonly JsonServer<IAudioControllerHandler> _audioControllerHandlerServer;
        private readonly JsonClient<IAudioController> _audioControllerClient;
        private readonly IAudioController _audioController;
        private readonly object _messageLocker = new object();

        public IAudioController AudioController => _audioControllerClient.Proxy;

        public AudioControllerHandlerDispatcher(IAudioControllerHandler handler)
        {
            _audioControllerHandlerServer = new JsonServer<IAudioControllerHandler>(s => new RpcCalledProxies.IAudioControllerHandlerRpcCalledProxy(s), handler);
            _audioControllerClient = new JsonClient<IAudioController>(s => new RpcCallingProxies.IAudioControllerRpcCallingProxy(s));
            _audioControllerClient.OnSendMessage = OnSendMessage;
            TryRegisterMessageHandlers();
        }

        private void OnSendMessage(string message)
        {
            var valueSet = AudioRpc.ConstructMessage(AudioRpc.RpcMessageTag, message);
            try
            {
                BackgroundMediaPlayer.SendMessageToBackground(valueSet);
            }
            catch (Exception ex) when(ex.HResult == RPC_S_SERVER_UNAVAILABLE)
            {
                ResetMessageHandlers();
                BackgroundMediaPlayer.SendMessageToBackground(valueSet);
            }
        }

        private void ResetMessageHandlers()
        {
            BackgroundMediaPlayer.Shutdown();
            RegisterMessageHandlers();
        }

        private void RegisterMessageHandlers()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void TryRegisterMessageHandlers()
        {
            try
            {
                RegisterMessageHandlers();
            }
            catch (Exception ex) when (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
            {
                ResetMessageHandlers();
            }
        }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            string tag, content;
            AudioRpc.DestructMessage(e.Data, out tag, out content);
            if (tag == AudioRpc.RpcMessageTag)
                _audioControllerHandlerServer.OnReceive(content);
            else
            {
                Debug.WriteLine($"Client Message: {tag}, {content}");
            }
        }

        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA
    }
}
