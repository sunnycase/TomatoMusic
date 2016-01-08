using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Configuration
{
    public sealed class MetadataConfiguration : ConfigurationBase
    {
        public const string Key = "MetadataConfiguration";
        public override string RuntimeKey => Key;
        
        private bool _updateAlbumCoverEvenByteBasis = false;
        public bool UpdateAlbumCoverEvenByteBasis
        {
            get { return _updateAlbumCoverEvenByteBasis; }
            set { SetProperty(ref _updateAlbumCoverEvenByteBasis, value); }
        }

        private bool _updateLyricsEvenByteBasis = true;
        public bool UpdateLyricsEvenByteBasis
        {
            get { return _updateLyricsEvenByteBasis; }
            set { SetProperty(ref _updateLyricsEvenByteBasis, value); }
        }
    }
}
