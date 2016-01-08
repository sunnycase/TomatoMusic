using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using Kfstorm.LrcParser;
using Tomato.Media;
using Tomato.TomatoMusic.Configuration;
using Tomato.TomatoMusic.Plugins.Client;
using Tomato.TomatoMusic.Plugins.Config;
using Tomato.TomatoMusic.Primitives;
using Tomato.TomatoMusic.Services;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Tomato.TomatoMusic.Plugins.Services
{
    class MediaMetadataService : IMediaMetadataService
    {
        private readonly LastfmClient _lastfm;
        private readonly GeciMeClient _geciMe;

        private readonly MetadataConfiguration _metadataConfiguration;

        public MediaMetadataService(LastFmConfig lastFmConfig, IConfigurationService configurationService)
        {
            _metadataConfiguration = configurationService.Metadata;
            _lastfm = new LastfmClient(lastFmConfig.ApiKey, null);
            _geciMe = new GeciMeClient();

            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            _lastfm.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TomatoMusic",
                $"{packageVersion.Major}.{packageVersion.Minor}"));
        }

        public async Task<ITrackMediaMetadata> GetMetadata(TrackInfo track)
        {
            var meta = TrackMediaMetadata.Default();

            if (meta.Cover == null && EnvironmentHelper.HasInternetConnection(!_metadataConfiguration.UpdateAlbumCoverEvenByteBasis))
                await TryFillFromLastFm(track, meta);
            if (string.IsNullOrEmpty(meta.Lyrics) && EnvironmentHelper.HasInternetConnection(!_metadataConfiguration.UpdateLyricsEvenByteBasis))
                await TryFillFromGeciMe(track, meta);
            return meta;
        }

        private async Task TryFillFromTrack(TrackInfo track, TrackMediaMetadata meta)
        {
            try
            {
                var uri = track.Source;
                StorageFile file;
                if (uri.Scheme == "ms-appx")
                    file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                else if (uri.IsFile)
                    file = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
                else
                    throw new NotSupportedException("not supported uri.");
                var provider = await MediaMetadataProvider.CreateFromStream(await file.OpenReadAsync(), false);
                
            }
            catch { }
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
