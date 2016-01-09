using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Tomato.TomatoMusic.Primitives
{
    public interface ITrackMediaMetadata : INotifyPropertyChanged
    {
        ImageSource Cover { get; }
        string Lyrics { get; }
    }
}
