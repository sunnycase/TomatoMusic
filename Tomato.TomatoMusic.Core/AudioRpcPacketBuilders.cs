using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc.Json;

namespace Tomato.TomatoMusic.Core
{
    public static class AudioRpcPacketBuilders
    {
        private static readonly Lazy<PacketBuilder> _audioController = new Lazy<PacketBuilder>(() =>
         {
             var pb = new PacketBuilder(typeof(IAudioController));
             pb.Build();
             return pb;
         });
        public static PacketBuilder AudioController => _audioController.Value;

        private static readonly Lazy<PacketBuilder> _audioControllerHandler = new Lazy<PacketBuilder>(() =>
        {
            var pb = new PacketBuilder(typeof(IAudioControllerHandler));
            pb.Build();
            return pb;
        });
        public static PacketBuilder AudioControllerHandler => _audioControllerHandler.Value;

        public const string RpcMessagePrefix = "Tomato.TomatoMusic.RpcCall:";
    }
}
