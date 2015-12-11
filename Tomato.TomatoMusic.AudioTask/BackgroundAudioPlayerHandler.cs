using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;
using Tomato.Rpc.Json;
using Tomato.TomatoMusic.Core;

namespace Tomato.TomatoMusic.AudioTask
{
    public sealed class BackgroundAudioPlayerHandler : IBackgroundMediaPlayerHandler
    {
        private AudioController _audioController;

        public BackgroundAudioPlayerHandler()
        {
        }

        public void OnActivated(BackgroundMediaPlayer mediaPlayer)
        {
            _audioController = new AudioController(mediaPlayer);
        }

        public void OnReceiveMessage(string message)
        {
            if (message.StartsWith(AudioRpcPacketBuilders.RpcMessagePrefix))
                _audioController?.OnReceiveMessage(message.Substring(AudioRpcPacketBuilders.RpcMessagePrefix.Length));
            else
            {
                Debug.WriteLine($"Client Message: {message}");
            }
        }

        public void Play()
        {

        }
    }
}
