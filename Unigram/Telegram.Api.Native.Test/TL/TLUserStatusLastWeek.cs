// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLUserStatusLastWeek : TLUserStatusBase 
	{
		public TLUserStatusLastWeek() { }
		public TLUserStatusLastWeek(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.UserStatusLastWeek; } }

		public override void Read(TLBinaryReader from)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteUInt32(0x7BF09FC);
		}
	}
}