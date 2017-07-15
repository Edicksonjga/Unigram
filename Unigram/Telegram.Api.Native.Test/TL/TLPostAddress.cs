// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLPostAddress : TLObject 
	{
		public String StreetLine1 { get; set; }
		public String StreetLine2 { get; set; }
		public String City { get; set; }
		public String State { get; set; }
		public String CountryIso2 { get; set; }
		public String PostCode { get; set; }

		public TLPostAddress() { }
		public TLPostAddress(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.PostAddress; } }

		public override void Read(TLBinaryReader from)
		{
			StreetLine1 = from.ReadString();
			StreetLine2 = from.ReadString();
			City = from.ReadString();
			State = from.ReadString();
			CountryIso2 = from.ReadString();
			PostCode = from.ReadString();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteString(StreetLine1 ?? string.Empty);
			to.WriteString(StreetLine2 ?? string.Empty);
			to.WriteString(City ?? string.Empty);
			to.WriteString(State ?? string.Empty);
			to.WriteString(CountryIso2 ?? string.Empty);
			to.WriteString(PostCode ?? string.Empty);
		}
	}
}