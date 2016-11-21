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
    public class AudioControllerRpcClient
    {
        private readonly ICallingProxyDispatcher _dispatcher;
        private readonly AudioControllerCallingProxy _proxy;

        public IAudioController Service => _proxy;

        public AudioControllerRpcClient()
        {
            _dispatcher = new CallingProxyDispatcher(OnSendPacket)
            {
                Timeout = AudioRpc.Timeout
            };
            _proxy = new AudioControllerCallingProxy(_dispatcher);
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            string tag, content;
            AudioRpc.DestructMessage(e.Data, out tag, out content);
            if (tag == AudioRpc.RpcMessageTagAudioController)
                _dispatcher.Receive(JsonConvert.DeserializeObject<RpcAnswerPacket>(content, AudioRpc.AnswerPacketJsonSettings));
        }

        private Task OnSendPacket(RpcPacket packet)
        {
            var json = JsonConvert.SerializeObject(packet, AudioRpc.JsonSettings);
            BackgroundMediaPlayer.SendMessageToBackground(AudioRpc.ConstructMessage(AudioRpc.RpcMessageTagAudioController, json));
            return Task.FromResult<object>(null);
        }
    }
}
