using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;

namespace Tomato.TomatoMusic.AudioTask
{
    public sealed class BackgroundAudioHandler : IBackgroundMediaPlayerHandler
    {
        private readonly BackgroundAudioPlayerDispatcher _dispatcher;

        public BackgroundAudioHandler()
        {
            _dispatcher = new BackgroundAudioPlayerDispatcher();
        }

        public void OnActivated(BackgroundMediaPlayer mediaPlayer)
        {
            _dispatcher.OnActivated(mediaPlayer);
        }

        public void OnCanceled()
        {
            _dispatcher.OnCanceled();
        }

        public void OnReceiveMessage(string tag, string message)
        {
            _dispatcher.OnReceiveMessage(tag, message);
        }
    }
}
