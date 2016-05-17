using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
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
using System.IO;
using Tomato.Mvvm;
using Tomato.TomatoMusic.Plugins.Cache;
using Windows.Storage.Streams;

namespace Tomato.TomatoMusic.Plugins.Services
{
    class MediaMetadataService : IMediaMetadataService
    {
        private readonly LastfmClient _lastfm;
        private readonly GeciMeClient _geciMe;
        private readonly MoeAtHomeClient _moeAtHome;

        private readonly AlbumCoverCache _albumCoverCache = new AlbumCoverCache();
        private readonly LyricsCache _lyricsCache = new LyricsCache();
        private readonly MetadataConfiguration _metadataConfiguration;
        private readonly ILocalLyricsService _localLyricsService;

        public MediaMetadataService(LastFmConfig lastFmConfig, IConfigurationService configurationService, ILocalLyricsService localLyricsService)
        {
            _metadataConfiguration = configurationService.Metadata;
            _lastfm = new LastfmClient(lastFmConfig.ApiKey, null);
            _geciMe = new GeciMeClient();
            _moeAtHome = new MoeAtHomeClient();
            _localLyricsService = localLyricsService;

            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            _lastfm.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TomatoMusic",
                $"{packageVersion.Major}.{packageVersion.Minor}"));
        }

        public async Task<ITrackMediaMetadata> GetMetadata(TrackInfo track)
        {
            var meta = TrackMediaMetadata.Default();

            await TryLoadLyricsFromLocal(track, meta);
            await TryFillFromTrack(track, meta);
            if (meta.Cover == null)
                await TryLoadCoverFromCache(track, meta);
            if (string.IsNullOrEmpty(meta.Lyrics))
                await TryLoadLyricsFromCache(track, meta);
            if (meta.Cover == null && EnvironmentHelper.HasInternetConnection(!_metadataConfiguration.UpdateAlbumCoverEvenByteBasis))
                TryFillFromLastFm(track, meta).Ignore();
            if (string.IsNullOrEmpty(meta.Lyrics) && EnvironmentHelper.HasInternetConnection(!_metadataConfiguration.UpdateLyricsEvenByteBasis))
                TryFillLyrics(track, meta);
            return meta;
        }

        private async Task TryLoadLyricsFromLocal(TrackInfo track, TrackMediaMetadata meta)
        {
            if (_localLyricsService != null)
            {
                var file = await _localLyricsService.TryGetLocalLyrics(track);
                if (file != null)
                {
                    try
                    {
                        meta.Lyrics = await FileIO.ReadTextAsync(file);
                    }
                    catch { }
                }
            }
        }

        private async void TryFillLyrics(TrackInfo track, TrackMediaMetadata meta)
        {
            if (string.IsNullOrEmpty(meta.Lyrics) && EnvironmentHelper.HasInternetConnection(!_metadataConfiguration.UpdateLyricsEvenByteBasis))
                await TryFillFromMoeAtHome(track, meta);
            if (string.IsNullOrEmpty(meta.Lyrics) && EnvironmentHelper.HasInternetConnection(!_metadataConfiguration.UpdateLyricsEvenByteBasis))
                await TryFillFromGeciMe(track, meta);
        }

        private async Task TryLoadLyricsFromCache(TrackInfo track, TrackMediaMetadata meta)
        {
            var text = await _lyricsCache.TryGetCache(track.Title, track.Artist);
            if (!string.IsNullOrEmpty(text))
                meta.Lyrics = text;
        }

        private async Task TryLoadCoverFromCache(TrackInfo track, TrackMediaMetadata meta)
        {
            var stream = await _albumCoverCache.TryGetCache(track.Album, track.Artist);
            if (stream != null)
                meta.Cover = await MediaHelper.CreateImage(stream);
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
                var cover = provider.Pictures.FirstOrDefault(o => o.PictureType.Contains("Cover"));
                if (cover == null)
                    cover = provider.Pictures.FirstOrDefault();
                if (cover != null)
                {
                    var stream = new MemoryStream(cover.Data);
                    meta.Cover = await MediaHelper.CreateImage(stream.AsRandomAccessStream());
                }
                if (string.IsNullOrEmpty(meta.Lyrics))
                    meta.Lyrics = provider.Lyrics;
            }
            catch { }
        }

        private async Task TryFillFromLastFm(TrackInfo track, TrackMediaMetadata meta)
        {
            try
            {
                var uri = await GetCoverUriFromLastFm(track.Artist, track.Album);
                if (uri == null)
                {
                    var trackResp = await _lastfm.Track.GetInfoAsync(track.Title ?? string.Empty, track.Artist ?? string.Empty);
                    if (trackResp.Success)
                        uri = await GetCoverUriFromLastFm(trackResp.Content.ArtistName, trackResp.Content.AlbumName);
                }
                if (uri != null)
                {
                    var stream = await _albumCoverCache.Download(track.Album, track.Artist, uri);
                    if (stream != null)
                        meta.Cover = await MediaHelper.CreateImage(stream);
                }
            }
            catch { }
        }

        private async Task<Uri> GetCoverUriFromLastFm(string artist, string album)
        {
            try
            {
                var response = await _lastfm.Album.GetInfoAsync(artist ?? string.Empty, album ?? string.Empty);
                if (response.Success)
                    return response.Content.Images.ExtraLarge;
            }
            catch { }
            return null;
        }

        private async Task TryFillFromGeciMe(TrackInfo track, TrackMediaMetadata meta)
        {
            try
            {
                var uri = await _geciMe.GetLyrics(track.Title, track.Artist);
                if (uri != null)
                    meta.Lyrics = await _lyricsCache.Download(track.Title, track.Artist, uri);
            }
            catch { }
        }

        private async Task TryFillFromMoeAtHome(TrackInfo track, TrackMediaMetadata meta)
        {
            try
            {
                var uri = await _moeAtHome.GetLyrics(track.Title, track.Artist);
                if (uri != null)
                    meta.Lyrics = await _lyricsCache.Download(track.Title, track.Artist, uri);
            }
            catch { }
        }
    }

    class TrackMediaMetadata : BindableBase, ITrackMediaMetadata
    {
        private ImageSource _cover;
        public ImageSource Cover
        {
            get { return _cover; }
            set { SetProperty(ref _cover, value); }
        }

        private string _lyrics;
        public string Lyrics
        {
            get { return _lyrics; }
            set { SetProperty(ref _lyrics, value); }
        }

        public static TrackMediaMetadata Default() => new TrackMediaMetadata
        {

        };
    }
}
