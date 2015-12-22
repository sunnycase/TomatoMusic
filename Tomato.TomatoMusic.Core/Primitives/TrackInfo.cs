using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Primitives
{
    public class TrackInfo
    {
        public Uri Source { get; set; }

        public string Title { get; set; }

        public string Album { get; set; }

        public Uri CoverThumbnail { get; set; }

        public string AlbumArtist { get; set; }

        public string Artist { get; set; }

        public TimeSpan? Duration { get; set; }

        public class ExistenceEqualityComparer : EqualityComparer<TrackInfo>
        {
            public ExistenceEqualityComparer()
            {

            }

            public override bool Equals(TrackInfo x, TrackInfo y)
            {
                return (x?.Source == y?.Source);
            }

            public override int GetHashCode(TrackInfo obj)
            {
                return obj?.Source?.GetHashCode() ?? 0;
            }
        }
    }
}
