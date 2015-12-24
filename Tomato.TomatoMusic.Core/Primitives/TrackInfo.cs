using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Primitives
{
    public class TrackInfo : IEquatable<TrackInfo>
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

        public bool Equals(TrackInfo other)
        {
            if (other == null)
                return false;
            if (object.ReferenceEquals(this, other))
                return true;
            return Source == other.Source;
        }

        public static bool operator==(TrackInfo left, TrackInfo right)
        {
            if (object.ReferenceEquals(left, null))
            {
                if (object.ReferenceEquals(right, null))
                    return true;
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(TrackInfo left, TrackInfo right)
        {
            if (object.ReferenceEquals(left, null))
            {
                if (object.ReferenceEquals(right, null))
                    return false;
                return true;
            }
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            var right = obj as TrackInfo;
            return this == right;
        }

        public override int GetHashCode()
        {
            return Source?.GetHashCode() ?? 0;
        }
    }
}
