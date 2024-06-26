using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Models
{
	public class SlimUsersDictionatry
	{
		public Dictionary<int, string> GetSlimUsersIdLogin()
		{
			DAL.Services.SQLLiteServiceUsers service = new DAL.Services.SQLLiteServiceUsers();
			Dictionary<int, string> usersDict = new Dictionary<int, string>();
			var DALUsers = service.GetAllRegistredUsers();		
			foreach (var DALuser in DALUsers)
			{
				BLLSlimClientModel user = Mappers.BLMapper.SlimMapClientDALToClientBLL(DALuser);
				usersDict.Add(user.Id, user.Login);
			}
			return usersDict;
		}
	}
}
