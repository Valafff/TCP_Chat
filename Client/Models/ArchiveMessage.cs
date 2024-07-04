using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
	public class ArchiveMessage
	{
        public DateTime Date { get; set; }
        public string Sender { get; set; }
        public string Reciver { get; set; }
        public string TextMessage { get; set; }
        public List<string> FilesNames{ get; set; }
    }
}
