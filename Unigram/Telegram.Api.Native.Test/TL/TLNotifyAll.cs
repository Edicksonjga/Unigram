// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLNotifyAll : TLNotifyPeerBase 
	{
		public TLNotifyAll() { }
		public TLNotifyAll(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.NotifyAll; } }

		public override void Read(TLBinaryReader from)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteUInt32(0x74D07C60);
		}
	}
}