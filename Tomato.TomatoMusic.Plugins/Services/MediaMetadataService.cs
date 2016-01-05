using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using Kfstorm.LrcParser;
using Tomato.TomatoMusic.Plugins.Client;
using Tomato.TomatoMusic.Plugins.Config;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Tomato.TomatoMusic.Plugins.Services
{
    class MediaMetadataService : IMediaMetadataService
    {
        private readonly LastfmClient _lastfm;
        private readonly GeciMeClient _geciMe;

        public MediaMetadataService(LastFmConfig lastFmConfig)
        {
            _lastfm = new LastfmClient(lastFmConfig.ApiKey, null);
            _geciMe = new GeciMeClient();

            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            _lastfm.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TomatoMusic",
                $"{packageVersion.Major}.{packageVersion.Minor}"));
        }

        public async Task<ITrackMediaMetadata> GetMetadata(TrackInfo track)
        {
            var meta = TrackMediaMetadata.Default();
            await TryFillFromLastFm(track, meta);
            await TryFillFromGeciMe(track, meta);
            return meta;
        }

        private async Task TryFillFromLastFm(TrackInfo track, TrackMediaMetadata meta)
        {
            try
            {
                var response = await _lastfm.Album.GetInfoAsync(track.Artist, track.Album);
                if (response.Success)
                {
                    meta.Cover = new BitmapImage(response.Content.Images.ExtraLarge);
                }
            }
            catch { }
        }

        private async Task TryFillFromGeciMe(TrackInfo track, TrackMediaMetadata meta)
        {
            try
            {
                meta.Lyrics = await _geciMe.GetLyrics(track.Title, track.Artist);
            }
            catch { }
        }
    }

    class TrackMediaMetadata : ITrackMediaMetadata
    {
        public ImageSource Cover { get; set; }
        public string Lyrics { get; set; }

        public static TrackMediaMetadata Default() => new TrackMediaMetadata
        {

        };
    }
}
