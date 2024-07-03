using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Models
{
	public class ActiveClientLogin
	{
        public TcpClient ActiveClient { get; set; }
		public Stream ClientStream { get; set; }
		public string? Login { get; set; }
    }
}
