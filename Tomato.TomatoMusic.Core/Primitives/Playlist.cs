using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomato.TomatoMusic.Primitives
{
    public static class PlaylistVersions
    {
        public const string V1_0 = "1.0";
    }

    public class Playlist : IHasKey
    {
        public Guid Key { get; set; }

        public string Version { get; set; }

        public List<string> KnownFolders { get; set; } = new List<string>();

        public List<string> Folders { get; set; } = new List<string>();

        public List<TrackInfo> Tracks { get; set; } = new List<TrackInfo>();

        public static readonly Guid MusicLibraryPlaylistKey = new Guid("670B986E-8B0C-45B0-8A2E-BCAF019013D8");
        public static readonly Guid DefaultPlaylistKey = new Guid("3826B0E5-1A43-42AB-8C6B-AB8C89ADA77F");

        public static Playlist CreateMusicLibrary()
        {
            var playlist = new Playlist()
            {
                Key = MusicLibraryPlaylistKey
            };
            playlist.KnownFolders.Add(nameof(Windows.Storage.KnownFolders.MusicLibrary));
            return playlist;
        }
    }

    public class PlaylistCache : IHasKey
    {
        public Guid Key { get; set; }

        public Dictionary<string, FolderCache> KnownFolders { get; set; } = new Dictionary<string, FolderCache>();

        public Dictionary<string, FolderCache> Folders { get; set; } = new Dictionary<string, FolderCache>();
    }

    public class FolderCache
    {
        public List<TrackInfo> Tracks { get; set; } = new List<TrackInfo>();
    }
}
