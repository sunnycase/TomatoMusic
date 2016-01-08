using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.TomatoMusic.Configuration;

namespace Tomato.TomatoMusic.Services
{
    public interface IConfigurationService
    {
        PlayerConfiguration Player { get; }
        ThemeConfiguration Theme { get; }
        MetadataConfiguration Metadata { get; }
    }
}
