using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
	public class UIClientModel
	{
		public int Id { get; set; }
		public string ChatName { get; set; }
		public string Login { get; set; }
		public int Status { get; set; }
		public DateTime LastVisit { get; set; }
        public string ResultString { get; set; }
        public bool IsActive { get; set; } = false;
        public string BackColor { get; set; }
    }
}
