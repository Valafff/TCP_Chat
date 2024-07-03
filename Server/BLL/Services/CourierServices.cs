using Server.BLL.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.BLL.Services
{
	static public class CourierServices
	{
		////Упаковщик сообщения 
		//static public byte[] Packer(string _senderLogin, string _reciverLogin, string _command, string _message, List<string> _contentFileNames)
		//{
		//	Courier courier = new Courier();
		//	courier.Header = _command;
		//	courier.SenderLogin = _senderLogin;
		//	courier.ReciverLogin = _reciverLogin;
		//	courier.MessageText = _message;
		//	//if (_contentFileNames.Count > 0)
		//	//{
		//	//	DirectoryInfo ClientDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + $"\\Clients\\{_reciverLogin}");
		//	//	FileInfo[] ClientFilesNamesInDir = ClientDirectory.GetFiles("*.*");
		//	//	//Фильтруем файолы согласно имеющимся в сообщении
		//	//	FileInfo[] ActualFiles = (FileInfo[])ClientFilesNamesInDir.Where(n => _contentFileNames.Contains(n.Name));

		//	//	for (int i = 0; i < ActualFiles.Length; i++)
		//	//	{
		//	//		//Размер буфера определяется размером читаемого файла
		//	//		byte[] tempBuffer = new byte[ActualFiles[i].Length];
		//	//		using (FileStream fs = new FileStream($"\\Clients\\" + _contentFileNames[i], FileMode.Open))
		//	//		{
		//	//			//Временный контейнер для наполнения массивом байт
		//	//			fs.Read(tempBuffer, 0, tempBuffer.Length);
		//	//		}
		//	//		//в словарь помещаем имя файла и массив данных файла
		//	//		courier.Attachment.FileNameEntity.Add(ActualFiles[i].Name, tempBuffer);
		//	//	}
		//	//}
		//	return JsonSerializer.SerializeToUtf8Bytes(courier);
		//}


		static public byte[] Packer(BLLMessageModel _message, string _command, Dictionary<string, string> _namesAndPaths = null)
		{
			try
			{
				Courier courier = new Courier() { Attachments = new Dictionary<string, byte[]>() };
				courier.Header = _command;
				courier.SenderLogin = _message.UserSender.Login;
				courier.ReciverLogin = _message.UserReciver.Login;
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
				return JsonSerializer.SerializeToUtf8Bytes(courier);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
			
		}

		static public byte[] Packer(string _command)
		{
			Courier courier = new Courier();
			courier.Header = _command;
			return JsonSerializer.SerializeToUtf8Bytes(courier);
		}
		//Распаковщик сообщения. На входе объект для распаковки, на выходе модель BLL с путями, записанные в директорию с клиентом контент если есть, команда 
		static public BLLMessageModel Unpacker(byte[] courierByteArr, out string _command, out Dictionary<string, byte[]> _fileData)
		{
			try
			{
				_command = "NoCommand";
				BLLMessageModel messageBLL = new BLLMessageModel() { UserReciver = new BLLSlimClientModel(), UserSender = new BLLSlimClientModel(), MessageContentNames = new List<string>() };
				//Распаковываем курьера
				Courier courier = JsonSerializer.Deserialize<Courier>(courierByteArr);

				_command = courier.Header;
				if (courier.SenderLogin != null) messageBLL.UserSender.Login = courier.SenderLogin;
				if (courier.ReciverLogin != null) messageBLL.UserReciver.Login = courier.ReciverLogin;
				if (courier.MessageText != null) messageBLL.MessageText = courier.MessageText;
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
