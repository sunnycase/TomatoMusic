using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tomato.TomatoMusic.Plugins.Client
{
    class MoeAtHomeClient
    {
        private readonly HttpClient _httpClient;

        public MoeAtHomeClient()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://sunnycase.moe/api/lyrics/")
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
            LyricResult result = null;
            try
            {
                result = JsonConvert.DeserializeObject<LyricResult>(await _httpClient.GetStringAsync($"find/{title}/{artist}"));
            }
            catch { }
            try
            {
                if (result == null)
                    result = JsonConvert.DeserializeObject<LyricResult>(await _httpClient.GetStringAsync($"find/{title}"));
            }
            catch { }
            if (result != null)
                return new Uri(_httpClient.BaseAddress, $"download/{result.FileName}");
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
            public string FileName { get; set; }
        }
    }
}
