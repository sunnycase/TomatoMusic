using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Primitives;

namespace Tomato.TomatoMusic.Services
{
    public interface IMediaMetadataService
    {
        Task<ITrackMediaMetadata> GetMetadata(TrackInfo track);
    }
}
