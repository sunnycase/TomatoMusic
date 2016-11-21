using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public class AudioControllerHandlerRpcClient
    {
        private readonly ICallingProxyDispatcher _dispatcher;
        private readonly AudioControllerHandlerCallingProxy _proxy;

        public IAudioControllerHandler Service => _proxy;

        public AudioControllerHandlerRpcClient()
        {
            _dispatcher = new CallingProxyDispatcher(OnSendPacket)
            {
                Timeout = AudioRpc.Timeout
            };
            _proxy = new AudioControllerHandlerCallingProxy(_dispatcher);
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            string tag, content;
            AudioRpc.DestructMessage(e.Data, out tag, out content);
            if (tag == AudioRpc.RpcMessageTagAudioControllerHandler)
                _dispatcher.Receive(JsonConvert.DeserializeObject<RpcAnswerPacket>(content, AudioRpc.AnswerPacketJsonSettings));
        }

        private Task OnSendPacket(RpcPacket packet)
        {
            var json = JsonConvert.SerializeObject(packet, AudioRpc.JsonSettings);
            BackgroundMediaPlayer.SendMessageToForeground(AudioRpc.ConstructMessage(AudioRpc.RpcMessageTagAudioControllerHandler, json));
            return Task.FromResult<object>(null);
        }

        public void OnCanceled()
        {

        }
    }
}
