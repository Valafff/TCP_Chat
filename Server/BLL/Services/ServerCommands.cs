using ProtoBuf;
using Server.BLL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.BLL.Services
{
	public class ServerCommands
	{
		//Система команд
		const string CommandHelloMsr = "HelloMsr";
		const string AnswerHelloUser = "HelloClientGoStep2";
		const string CommandRegisterMe = "RegisterMe";
		const string AnswerRegisterOk = "RegisterOkGoStep3";
		const string AnswerRegisterFailed = "RegisterFailed";
		const string CommandAuthorizeMe = "AuthorizeMe";
		const string AnswerAuthorizationOk = "AuthorizationOkGoStep3";
		const string AnswerAuthorizationFailed = "AuthorizationFailed";
		const string CommandDeleteMe = "DeleteMe";
		const string AnswerDeleteOk = "DeleteOkGoStep1";
		const string AnswerDeleteFailed = "DeleteFailed";
		const string CommandGetMeUsers = "GetMeUsers";
		const string CommandGetMeActiveUsers = "GetMeActiveUsers";
		const string AnswerCatchUsers = "CatchUsers";
		const string AnswerCatchActiveUsers = "CatchActiveUsers";
		const string CommandGiveMeUnReadMes = "GiveMeUnReadMes";
		const string AnswerCatchMessages = "CatchMessages";
		const string CommandMessageTo = "MessageTo"; //Команда серверу - отправь сообщение такому то пользователю
		const string AnswerMessageSendOk = "MessageSendOK";
		const string AnswerMessageSendFailed = "MessageSendFailed";
		const string CommandTakeMessage = "TakeMessage"; //Команда клиенту - прими сообщение от такого то пользователя

		public void HelloClient(Stream _stream, TcpClient _tcpClient)
		{
			try
			{
				Console.WriteLine($"Попытка подключения {_tcpClient.Client.RemoteEndPoint} рукопожатие OK\t{DateTime.Now}");
				Courier courier = new Courier();
				courier.Header = AnswerHelloUser;
				using var ms = new MemoryStream();
				Serializer.Serialize(ms, courier);
				var buffer = ms.ToArray();
				_stream.Write(buffer, 0, buffer.Length);
				//stream.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void Registration(Courier _courier, Dictionary<int, string> _registredClients, List<ActiveClientLogin> _activeClients, TcpClient _tcpClient, Stream _stream)
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

					Courier courier = new Courier() { Header = AnswerRegisterOk };
					using var ms = new MemoryStream();
					Serializer.Serialize(ms, courier);
					var buffer = ms.ToArray();
					_stream.Write(buffer, 0, buffer.Length);

					Console.WriteLine($"Регистрация пользователя {newClient.Login} прошла успешно\t{DateTime.Now}");
					Console.WriteLine($"Активные клиенты {_activeClients.Count}");
				}
				else
				{
					Courier courier = new Courier() { Header = AnswerRegisterFailed };
					using var ms = new MemoryStream();
					Serializer.Serialize(ms, courier);
					var buffer = ms.ToArray();
					_stream.Write(buffer, 0, buffer.Length);
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
				if (_activeClients.Any(c => c.Login == AuthorizeUser.Key)) authorizeStatus = false;

				if (authorizeStatus)
				{
					_registredClients = BLL.Services.SlimUsersDictionatry.GetSlimUsersIdLogin();
					_activeClients.Add(new ActiveClientLogin() { ActiveClient = _tcpClient, ClientStream = _stream, Login = AuthorizeUser.Key });
					BLLMessageModel nullmessage = new BLLMessageModel();
					byte[] buffer = CourierServices.Packer(nullmessage, AnswerAuthorizationOk);

					_stream.Write(buffer, 0, buffer.Length);
					Console.WriteLine($"Авторизация пользователя {AuthorizeUser.Key} прошла успешно\t{DateTime.Now}");
					Console.WriteLine($"Активные клиенты {_activeClients.Count}");
				}
				else
				{
					Courier courier = new Courier() { Header = AnswerAuthorizationFailed };
					using var ms = new MemoryStream();
					Serializer.Serialize(ms, courier);
					var buffer = ms.ToArray();
					_stream.Write(buffer, 0, buffer.Length);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}

		}

		public void TakeRegistredUsers(Stream _stream, Dictionary<int, string> _registredClients)
		{
			try
			{
				Courier courier = new Courier();
				courier.Header = AnswerCatchUsers;
				courier.MessageText = JsonSerializer.Serialize(_registredClients.Values);

				using var ms = new MemoryStream();
				Serializer.Serialize(ms, courier);
				var buffer = ms.ToArray();
				_stream.Write(buffer, 0, buffer.Length);
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
				courier.Header = AnswerCatchActiveUsers;
				courier.MessageText = JsonSerializer.Serialize(ActivClientsLogins);

				using var ms = new MemoryStream();
				Serializer.Serialize(ms, courier);
				var buffer = ms.ToArray();
				_stream.Write(buffer, 0, buffer.Length);
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
						using (FileStream fs = new FileStream(directoryPath + file.Key, FileMode.Create))
						{
							fs.Write(file.Value);
						}
					}
				}
				//TODO Если клиент активен - передавать на прямую

				courier = new Courier();
				courier.Header = AnswerMessageSendOk;
				byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(courier);
				_stream.Write(buffer, 0, buffer.Length);
			}
			catch (Exception ex)
			{
				Courier courier = new Courier();
				courier.Header = AnswerMessageSendFailed;
				byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(courier);
				_stream.Write(buffer, 0, buffer.Length);
				Console.WriteLine(ex.Message);
				throw;
			}
		}


	}
}
