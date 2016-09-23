using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.Storage;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Tomato.TomatoMusic.Primitives
{
    public interface IPlaylistContentProvider : INotifyPropertyChanged
    {
        IReadOnlyCollection<TrackInfo> Tracks { get; }
        event NotifyCollectionChangedEventHandler TracksChanged;

        void AddFolder(StorageFolder folder);
        Task<IReadOnlyList<StorageFolder>> GetFolders();
        bool IsRefreshing { get; }
        void UpdateFolders(IReadOnlyList<StorageFolder> folders);
    }
}
