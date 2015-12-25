﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Tomato.TomatoMusic.RpcPackets.IAudioController;

namespace Tomato.TomatoMusic.RpcCallingProxies
{
	sealed class IAudioControllerRpcCallingProxy : Tomato.TomatoMusic.Core.IAudioController, global::Tomato.Rpc.Core.IPacketReceiver
	{
		private readonly global::Tomato.Rpc.Core.IPacketSender _packetSender;

		public IAudioControllerRpcCallingProxy(global::Tomato.Rpc.Core.IPacketSender packetSender)
		{
			_packetSender = packetSender;
		}

		void global::Tomato.Rpc.Core.IPacketReceiver.OnReceive(object packet)
		{
		}

		public void SetupHandler()
		{
			var packet = new Void_20SetupHandler_28_29
			{
			};
			_packetSender.Send(packet);
		}

		public void Play()
		{
			var packet = new Void_20Play_28_29
			{
			};
			_packetSender.Send(packet);
		}

		public void Pause()
		{
			var packet = new Void_20Pause_28_29
			{
			};
			_packetSender.Send(packet);
		}

		public void SetPlaylist(System.Collections.Generic.IList<Tomato.TomatoMusic.Primitives.TrackInfo> tracks)
		{
			var packet = new Void_20SetPlaylist_28System_Collections_Generic_IList_601_5BTomato_TomatoMusic_Primitives_TrackInfo_5D_29
			{
				Arg0 = tracks,
			};
			_packetSender.Send(packet);
		}

		public void SetCurrentTrack(Tomato.TomatoMusic.Primitives.TrackInfo track)
		{
			var packet = new Void_20SetCurrentTrack_28Tomato_TomatoMusic_Primitives_TrackInfo_29
			{
				Arg0 = track,
			};
			_packetSender.Send(packet);
		}

		public void MoveNext()
		{
			var packet = new Void_20MoveNext_28_29
			{
			};
			_packetSender.Send(packet);
		}

		public void MovePrevious()
		{
			var packet = new Void_20MovePrevious_28_29
			{
			};
			_packetSender.Send(packet);
		}

		public void SetPlayMode(System.Guid id)
		{
			var packet = new Void_20SetPlayMode_28System_Guid_29
			{
				Arg0 = id,
			};
			_packetSender.Send(packet);
		}

	}
}
