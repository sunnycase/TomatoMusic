using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Tomato.TomatoMusic.Services
{
    public interface IThemeService : INotifyPropertyChanged
    {
        ImageSource Background { get; }
    }
}
