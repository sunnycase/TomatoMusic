using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.Storage;

namespace Tomato.TomatoMusic.Primitives
{
    public interface IPlaylistContentProvider
    {
        Task<IObservableCollection<TrackInfo>> Result { get; }
        void AddFolder(StorageFolder folder);
    }
}
