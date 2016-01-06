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
            App.Startup();
        }

        public void OnActivated(BackgroundMediaPlayer mediaPlayer)
        {
            _audioController = new AudioController(mediaPlayer);
        }

        public void OnCanceled()
        {
            _audioController.OnCanceled();
        }

        public void OnReceiveMessage(string tag, string message)
        {
            if (tag == AudioRpc.RpcMessageTag)
                _audioController?.OnReceiveMessage(message);
            else
            {
                Debug.WriteLine($"Client Message: {tag}, {message}");
            }
        }
    }
}
