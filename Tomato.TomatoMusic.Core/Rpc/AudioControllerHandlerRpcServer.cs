using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc;
using Tomato.Rpc.Core;
using Tomato.TomatoMusic.Core;
using Tomato.TomatoMusic.Core.Rpc;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.Rpc
{
    public class AudioControllerHandlerRpcServer
    {
        private readonly CalledProxyDispatcher<AudioControllerHandlerCalledProxy> _dispatcher;

        public AudioControllerHandlerRpcServer(IAudioControllerHandler service)
        {
            _dispatcher = new CalledProxyDispatcher<AudioControllerHandlerCalledProxy>(new AudioControllerHandlerCalledProxy(service), OnSendPacket);
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            string tag, content;
            AudioRpc.DestructMessage(e.Data, out tag, out content);
            if (tag == AudioRpc.RpcMessageTagAudioControllerHandler)
                _dispatcher.Receive(JsonConvert.DeserializeObject<RpcPacket>(content, AudioRpc.JsonSettings));
        }

        private Task OnSendPacket(RpcAnswerPacket packet)
        {
            var json = JsonConvert.SerializeObject(packet, AudioRpc.AnswerPacketJsonSettings);
            BackgroundMediaPlayer.SendMessageToBackground(AudioRpc.ConstructMessage(AudioRpc.RpcMessageTagAudioControllerHandler, json));
            return Task.FromResult<object>(null);
        }
    }
}
