using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Media;
using Windows.Storage;

namespace Tomato.TomatoMusic.AudioTask
{
    public sealed class BackgroundAudioPlayerHandler : IBackgroundMediaPlayerHandler
    {
        private IMediaPlayer _player;

        public async void OnActivated(BackgroundMediaPlayer mediaPlayer)
        {
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/04 - irony -TV Mix-.mp3"));
            var stream = await file.OpenReadAsync();
            var mediaSource = await MediaSource.CreateFromStream(stream);
            Debug.WriteLine($"Title: {mediaSource.Title}");
            Debug.WriteLine($"Album: {mediaSource.Album}");

            mediaPlayer.SetMediaSource(mediaSource);
            _player = mediaPlayer;
        }

        private void MediaPlayer_MediaOpened(IMediaPlayer sender, object args)
        {
            sender.Play();
        }
    }
}
