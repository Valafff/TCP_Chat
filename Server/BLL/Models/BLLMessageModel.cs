using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Models
{
	public class BLLMessageModel
	{
		public int Id { get; set; }
		public BLLSlimClientModel UserSender { get; set; } // отправитель сообщения
		public BLLSlimClientModel UserReciver { get; set; } // кому отправлено сообщения
		public DateTime Date { get; set; }
		public string MessageText { get; set; } // текст сообщения
		public List<string> MessageContentPaths { get; set; } // Пути к файлам
		public int IsRead { get; set; }
		public int IsDelivered { get; set; }
	}
}
