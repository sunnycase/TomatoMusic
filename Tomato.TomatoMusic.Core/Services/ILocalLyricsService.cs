using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;
using Windows.Storage;

namespace Tomato.TomatoMusic.Services
{
    public interface ILocalLyricsService
    {
        Task<IStorageFile> TryGetLocalLyrics(TrackInfo track);
        void SetLocalLyrics(TrackInfo track, IStorageFile storageFile);
        void ClearLocalLyricsPath(TrackInfo track);
    }
}
