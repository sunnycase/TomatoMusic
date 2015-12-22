using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Tomato.TomatoMusic.Primitives
{
    public interface IPlaylistContentProvider
    {
        Task<IObservableCollection<TrackInfo>> Result { get; }
    }
}
