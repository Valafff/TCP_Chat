using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DAL
{
	internal class MessageFilter_UNUSED
	{
		public int IdFromUser { get; set; }
		public int IdToUser { get; set; } 
		public DateTime MessageDateFrom { get; set; } //диапазон времени по которому будут выдергиваться сообщения
		public DateTime MessageDateTo { get; set; }
		public bool IsDelivered { get; set; }
		public bool IsRead { get; set; }
	}
}
