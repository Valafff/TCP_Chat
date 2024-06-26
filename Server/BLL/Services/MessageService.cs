using Server.BLL.Mappers;
using Server.BLL.Models;
using Server.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Services
{
	internal class MessageService
	{
		DAL.Services.SQLLiteServiceMasseges service = new DAL.Services.SQLLiteServiceMasseges();

		//Извлечь все непрочитанные сообщения для UserReciver из БД и отправить их UserReciver (для подключившегося клиента).
		public List<BLLMessageModel> GetAllUnreadMessages(int _clientId, Dictionary<int, string> _slimClients)
		{			
			List<BLLMessageModel> messages = new List<BLLMessageModel>();
			IEnumerable<DALMessageModel> DALMessages = service.GetAllMessegesReciverID(_clientId);
			foreach (DALMessageModel message in DALMessages)
			{
				messages.Add(BLMapper.MapMesDALToMesBLL(message, _slimClients));
			}	
			return messages;
		}

		//Написать сообщение - Подключенный клиент отправляет сообщение
		public bool WriteMessage(Courier _newmessage)
		{
			//Под вопросом разложить пакет на 
			return true;
		}
	}
}
