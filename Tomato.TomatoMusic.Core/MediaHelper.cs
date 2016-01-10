using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Tomato.TomatoMusic
{
    public static class MediaHelper
    {
        public static async Task<ImageSource> CreateImage(IRandomAccessStream stream)
        {
            try
            {
                using (stream)
                {
                    var image = new BitmapImage();
                    await image.SetSourceAsync(stream);
                    return image;
                }
            }
            catch { }
            return null;
        }
    }
}
