using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;

namespace Tomato.TomatoMusic.Configuration
{
    public class PlaylistViewConfiguration : ConfigurationBase
    {
        private readonly Guid _playlistKey;
        public const string KeyPrefix = "PlaylistViewConfiguration.";
        public override string RuntimeKey => KeyPrefix + _playlistKey.ToString("N");

        public MusicsViewType ViewType { get; set; }
        public MusicsOrderRule OrderRule { get; set; }

        public PlaylistViewConfiguration(PlaylistPlaceholder playlist)
        {
            _playlistKey = playlist.Key;
        }
    }
}
