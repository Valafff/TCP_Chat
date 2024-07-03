using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Models
{
	[Serializable]
	[ProtoContract]
	public class Courier
	{
		[ProtoMember(1)]
		public string Header { get; set; }
		[ProtoMember(2)]
		public string SenderLogin { get; set; }
		[ProtoMember(3)]
		public string ReciverLogin { get; set; }
		[ProtoMember(4)]
		public string MessageText { get; set; }
		[ProtoMember(5)]
		public DateTime Date { get; set; }
		[ProtoMember(6)]
		public int IsRead { get; set; }
		[ProtoMember(7)]
		public int IsDelivered { get; set; }
		[ProtoMember(8)]
		public Dictionary<string, byte[]> Attachments { get; set; }
		[ProtoMember(9)]
        public byte Entity { get; set; }
    }

	[Serializable]
	[ProtoContract]
	public class Content
	{
		[ProtoMember(1)]
		public string ServiceText { get; set; }
		[ProtoMember(2)]
		public byte[] Entity { get; set; }
	}
}
