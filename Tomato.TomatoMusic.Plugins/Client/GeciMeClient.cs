using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tomato.TomatoMusic.Plugins.Client
{
    class GeciMeClient
    {
        private readonly HttpClient _httpClient;

        public GeciMeClient()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://geci.me/api/lyric/")
            };
        }

        public async Task<Uri> GetLyrics(string title, string artist)
        {
            var uri = await GetLyricsCore(title, artist);
            if (uri == null)
                return await GetLyricsCore(PreProcess(title), artist);
            return uri;
        }

        public async Task<Uri> GetLyricsCore(string title, string artist)
        {
            var result = JsonConvert.DeserializeObject<ApiResult>(await _httpClient.GetStringAsync($"{title}/{artist}"));
            if (result.Count == 0)
                result = JsonConvert.DeserializeObject<ApiResult>(await _httpClient.GetStringAsync($"{title}"));
            if (result.Count != 0)
                return result.Result.First().Lrc;
            return null;
        }

        private string PreProcess(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;
            var sb = new StringBuilder(title);
            sb.Replace("TV Mix", "");
            sb.Replace("-", "");
            sb.Replace("~", "");
            return sb.ToString().Trim();
        }

        class LyricResult
        {
            public Uri Lrc { get; set; }
        }

        class ApiResult
        {
            public int Code { get; set; }
            public int Count { get; set; }
            public List<LyricResult> Result { get; set; } = new List<LyricResult>();
        }
    }
}
