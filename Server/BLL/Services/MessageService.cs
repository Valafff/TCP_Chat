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
	public  class MessageService
	{
		const int UNREAD = 0;
		DAL.Services.SQLLiteServiceMasseges service = new DAL.Services.SQLLiteServiceMasseges();

		//Извлечь все непрочитанные сообщения для UserReciver из БД и отправить их UserReciver (для подключившегося клиента).
		public List<BLLMessageModel> GetAllUnReadMessages(int _clientId, Dictionary<int, string> _slimClients)
		{			
			List<BLLMessageModel> messages = new List<BLLMessageModel>();
			IEnumerable<DALMessageModel> DALMessages = service.GetAllMessegesReciverID(_clientId);
			foreach (DALMessageModel message in DALMessages)
			{
				if (message.IsDelivered == UNREAD)
				{
					messages.Add(BLMapper.MapMesDALToMesBLL(message, _slimClients));
				}
			}	
			return messages;
		}

		public List<BLLMessageModel> GetAllMessagesBySenderReciver(Dictionary<int, string> _slimClients, string _senderLogin, string _reciverLogin)
		{
			int senderId = (_slimClients.First(v => v.Value == _senderLogin)).Key;
			int reciverId = (_slimClients.First(v => v.Value == _reciverLogin)).Key;

			List<BLLMessageModel> messages = new List<BLLMessageModel>();
			IEnumerable<DALMessageModel> DALMessages = service.GetAllMessegesReciverID(reciverId);

			foreach (DALMessageModel message in DALMessages)
			{
				if (message.ToUserID == reciverId && message.FromUserID == senderId)
				{
					messages.Add(BLMapper.MapMesDALToMesBLL(message, _slimClients));
				}
			}
			return messages;
		}

		public void UpdateMessage(BLLMessageModel _model, Dictionary<int, string> _slimClients)
		{
			service.UpdateMessage(Mappers.BLMapper.MapMesBLLLToMesDAL(_model, _slimClients));
		}

		public void InsertMessage(BLLMessageModel _model, Dictionary<int, string> _slimClients)
		{
			service.InsertMessage(Mappers.BLMapper.MapMesBLLLToMesDAL(_model, _slimClients));
		}
	}
}
