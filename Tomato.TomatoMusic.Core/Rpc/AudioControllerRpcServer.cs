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
    public class AudioControllerRpcServer
    {
        private readonly CalledProxyDispatcher<AudioControllerCalledProxy> _dispatcher;

        public AudioControllerRpcServer(IAudioController service)
        {
            _dispatcher = new CalledProxyDispatcher<AudioControllerCalledProxy>(new AudioControllerCalledProxy(service), OnSendPacket);
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            string tag, content;
            AudioRpc.DestructMessage(e.Data, out tag, out content);
            if (tag == AudioRpc.RpcMessageTagAudioController)
                _dispatcher.Receive(JsonConvert.DeserializeObject<RpcPacket>(content, AudioRpc.JsonSettings));
        }

        private Task OnSendPacket(RpcAnswerPacket packet)
        {
            var json = JsonConvert.SerializeObject(packet, AudioRpc.AnswerPacketJsonSettings);
            BackgroundMediaPlayer.SendMessageToForeground(AudioRpc.ConstructMessage(AudioRpc.RpcMessageTagAudioController, json));
            return Task.FromResult<object>(null);
        }

        public void OnCanceled()
        {

        }
    }
}
