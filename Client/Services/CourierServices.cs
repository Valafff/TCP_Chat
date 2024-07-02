using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Client.ViewModels;

namespace Client.Services
{
	static public class CourierServices
	{
		//Упаковщик сообщения 
		static public byte[] Packer(string _senderLogin, string _reciverLogin, string _command, string _message, List<string> _contentFileNames)
		{
			Courier courier = new Courier();
			courier.Header = _command;
			courier.SenderLogin = _senderLogin;
			courier.ReciverLogin = _reciverLogin;
			courier.MessageText = _message;
			if (_contentFileNames.Count > 0)
			{
				DirectoryInfo ClientDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + $"\\Clients\\{_reciverLogin}");
				FileInfo[] ClientFilesNamesInDir = ClientDirectory.GetFiles("*.*");
				//Фильтруем файолы согласно имеющимся в сообщении
				FileInfo[] ActualFiles = (FileInfo[])ClientFilesNamesInDir.Where(n => _contentFileNames.Contains(n.Name));

				for (int i = 0; i < ActualFiles.Length; i++)
				{
					//Размер буфера определяется размером читаемого файла
					byte[] tempBuffer = new byte[ActualFiles[i].Length];
					using (FileStream fs = new FileStream($"\\Clients\\" + _contentFileNames[i], FileMode.Open))
					{
						//Временный контейнер для наполнения массивом байт
						fs.Read(tempBuffer, 0, tempBuffer.Length);
					}
					//в словарь помещаем имя файла и массив данных файла
					courier.Attachment.FileNameEntity.Add(ActualFiles[i].Name, tempBuffer);
				}
			}
			return JsonSerializer.SerializeToUtf8Bytes(courier);
		}


		static public byte[] Packer(BLLMessageModel _message, string _command)
		{
			Courier courier = new Courier();
			courier.Header = _command;
			courier.SenderLogin = _message.UserSender.Login;
			courier.ReciverLogin = _message.UserReciver.Login;
			courier.MessageText = _message.MessageText;
			courier.Date = _message.Date;
			courier.IsDelivered = _message.IsDelivered;
			courier.IsRead = _message.IsRead;
			if (_message.MessageContentNames.Count > 0)
			{
				DirectoryInfo ClientDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + $"\\Clients\\{_message.UserReciver.Login}");
				FileInfo[] ClientFilesNamesInDir = ClientDirectory.GetFiles("*.*");
				//Фильтруем файолы согласно имеющимся в сообщении
				FileInfo[] ActualFiles = (FileInfo[])ClientFilesNamesInDir.Where(n => _message.MessageContentNames.Contains(n.Name));

				for (int i = 0; i < ActualFiles.Length; i++)
				{
					//Размер буфера определяется размером читаемого файла
					byte[] tempBuffer = new byte[ActualFiles[i].Length];
					using (FileStream fs = new FileStream($"\\Clients\\" + _message.MessageContentNames[i], FileMode.Open))
					{
						//Временный контейнер для наполнения массивом байт
						fs.Read(tempBuffer, 0, tempBuffer.Length);
					}
					//в словарь помещаем имя файла и массив данных файла
					courier.Attachment.FileNameEntity.Add(ActualFiles[i].Name, tempBuffer);
				}
			}
			return JsonSerializer.SerializeToUtf8Bytes(courier);
		}

		static public byte[] Packer(string _command)
		{
			Courier courier = new Courier();
			courier.Header = _command;
			return JsonSerializer.SerializeToUtf8Bytes(courier);
		}

		//Распаковщик сообщения. На входе объект для распаковки, на выходе модель BLL с путями, записанные в директорию с клиентом контент если есть, команда 
		static public BLLMessageModel Unpacker(byte[] courierByteArr, out string _command)
		{
			_command = "NoCommand";
			BLLMessageModel messageBLL = new BLLMessageModel() { UserReciver = new BLLSlimClientModel(), UserSender = new BLLSlimClientModel(), MessageContentNames = new List<string>() };
			//Распаковываем курьера
			Courier courier = JsonSerializer.Deserialize<Courier>(courierByteArr);

			//Пишем пришедшие файлы в директорию с именем клиента ФАЙЛЫ С ОДИНАКОВЫМ НАЗВАНИЕМ ПЕРЕЗАПИСЫВАЮТСЯ!
			_command = courier.Header;
			if (courier.SenderLogin != null) messageBLL.UserSender.Login = courier.SenderLogin;
			if (courier.ReciverLogin != null) messageBLL.UserReciver.Login = courier.ReciverLogin;
			if (courier.MessageText != null) messageBLL.MessageText = courier.MessageText;
			messageBLL.Date = courier.Date;
			messageBLL.IsRead = courier.IsRead;
			messageBLL.IsDelivered = courier.IsDelivered;
			if (courier.Attachment != null)
			{
				foreach (var item in courier.Attachment.FileNameEntity)
				{
					messageBLL.MessageContentNames.Add(item.Key);
					using (FileStream fs = new FileStream($"\\Clients\\{courier.ReciverLogin}\\{item.Key}", FileMode.Create))
					{
						fs.Write(item.Value, 0, item.Value.Length);
					}
				}
				return messageBLL;
			}
			else return messageBLL;
		}
	}
}
