using ProtoBuf;
using Server.BLL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.BLL.Services
{
	public class ServerCommands
	{
		public void HelloClient(Stream _stream, TcpClient _tcpClient)
		{
			try
			{
				Console.WriteLine($"Попытка подключения {_tcpClient.Client.RemoteEndPoint} рукопожатие OK\t{DateTime.Now}");
				var buffer = CourierServices.Packer(com.AnswerHelloUser);
				Tools.DataToBinaryWriter.WriteData(_stream, buffer);

				//_stream.Write(buffer, 0, buffer.Length);
				//stream.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void Registration(Courier _courier, ref Dictionary<int, string> _registredClients, List<ActiveClientLogin> _activeClients, TcpClient _tcpClient, Stream _stream)
		{
			try
			{
				BLL.Services.AccountService service = new BLL.Services.AccountService();
				BLLClientModel newClient = JsonSerializer.Deserialize<BLLClientModel>(_courier.MessageText);
				bool registrationStatus = ClientDirectoryCreation.AddClientDirectory(newClient.Login);
				if (registrationStatus)
					registrationStatus = service.Register(newClient, _registredClients);

				if (registrationStatus)
				{
					_registredClients = BLL.Services.SlimUsersDictionatry.GetSlimUsersIdLogin();
					
					_activeClients.Add(new ActiveClientLogin() { ActiveClient = _tcpClient, ClientStream = _stream, Login = newClient.Login });

					var buffer = CourierServices.Packer(com.AnswerRegisterOk);
					Tools.DataToBinaryWriter.WriteData(_stream, buffer);
					//_stream.Write(buffer, 0, buffer.Length);

					Console.WriteLine($"Регистрация пользователя {newClient.Login} прошла успешно\t{DateTime.Now}");
					Console.WriteLine($"Активные клиенты {_activeClients.Count}");
				}
				else
				{
					var buffer = CourierServices.Packer(com.AnswerRegisterFailed);
					Tools.DataToBinaryWriter.WriteData(_stream, buffer);


					_stream.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}

		}

		public void Authtorization(Courier _courier, Dictionary<int, string> _registredClients, List<ActiveClientLogin> _activeClients, TcpClient _tcpClient, Stream _stream)
		{
			try
			{
				BLL.Services.AccountService service = new BLL.Services.AccountService();
				KeyValuePair<string, string> AuthorizeUser = JsonSerializer.Deserialize<KeyValuePair<string, string>>(_courier.MessageText);
				bool authorizeStatus = service.Authorize(AuthorizeUser.Key, AuthorizeUser.Value);
				if (_activeClients.Any(c => c.Login == AuthorizeUser.Key) || _activeClients.Any(s => s.ClientStream == _stream)) authorizeStatus = false;

				if (authorizeStatus)
				{
					_registredClients = BLL.Services.SlimUsersDictionatry.GetSlimUsersIdLogin();
					_activeClients.Add(new ActiveClientLogin() { ActiveClient = _tcpClient, ClientStream = _stream, Login = AuthorizeUser.Key });
					BLLMessageModel nullmessage = new BLLMessageModel();

					byte[] buffer = CourierServices.Packer(com.AnswerAuthorizationOk);
					Tools.DataToBinaryWriter.WriteData(_stream, buffer);
					//_stream.Write(buffer, 0, buffer.Length);
					Console.WriteLine($"Авторизация пользователя {AuthorizeUser.Key} прошла успешно\t{DateTime.Now}");
					Console.WriteLine($"Активные клиенты {_activeClients.Count}");
				}
				else
				{
					var buffer = CourierServices.Packer(com.AnswerAuthorizationFailed);
					Tools.DataToBinaryWriter.WriteData(_stream, buffer);

					//_tcpClient.Close();
					//_stream.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("!!!");
				Console.WriteLine(ex.Message);
				throw;
			}

		}

		public void TakeRegistredUsers(Stream _stream, Dictionary<int, string> _registredClients)
		{
			try
			{
				Courier courier = new Courier();
				courier.Header = com.AnswerCatchUsers;
				courier.MessageText = JsonSerializer.Serialize(_registredClients.Values);
				var buffer = Services.CourierServices.Packer(courier);
				Tools.DataToBinaryWriter.WriteData(_stream, buffer);
				//_stream.Write(buffer, 0, buffer.Length);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		public void TakeActiveClients(Stream _stream, List<ActiveClientLogin> _activeClients)
		{
			try
			{
				List<string> ActivClientsLogins = new List<string>();
				foreach (var item in _activeClients)
				{
					ActivClientsLogins.Add(item.Login);
				}
				Courier courier = new Courier();
				courier.Header = com.AnswerCatchActiveUsers;
				courier.MessageText = JsonSerializer.Serialize(ActivClientsLogins);
				var buffer = CourierServices.Packer(courier);
				Tools.DataToBinaryWriter.WriteData(_stream, buffer);
				//_stream.Write(buffer, 0, buffer.Length);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}

		}

		public void SendNewMessage(Courier _courier, Dictionary<int, string> _registredClients, Stream _stream)
		{
			try
			{
				Courier courier = _courier;
				BLLMessageModel incomeMessage = new BLLMessageModel() { UserReciver = new BLLSlimClientModel(), UserSender = new BLLSlimClientModel(), MessageContentNames = new List<string>() };

				if (courier.SenderLogin != null) incomeMessage.UserSender.Login = courier.SenderLogin;
				if (courier.ReciverLogin != null) incomeMessage.UserReciver.Login = courier.ReciverLogin;
				incomeMessage.MessageText = courier.MessageText;
				incomeMessage.Date = courier.Date;
				incomeMessage.IsRead = courier.IsRead;
				incomeMessage.IsDelivered = courier.IsDelivered;
				if (courier.Attachments != null) incomeMessage.MessageContentNames = courier.Attachments.Keys.ToList();

				//Запись данных в БД
				MessageService service = new MessageService();
				service.InsertMessage(incomeMessage, _registredClients);
				if (incomeMessage.MessageContentNames != null && incomeMessage.MessageContentNames.Count > 0)
				{
					string directoryPath = Directory.GetCurrentDirectory() + $"\\Clients\\{incomeMessage.UserReciver.Login}\\";
					DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
					if (!directoryInfo.Exists)
					{
						directoryInfo.Create();
					}
					foreach (var file in courier.Attachments)
					{
						using (FileStream fs = new FileStream(directoryPath + file.Key, FileMode.OpenOrCreate))
						{
							fs.Write(file.Value);
						}
					}
				}
				//TODO Если клиент активен - передавать на прямую


				byte[] buffer = CourierServices.Packer(com.AnswerMessageSendOk);
				Tools.DataToBinaryWriter.WriteData(_stream, buffer);
				//_stream.Write(buffer, 0, buffer.Length);
			}
			catch (Exception ex)
			{
				byte[] buffer = CourierServices.Packer(com.AnswerMessageSendFailed);
				Tools.DataToBinaryWriter.WriteData(_stream, buffer);
				//_stream.Write(buffer, 0, buffer.Length);
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		public void SendUnreadMessagesForClient(Dictionary<int, string> _registredClients, List<ActiveClientLogin> _activeClients, TcpClient _tcpClient, Stream _stream)
		{
			try
			{

				string userReciverLogin = (_activeClients.First(c => c.ActiveClient == _tcpClient)).Login;
				int userId = (_registredClients.First(l => l.Value == userReciverLogin)).Key;
				MessageService service = new MessageService();
				//Получен список непрочитанных сообщений с ссылками на вложения
				List<BLLMessageModel> unreadMessages = service.GetAllUnReadMessages(userId, _registredClients);

				Courier courier = new Courier();
				courier.Header = com.AnswerCatchMessages;
				courier.MessageText = JsonSerializer.Serialize(unreadMessages);
				var buffer = Services.CourierServices.Packer(courier);
				Tools.DataToBinaryWriter.WriteData(_stream, buffer);

			}
			catch (Exception ex)
			{
				Console.WriteLine("Не прочитаны сообщения для клиента");
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		public void CheckDeliveredMesseges(Courier courier, Dictionary<int, string> _slimClients)
		{
			List<BLLMessageModel> readedMessages = JsonSerializer.Deserialize<List<BLLMessageModel>>(courier.MessageText);
			MessageService service = new MessageService();
			foreach (var item in readedMessages)
			{
				item.IsDelivered = 1;
				item.IsRead = 1;
				service.UpdateMessage(item, _slimClients);
			}
		}

		public void AttachmentsSend(Stream _stream, Courier _courier, Dictionary<int, string> _registredClients)
		{
			try
			{
				string[] names = JsonSerializer.Deserialize<string[]>(_courier.MessageText);
				string senderLogin = names[0];
				string reciverLogin = names[1];		
				names = names[2..names.Length];

				MessageService service = new MessageService();
				List<BLLMessageModel> messages = service.GetAllMessagesBySenderReciver(_registredClients, senderLogin, reciverLogin);

				Dictionary<string,string> namesAndPaths = new Dictionary<string,string>();
				foreach (var item in names)
				{
					namesAndPaths.Add(key: item, value: Directory.GetCurrentDirectory() + $"\\Clients\\{reciverLogin}\\{item}");
				}

				//Сравнение с учетом порядка элементов bool Equal = list1.SequenceEquals(list2);
				//Сравнение без учета порядка элементов bool Equal = new HashSet<string>(list1).SetEquals(list2);
				BLLMessageModel targetMessege = messages.First(m => new HashSet<string>(m.MessageContentNames).SetEquals(names.ToList()));
				var buffer = CourierServices.Packer(targetMessege, com.AnswerCatchAttachments, namesAndPaths);
				Tools.DataToBinaryWriter.WriteData(_stream, buffer);
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex.Message);
                Console.WriteLine("Ошибка отправления данных вложений");
                throw;
			}




		}
	}

}
