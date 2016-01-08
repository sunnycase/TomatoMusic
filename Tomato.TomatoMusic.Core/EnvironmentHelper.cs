using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Tomato.TomatoMusic
{
    public static class EnvironmentHelper
    {
        public static bool HasInternetConnection(bool ignorePerByteBasis = true)
        {
            var internet = NetworkInformation.GetInternetConnectionProfile();
            if (internet != null)
                return !(ignorePerByteBasis && internet.GetConnectionCost().NetworkCostType == NetworkCostType.Variable);
            return false;
        }
    }
}
