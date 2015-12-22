using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tomato.TomatoMusic.Primitives;
using Windows.Storage;

namespace Tomato.TomatoMusic.Playlist.Models
{
    class PlaylistFile
    {
        private const string PlaylistFolder = "Playlist";
        private readonly Primitives.Playlist _playlist;
        public Primitives.Playlist Playlist
        {
            get { return _playlist; }
        }

        public PlaylistFile(Primitives.Playlist value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            _playlist = value;
        }

        private static async Task<StorageFile> OpenPlaylistFile(PlaylistPlaceholder placeholder)
        {
            var folder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync(PlaylistFolder, CreationCollisionOption.OpenIfExists);
            return await folder.CreateFileAsync($"{placeholder.Key.ToString("N")}.json", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<PlaylistFile> OpenAsync(PlaylistPlaceholder placeholder)
        {
            Primitives.Playlist model = null;
            try
            {
                var file = await OpenPlaylistFile(placeholder);
                model = JsonConvert.DeserializeObject<Primitives.Playlist>(await FileIO.ReadTextAsync(file));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Flatten());
            }
            HandleNullModel(ref model, placeholder);
            return new PlaylistFile(model);
        }

        private static void HandleNullModel(ref Primitives.Playlist model, PlaylistPlaceholder placeholder)
        {
            if(model == null)
            {
                if (placeholder.Key == Primitives.Playlist.MusicLibraryPlaylistKey)
                    model = Primitives.Playlist.CreateMusicLibrary();
                else
                    model = new Primitives.Playlist
                    {
                        Key = placeholder.Key
                    };
            }
        }
    }
}
