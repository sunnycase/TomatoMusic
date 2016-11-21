using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc;
using Tomato.TomatoMusic.Primitives;
using Windows.Media.Playback;

namespace Tomato.TomatoMusic.Core.Rpc
{
    partial class AudioControllerHandlerCallingProxy : IAudioControllerHandler
    {
        Tomato.Rpc.Core.ICallingProxyDispatcher Dispatcher
        {
            get;
        }

        public AudioControllerHandlerCallingProxy(Tomato.Rpc.Core.ICallingProxyDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public void NotifyReady()
        {
            Dispatcher.DoCall(new Packets.NotifyReady__{});
        }

        public void OnMediaPlaybackStateChanged(MediaPlaybackState state)
        {
            Dispatcher.DoCall(new Packets.OnMediaPlaybackStateChanged__global_3A_3AWindows_Media_Playback_MediaPlaybackState{Arg0 = state});
        }

        public void OnCurrentTrackChanged(TrackInfo track)
        {
            Dispatcher.DoCall(new Packets.OnCurrentTrackChanged__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo{Arg0 = track});
        }

        public void OnNaturalDurationChanged(TimeSpan duration)
        {
            Dispatcher.DoCall(new Packets.OnNaturalDurationChanged__global_3A_3ASystem_TimeSpan{Arg0 = duration});
        }
    }

    partial class AudioControllerHandlerCalledProxy : Tomato.Rpc.Core.IHandleRpcPacket<Packets.NotifyReady__>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.OnMediaPlaybackStateChanged__global_3A_3AWindows_Media_Playback_MediaPlaybackState>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.OnCurrentTrackChanged__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.OnNaturalDurationChanged__global_3A_3ASystem_TimeSpan>
    {
        IAudioControllerHandler Service
        {
            get;
        }

        public AudioControllerHandlerCalledProxy(IAudioControllerHandler service)
        {
            Service = service;
        }

        public void Handle(Packets.NotifyReady__ args)
        {
            Service.NotifyReady();
        }

        public void Handle(Packets.OnMediaPlaybackStateChanged__global_3A_3AWindows_Media_Playback_MediaPlaybackState args)
        {
            Service.OnMediaPlaybackStateChanged(args.Arg0);
        }

        public void Handle(Packets.OnCurrentTrackChanged__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo args)
        {
            Service.OnCurrentTrackChanged(args.Arg0);
        }

        public void Handle(Packets.OnNaturalDurationChanged__global_3A_3ASystem_TimeSpan args)
        {
            Service.OnNaturalDurationChanged(args.Arg0);
        }
    }

    namespace Packets
    {
        sealed class NotifyReady__
        {
        }

        sealed class OnMediaPlaybackStateChanged__global_3A_3AWindows_Media_Playback_MediaPlaybackState
        {
            public MediaPlaybackState Arg0;
        }

        sealed class OnCurrentTrackChanged__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo
        {
            public TrackInfo Arg0;
        }

        sealed class OnNaturalDurationChanged__global_3A_3ASystem_TimeSpan
        {
            public TimeSpan Arg0;
        }
    }
}