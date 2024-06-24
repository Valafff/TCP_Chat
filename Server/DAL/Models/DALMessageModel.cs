using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DAL.Models
{
    internal class DALMessageModel
    {
        public int Id { get; set; }
        public int FromUserID { get; set; } //Id пользователя, который писал это сообщение
        public int ToUserID { get; set; }   //Id пользователя, которому адресовано это сообщение
        public string Date { get; set; }
        public string MessageText { get; set; }
        public string MessageContent { get; set; }
		public int IsRead { get; set; }
		public int IsDelivered { get; set; }


    }
}
