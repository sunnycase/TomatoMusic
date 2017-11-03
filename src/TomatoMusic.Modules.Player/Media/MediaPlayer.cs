using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMMediaPlayer = Windows.Media.Playback.MediaPlayer;

namespace TomatoMusic.Media
{
    /// <summary>
    /// 媒体播放器
    /// </summary>
    public class MediaPlayer : IMediaPlayer
    {
        private readonly WMMediaPlayer _mediaPlayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPlayer"/> class.
        /// </summary>
        public MediaPlayer()
        {
            _mediaPlayer = new WMMediaPlayer();
        }
    }
}
