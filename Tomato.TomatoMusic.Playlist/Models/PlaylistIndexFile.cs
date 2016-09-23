using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tomato.TomatoMusic.Primitives;
using Windows.Storage;

namespace Tomato.TomatoMusic.Playlist.Models
{
    class PlaylistIndex
    {
        public List<PlaylistPlaceholder> Playlists { get; set; } = new List<PlaylistPlaceholder>();
    }

    class PlaylistIndexFile
    {
        private const string PlaylistFolder = "Playlist";
        private readonly PlaylistIndex _playlistIndex;
        public PlaylistIndex PlaylistIndex
        {
            get { return _playlistIndex; }
        }

        public PlaylistIndexFile(PlaylistIndex value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            _playlistIndex = value;
        }

        private static async Task<StorageFile> OpenPlaylistIndexFile()
        {
            var folder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync(PlaylistFolder, CreationCollisionOption.OpenIfExists);
            return await folder.CreateFileAsync("index.json", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<PlaylistIndexFile> OpenAsync()
        {
            PlaylistIndex model = null;
            try
            {
                var file = await OpenPlaylistIndexFile();
                model = JsonConvert.DeserializeObject<PlaylistIndex>(await FileIO.ReadTextAsync(file));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Flatten());
            }
            return new PlaylistIndexFile(model ?? new PlaylistIndex());
        }

        public async Task Save()
        {
            try
            {
                var file = await OpenPlaylistIndexFile();
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(PlaylistIndex));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Flatten());
            }
        }
    }
}
