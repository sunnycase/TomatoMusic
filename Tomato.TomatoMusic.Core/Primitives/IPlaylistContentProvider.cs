using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.Storage;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Tomato.TomatoMusic.Primitives
{
    public interface IPlaylistContentProvider : INotifyPropertyChanged
    {
        Task<IObservableCollection<TrackInfo>> Result { get; }
        void AddFolder(StorageFolder folder);
        Task<IReadOnlyList<StorageFolder>> GetFolders();
        bool IsRefreshing { get; }

        void UpdateFolders(IReadOnlyList<StorageFolder> folders);
    }
}
