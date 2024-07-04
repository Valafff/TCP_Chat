using ProtoBuf;
using Server.BLL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.BLL.Services
{
	static public class CourierServices
	{
		static public byte[] Packer(Courier courier)
		{
			try
			{
				BinaryFormatter formatter = new BinaryFormatter();
				using (MemoryStream stream = new MemoryStream())//переводим объект в байты
				{
					formatter = new BinaryFormatter();
					formatter.Serialize(stream, courier);
					byte[] data = stream.ToArray();
					return data;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		static public byte[] Packer(string _command)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (MemoryStream stream = new MemoryStream())//переводим объект в байты
			{
				Courier courier = new Courier();
				courier.Header = _command;
				formatter = new BinaryFormatter();
				formatter.Serialize(stream, courier);
				byte[] data = stream.ToArray();
				return data;
			}
		}

		static public byte[] Packer(BLLMessageModel _message, string _command, Dictionary<string, string> _namesAndPaths = null)
		{
			try
			{
				Courier courier = new Courier() { Attachments = new Dictionary<string, byte[]>() };
				courier.Header = _command;
				if (_message.UserSender != null) courier.SenderLogin = _message.UserSender.Login;
				if (_message.UserReciver != null) courier.ReciverLogin = _message.UserReciver.Login;
				courier.MessageText = _message.MessageText;
				courier.Date = _message.Date;
				courier.IsDelivered = _message.IsDelivered;
				courier.IsRead = _message.IsRead;
				if (_namesAndPaths != null && _namesAndPaths.Count > 0)
				{
					foreach (var item in _namesAndPaths)
					{
						FileInfo file = new FileInfo(item.Value);
						byte[] buffer = new byte[file.Length];
						using (FileStream fs = new FileStream(item.Value, FileMode.Open))
						{
							fs.Read(buffer, 0, buffer.Length);
						}
						//Помещаем имя файла в key сущность в value
						courier.Attachments.Add(item.Key, buffer);
					}
				}

				BinaryFormatter formatter = new BinaryFormatter();
				using (MemoryStream stream = new MemoryStream())//переводим объект в байты
				{
					formatter = new BinaryFormatter();
					formatter.Serialize(stream, courier);
					byte[] data = stream.ToArray();
					return data;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
			
		}


		//Распаковщик сообщения. На входе объект для распаковки, на выходе модель BLL с путями, записанные в директорию с клиентом контент если есть, команда 
		static public BLLMessageModel Unpacker(byte[] courierByteArr, out string _command, out Dictionary<string, byte[]> _fileData)
		{
			try
			{
				_command = "NoCommand";
				BLLMessageModel messageBLL = new BLLMessageModel() { UserReciver = new BLLSlimClientModel(), UserSender = new BLLSlimClientModel(), MessageContentNames = new List<string>() };
				//Распаковываем курьера
				//Courier courier = JsonSerializer.Deserialize<Courier>(courierByteArr);

				using var ms = new MemoryStream();
				Courier courier = Serializer.Deserialize<Courier>(ms);

				_command = courier.Header;
				if (courier.SenderLogin != null) messageBLL.UserSender.Login = courier.SenderLogin;
				if (courier.ReciverLogin != null) messageBLL.UserReciver.Login = courier.ReciverLogin;
				messageBLL.MessageText = courier.MessageText;
				messageBLL.Date = courier.Date;
				messageBLL.IsRead = courier.IsRead;
				messageBLL.IsDelivered = courier.IsDelivered;
				_fileData = courier.Attachments;
				return messageBLL;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;

            }

		}

		static public List<BLLMessageModel> ListUnpacker(byte[] courierByteArr, out string _command, out List< Dictionary<string, byte[]>> _fileDataList)
		{
			try
			{
				_command = "NoCommand";
				BLLMessageModel messageBLL = new BLLMessageModel() { UserReciver = new BLLSlimClientModel(), UserSender = new BLLSlimClientModel(), MessageContentNames = new List<string>() };
				List<BLLMessageModel> messages = new List<BLLMessageModel>();
				List<Dictionary<string, byte[]>> fileDataList = new List<Dictionary<string, byte[]>>();
				//Распаковываем курьера
				Courier courier = JsonSerializer.Deserialize<Courier>(courierByteArr);

				_command = courier.Header;
				if (courier.SenderLogin != null) messageBLL.UserSender.Login = courier.SenderLogin;
				if (courier.ReciverLogin != null) messageBLL.UserReciver.Login = courier.ReciverLogin;
				if (courier.MessageText != null) messageBLL.MessageText = courier.MessageText;
				messageBLL.Date = courier.Date;
				messageBLL.IsRead = courier.IsRead;
				messageBLL.IsDelivered = courier.IsDelivered;
				//_fileData = courier.Attachments;
				
				
				
	
				_fileDataList = fileDataList;
				return messages;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}

		}
	}
}
