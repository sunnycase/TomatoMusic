using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lex.Db;
using Tomato.TomatoMusic.Primitives;
using Windows.Storage;

namespace Tomato.TomatoMusic.Plugins.Providers
{
    /// <summary>
    /// 本地歌词提供程序
    /// </summary>
    class LocalLyricsProvider : IDisposable
    {
        private const string _storagePath = @"Plugins\LocalLyrics\db\";
        private readonly DbInstance _db;

        public LocalLyricsProvider()
        {
            _db = new DbInstance(_storagePath, ApplicationData.Current.LocalFolder);
            _db.Map<WithSourceMapping>()
                .Automap(o => o.Source);
            _db.Initialize();
        }

        public void ClearLocalLyricsPath(TrackInfo track)
        {
            _db.DeleteByKey<WithSourceMapping>(track.Source.ToString());
        }

        public string TryGetLocalLyricsPath(TrackInfo track)
        {
            return _db.LoadByKey<WithSourceMapping>(track.Source.ToString())?.Lyrics;
        }

        public void SetLocalLyricsPath(TrackInfo track, string lyricsPath)
        {
            _db.Save(new WithSourceMapping
            {
                Source = track.Source.ToString(),
                Lyrics = lyricsPath
            });
        }

        class WithSourceMapping
        {
            public string Source { get; set; }
            public string Lyrics { get; set; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _db.Flush();
                    _db.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
