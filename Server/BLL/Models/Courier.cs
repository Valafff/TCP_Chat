using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Models
{
	public class Courier
	{
        public string Header { get; set; }
        public string MessageText { get; set; }
        public Content Attachment { get; set; }
    }

    public class Content
    {
        public string FileName { get; set; }
        public byte[] Entity { get; set; }
    }
}
