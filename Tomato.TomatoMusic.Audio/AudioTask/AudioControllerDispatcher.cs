using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;
using Tomato.Rpc.Json;
using Tomato.TomatoMusic.Core;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.AudioTask
{
    sealed class AudioControllerDispatcher
    {
        private readonly JsonServer<IAudioController> _audioControllerServer;
        private readonly JsonClient<IAudioControllerHandler> _controllerHandlerClient;

        public IAudioControllerHandler ControllerHandler => _controllerHandlerClient.Proxy;

        public AudioControllerDispatcher(IAudioController controller)
        {
            App.Startup();
            _audioControllerServer = new JsonServer<IAudioController>(s => new RpcCalledProxies.IAudioControllerRpcCalledProxy(s), controller);
            _controllerHandlerClient = new JsonClient<IAudioControllerHandler>(s => new RpcCallingProxies.IAudioControllerHandlerRpcCallingProxy(s));
            _controllerHandlerClient.OnSendMessage = OnSendMessage;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            string tag, content;
            AudioRpc.DestructMessage(e.Data, out tag, out content);
            if (tag == AudioRpc.RpcMessageTag)
                _audioControllerServer.OnReceive(content);
            else
            {
                Debug.WriteLine($"Server Message: {tag}, {content}");
            }
        }

        private void OnSendMessage(string message)
        {
            var valueSet = AudioRpc.ConstructMessage(AudioRpc.RpcMessageTag, message);
            BackgroundMediaPlayer.SendMessageToForeground(valueSet);
        }

        public void OnCanceled()
        {
            BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
        }
    }
}
