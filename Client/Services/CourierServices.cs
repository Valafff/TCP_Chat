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
			BLLMessageModel messageBLL = new BLLMessageModel();
			//Распаковываем курьера
			Courier courier = JsonSerializer.Deserialize<Courier>(courierByteArr);

			//Пишем пришедшие файлы в директорию с именем клиента ФАЙЛЫ С ОДИНАКОВЫМ НАЗВАНИЕМ ПЕРЕЗАПИСЫВАЮТСЯ!
			_command = courier.Header;
			if (courier.MessageText != null)  messageBLL.MessageText = courier.MessageText;
			
			if (courier.Attachment != null)
			{
				messageBLL.UserSender.Login = courier.SenderLogin;
				messageBLL.UserReciver.Login = courier.ReciverLogin;				
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
