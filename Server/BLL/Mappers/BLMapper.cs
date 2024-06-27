using Server.BLL.Models;
using Server.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.BLL.Mappers
{
	public static class BLMapper
	{
		//Клиент из DAL в BLL авторизации
		public static BLLClientModel MapClientDALToClientBLL(DAL.Models.DALClientModel _clientDAL)
		{
			return new BLLClientModel()
			{
				Id = _clientDAL.Id,
				Login = _clientDAL.Login,
				Password = _clientDAL.Password,
				FirstName = _clientDAL.FirstName,
				SecondName = _clientDAL.SecondName,
				Status = _clientDAL.Status,
				LastVisit = DateTime.Parse(_clientDAL.LastVisit)
			};
		}

		//Урезанный Клиент из DAL в BLL для подгрузки в Dictionary<int, string> (Id, Login)
		public static BLLSlimClientModel SlimMapClientDALToClientBLL(DAL.Models.DALClientModel _clientDAL)
		{
			return new BLLSlimClientModel()
			{
				Id = _clientDAL.Id,
				Login = _clientDAL.Login
			};
		}

		//Урезанный Клиент из DAL в BLL для идентификации в мапере
		public static BLLSlimClientModel SlimMapClientDALToClientBLL(int _clientID, string _clientLogin)
		{
			return new BLLSlimClientModel()
			{
				Id = _clientID,
				Login = _clientLogin
			};
		}

		//Клиент из BLL в DAL для регистрации или удаления
		public static DAL.Models.DALClientModel MapClientBLLToClientDAL(BLLClientModel _clientBLL)
		{
			return new DAL.Models.DALClientModel()
			{
				Id = _clientBLL.Id,
				Login = _clientBLL.Login,
				Password = _clientBLL.Password,
				FirstName = _clientBLL.FirstName,
				SecondName = _clientBLL.SecondName,
				Status = _clientBLL.Status,
				LastVisit = _clientBLL.LastVisit.ToString()
			};
		}

		//Мапирование сообщения в BBL через загруженный словарь с BLLSlimClientModel
		public static BLLMessageModel MapMesDALToMesBLL(DAL.Models.DALMessageModel _mesDAL, Dictionary<int, string> _slimClients )
		{
			return new BLLMessageModel()
			{
				Id = _mesDAL.Id,
				//Если в DAL модели сообщений FromUserID совпадает со значением в словаре то UserSender получает значения из словаря
				UserSender = SlimMapClientDALToClientBLL(_slimClients.Keys.FirstOrDefault(_mesDAL.FromUserID), _slimClients.GetValueOrDefault(_mesDAL.FromUserID)),
				UserReciver = SlimMapClientDALToClientBLL(_slimClients.Keys.FirstOrDefault(_mesDAL.ToUserID), _slimClients.GetValueOrDefault(_mesDAL.ToUserID)),
				Date = DateTime.Parse(_mesDAL.Date),
				MessageText = _mesDAL.MessageText,
				MessageContentNames = JsonSerializer.Deserialize<List<string>>(_mesDAL.MessageContent),
				IsRead = _mesDAL.IsRead,
				IsDelivered = _mesDAL.IsDelivered
			};
		}

		//Мапирование сообщения в DAL через загруженный словарь с BLLSlimClientModel
		public static DALMessageModel MapMesBLLLToMesDAL(BLLMessageModel _mesBLL, Dictionary<int, string> _slimClients)
		{
			return new DALMessageModel()
			{
				Id = _mesBLL.Id,
				FromUserID = _mesBLL.UserSender.Id,
				ToUserID = _mesBLL.UserReciver.Id,
				Date = _mesBLL.Date.ToString(),
				MessageText = _mesBLL.MessageText,
				MessageContent = JsonSerializer.Serialize(_mesBLL.MessageContentNames),
				IsRead =_mesBLL.IsRead,
				IsDelivered = _mesBLL.IsDelivered
			};
		}
	}
}
