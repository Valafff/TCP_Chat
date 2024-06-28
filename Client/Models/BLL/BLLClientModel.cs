using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
	public class BLLClientModel
	{
		public int Id { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string FirstName { get; set; }
		public string SecondName { get; set; }
		public int Status { get; set; }
		public DateTime LastVisit { get; set; }
	}
}
