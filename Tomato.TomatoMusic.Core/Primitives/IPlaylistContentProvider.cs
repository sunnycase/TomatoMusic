using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.Storage;
using System.ComponentModel;

namespace Tomato.TomatoMusic.Primitives
{
    public interface IPlaylistContentProvider : INotifyPropertyChanged
    {
        Task<IObservableCollection<TrackInfo>> Result { get; }
        void AddFolder(StorageFolder folder);
        bool IsRefreshing { get; }
    }
}
