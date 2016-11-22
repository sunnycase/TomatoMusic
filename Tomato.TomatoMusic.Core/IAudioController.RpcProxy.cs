using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomato.Rpc;
using Tomato.TomatoMusic.Primitives;
using Windows.Media;

namespace Tomato.TomatoMusic.Core.Rpc
{
    partial class AudioControllerCallingProxy : IAudioController
    {
        Tomato.Rpc.Core.ICallingProxyDispatcher Dispatcher
        {
            get;
        }

        public AudioControllerCallingProxy(Tomato.Rpc.Core.ICallingProxyDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public void AskIfReady()
        {
            Dispatcher.DoCall(new Packets.IAudioController_AskIfReady__{});
        }

        public void SetPlaylist(IReadOnlyList<TrackInfo> tracks, TrackInfo nextTrack, bool autoPlay = true)
        {
            Dispatcher.DoCall(new Packets.IAudioController_SetPlaylist__global_3A_3ASystem_Collections_Generic_IReadOnlyList_3Cglobal_3A_3ATomato_TomatoMusic_Primitives_TrackInfo_3E__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo__bool{Arg0 = tracks, Arg1 = nextTrack, Arg2 = autoPlay});
        }

        public void SetCurrentTrack(TrackInfo track)
        {
            Dispatcher.DoCall(new Packets.IAudioController_SetCurrentTrack__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo{Arg0 = track});
        }

        public void OnMediaTransportControlsButtonPressed(SystemMediaTransportControlsButton button)
        {
            Dispatcher.DoCall(new Packets.IAudioController_OnMediaTransportControlsButtonPressed__global_3A_3AWindows_Media_SystemMediaTransportControlsButton{Arg0 = button});
        }

        public async Task<TimeSpan> GetPosition()
        {
            return (TimeSpan)await Dispatcher.DoCallAndWaitAnswer(new Packets.IAudioController_GetPosition__{});
        }

        public async Task Seek(TimeSpan position)
        {
            await Dispatcher.DoCallAndWaitAnswer(new Packets.IAudioController_Seek__global_3A_3ASystem_TimeSpan{Arg0 = position});
        }

        public void SetVolume(double volume)
        {
            Dispatcher.DoCall(new Packets.IAudioController_SetVolume__double{Arg0 = volume});
        }

        public void SetPlayMode(Guid id)
        {
            Dispatcher.DoCall(new Packets.IAudioController_SetPlayMode__global_3A_3ASystem_Guid{Arg0 = id});
        }
    }

    partial class AudioControllerCalledProxy : Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_AskIfReady__>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_SetPlaylist__global_3A_3ASystem_Collections_Generic_IReadOnlyList_3Cglobal_3A_3ATomato_TomatoMusic_Primitives_TrackInfo_3E__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo__bool>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_SetCurrentTrack__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_OnMediaTransportControlsButtonPressed__global_3A_3AWindows_Media_SystemMediaTransportControlsButton>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_GetPosition__, Task<TimeSpan>>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_Seek__global_3A_3ASystem_TimeSpan, Task>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_SetVolume__double>, Tomato.Rpc.Core.IHandleRpcPacket<Packets.IAudioController_SetPlayMode__global_3A_3ASystem_Guid>
    {
        IAudioController Service
        {
            get;
        }

        public AudioControllerCalledProxy(IAudioController service)
        {
            Service = service;
        }

        public void Handle(Packets.IAudioController_AskIfReady__ args)
        {
            Service.AskIfReady();
        }

        public void Handle(Packets.IAudioController_SetPlaylist__global_3A_3ASystem_Collections_Generic_IReadOnlyList_3Cglobal_3A_3ATomato_TomatoMusic_Primitives_TrackInfo_3E__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo__bool args)
        {
            Service.SetPlaylist(args.Arg0, args.Arg1, args.Arg2);
        }

        public void Handle(Packets.IAudioController_SetCurrentTrack__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo args)
        {
            Service.SetCurrentTrack(args.Arg0);
        }

        public void Handle(Packets.IAudioController_OnMediaTransportControlsButtonPressed__global_3A_3AWindows_Media_SystemMediaTransportControlsButton args)
        {
            Service.OnMediaTransportControlsButtonPressed(args.Arg0);
        }

        public Task<TimeSpan> Handle(Packets.IAudioController_GetPosition__ args)
        {
            return Service.GetPosition();
        }

        public Task Handle(Packets.IAudioController_Seek__global_3A_3ASystem_TimeSpan args)
        {
            return Service.Seek(args.Arg0);
        }

        public void Handle(Packets.IAudioController_SetVolume__double args)
        {
            Service.SetVolume(args.Arg0);
        }

        public void Handle(Packets.IAudioController_SetPlayMode__global_3A_3ASystem_Guid args)
        {
            Service.SetPlayMode(args.Arg0);
        }
    }

    namespace Packets
    {
        sealed class IAudioController_AskIfReady__
        {
        }

        sealed class IAudioController_SetPlaylist__global_3A_3ASystem_Collections_Generic_IReadOnlyList_3Cglobal_3A_3ATomato_TomatoMusic_Primitives_TrackInfo_3E__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo__bool
        {
            public IReadOnlyList<TrackInfo> Arg0;
            public TrackInfo Arg1;
            public bool Arg2;
        }

        sealed class IAudioController_SetCurrentTrack__global_3A_3ATomato_TomatoMusic_Primitives_TrackInfo
        {
            public TrackInfo Arg0;
        }

        sealed class IAudioController_OnMediaTransportControlsButtonPressed__global_3A_3AWindows_Media_SystemMediaTransportControlsButton
        {
            public SystemMediaTransportControlsButton Arg0;
        }

        sealed class IAudioController_GetPosition__
        {
        }

        sealed class IAudioController_Seek__global_3A_3ASystem_TimeSpan
        {
            public TimeSpan Arg0;
        }

        sealed class IAudioController_SetVolume__double
        {
            public double Arg0;
        }

        sealed class IAudioController_SetPlayMode__global_3A_3ASystem_Guid
        {
            public Guid Arg0;
        }
    }
}