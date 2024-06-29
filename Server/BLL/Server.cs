﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using Server.Tools;
using Server.BLL.Models;
using System.Text.Json;
using Server.BLL.Services;
using System.Reflection;

namespace Server.BLL
{
	public class Server
	{
		static int Port = 8888;
		const int delay = 1000;
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
		const string CommandTakeMessage = "TakeMessage"; //Команда клиенту - прими сообщение от такого то пользователя

		//Id зарегистрированных клиентов и их логины
		Dictionary<int, string> RegistredClients = new Dictionary<int, string>();
		//Связь активных клиентов и их логинов
		List<ActiveClientLogin> ActiveClients = new List<ActiveClientLogin>();

		TcpListener tcpListener;

		public Server(int _port)
		{
			Port = _port;
			tcpListener = new TcpListener(IPAddress.Any, Port); // сервер для прослушивания
			RegistredClients = BLL.Services.SlimUsersDictionatry.GetSlimUsersIdLogin();
		}


		public async void StartServer()
		{
			bool itsWork = false;
			try
			{
				//Запускаем сервер на прослушивание
				tcpListener.Start();
				Console.WriteLine("Сервер запущен");

				while (true)
				{
					//Ждем входящее подключение
					TcpClient tcpClient = tcpListener.AcceptTcpClient();
					//Создаем новое подключение для клиента в отдельном потоке
					_ = Task.Factory.StartNew(() => ProcessClientAsync(tcpClient), TaskCreationOptions.LongRunning);
					//Проверка активных подключений
					if (!itsWork)
					{
						_ = Task.Factory.StartNew(() => CheckActiveClients(ActiveClients), TaskCreationOptions.LongRunning);
						itsWork = true;
					}

				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine("Ошибка сервера");
			}
			finally
			{
				tcpListener.Stop();
			}
		}

		async Task ProcessClientAsync(TcpClient tcpClient)
		{
			NetworkStream stream = tcpClient.GetStream();

			bool AuthtorizationStatusOK = false;
			bool WorkStatusOK = false;
            while (true)
			{
				string inputcommand="";
				byte[] workLeveldata = null;
				//Расшифровка сообщений до авторизации
				if (!WorkStatusOK )
				{
					inputcommand =  ByteToString.GetStringFromStream(stream);
					
				}
				//Расшифровка сообщений после авторизации
				else
				{
					workLeveldata = StreamToByte.StreamToByteArr(stream);
				}

				////Отладка
				//await Console.Out.WriteLineAsync(inputcommand);

				//Контрольное слово для отсеивания тех, кого не звали
				if (inputcommand.ToString() == CommandHelloMsr && !AuthtorizationStatusOK)
				{
					try
					{

						Console.WriteLine($"Попытка подключения {tcpClient.Client.RemoteEndPoint} одобрена\t{DateTime.Now}");
						byte[] buffer = Encoding.UTF8.GetBytes(AnswerHelloUser);
						stream.Write(buffer, 0, buffer.Length);
						//stream.FlushAsync();
						AuthtorizationStatusOK = true;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}

				}
				//Логика отправки, получения, запрос клиентов, запрос активных клиентов
				else if (WorkStatusOK)
				{
					BLLMessageModel IncomeMessage = new BLLMessageModel();
					string command = "NoCommand";
					try
					{
						BLL.Services.AccountService service = new BLL.Services.AccountService();
						IncomeMessage = CourierServices.Unpacker(workLeveldata, out command);
					}
					catch (Exception ex)
					{
						await Console.Out.WriteLineAsync(ex.Message);
						await Console.Out.WriteLineAsync("Ошибка Message-сервиса: некорректная десериализация сообщения");
						AuthtorizationStatusOK = false;
					}

					//Запрос на отправку активных клиентов для текущего соединения без текущего клиента
					if (command == CommandGetMeActiveUsers)
					{
						PushActiveClients();
					}

					//Запрос на отправку зарегистрированнх клиентов с текущим клиентом
					if (command == CommandGetMeUsers)
					{
						PushRegistredClients();
					}

					//Запрос на отправку непрочитанных сообщений 
					if (command == CommandGiveMeUnReadMes)
					{
						PushUnreadMessages();
					}
					//Запрос на отправку сообщения другому пользователю
					if (command == CommandMessageTo)
					{
						SendNewMessage(IncomeMessage);
					}
				}
				//Блок регистрации авторизации удаления аккаунта
				else if (AuthtorizationStatusOK)
				{
					Content AccountWorks = new Content();
					BLL.Services.AccountService service = new BLL.Services.AccountService();
					try
					{
						AccountWorks = JsonSerializer.Deserialize<Content>(inputcommand);
					}
					catch (Exception ex)
					{
						await Console.Out.WriteLineAsync(ex.Message);
						await Console.Out.WriteLineAsync("Ошибка Аккаунт-сервиса: некорректная десериализация пользователя");
						AuthtorizationStatusOK = false;
					}
					//РЕГИСТРАЦИЯ
					if (AccountWorks.ServiceText == CommandRegisterMe)
					{
						BLLClientModel newClient = JsonSerializer.Deserialize<BLLClientModel>(Encoding.UTF8.GetString(AccountWorks.Entity));
						bool registrationStatus = ClientDirectoryCreation.AddClientDirectory(newClient.Login);
						if (registrationStatus)
							registrationStatus = service.Register(newClient, RegistredClients);

						if (registrationStatus)
						{
							byte[] buffer = Encoding.UTF8.GetBytes(AnswerRegisterOk);
							await stream.WriteAsync(buffer, 0, buffer.Length);
							await stream.FlushAsync();
							ActiveClients.Add(new ActiveClientLogin() { ActiveClient = tcpClient, Login = newClient.Login });
							WorkStatusOK = true;
							await Console.Out.WriteLineAsync($"Регистрация пользователя {newClient.Login} прошла успешно\t{DateTime.Now}");
							await Console.Out.WriteLineAsync($"Активные клиенты {ActiveClients.Count}");
						}
						else
						{
							byte[] buffer = Encoding.UTF8.GetBytes(AnswerRegisterFailed);
							await stream.WriteAsync(buffer, 0, buffer.Length);
							await stream.FlushAsync();
						}
					}
					//АВТОРИЗАЦИЯ
					if (AccountWorks.ServiceText == CommandAuthorizeMe)
					{
						KeyValuePair<string, string> AuthorizeUser = JsonSerializer.Deserialize<KeyValuePair<string, string>>(Encoding.UTF8.GetString(AccountWorks.Entity));
						bool authorizeStatus = service.Authorize(AuthorizeUser.Key, AuthorizeUser.Value);
						if (authorizeStatus)
						{
							byte[] buffer = Encoding.UTF8.GetBytes(AnswerAuthorizationOk);
							await stream.WriteAsync(buffer, 0, buffer.Length);
							await stream.FlushAsync();
							ActiveClients.Add(new ActiveClientLogin() { ActiveClient = tcpClient, Login = AuthorizeUser.Key });
							WorkStatusOK = true;
							await Console.Out.WriteLineAsync($"Авторизация пользователя {AuthorizeUser.Key} прошла успешно\t{DateTime.Now}");
							await Console.Out.WriteLineAsync($"Активные клиенты {ActiveClients.Count}");
						}
						else
						{
							byte[] buffer = Encoding.UTF8.GetBytes(AnswerAuthorizationFailed);
							await stream.WriteAsync(buffer, 0, buffer.Length);
							await stream.FlushAsync();
						}
					}
					//УДАЛЕНИЕ КЛИЕНТА
					if (AccountWorks.ServiceText == CommandDeleteMe)
					{
						KeyValuePair<string, string> DeletingUser = JsonSerializer.Deserialize<KeyValuePair<string, string>>(Encoding.UTF8.GetString(AccountWorks.Entity));
						bool deletingStatus = service.SoftDeleting(DeletingUser.Key, DeletingUser.Value);
						if (deletingStatus)
						{
							byte[] buffer = Encoding.UTF8.GetBytes(AnswerDeleteOk);
							await stream.WriteAsync(buffer, 0, buffer.Length);
							await stream.FlushAsync();
							WorkStatusOK = false;
							if (ActiveClients.Any(c => c.ActiveClient == tcpClient))
							{
								ActiveClients.RemoveAt(ActiveClients.FindIndex(c => c.ActiveClient == tcpClient));
							}
							await Console.Out.WriteLineAsync($"Пользователь {DeletingUser.Key} удален\t{DateTime.Now}");
							await Console.Out.WriteLineAsync($"Активные клиенты {ActiveClients.Count}");
						}
						else
						{
							byte[] buffer = Encoding.UTF8.GetBytes(AnswerDeleteFailed);
							await stream.WriteAsync(buffer, 0, buffer.Length);
							await stream.FlushAsync();
						}
					}
				}
				else
				{
					if (ActiveClients.Any(c => c.ActiveClient == tcpClient))
					{
						ActiveClients.RemoveAt(ActiveClients.FindIndex(c => c.ActiveClient == tcpClient));
					}
					await Console.Out.WriteLineAsync($"Клиент {tcpClient.Client.RemoteEndPoint} выполнил недопустимую операцию. Связь разорвана\t{DateTime.Now}");
					await Console.Out.WriteLineAsync($"Активные клиенты {ActiveClients.Count}");
					tcpClient.Close();
					break;
				}
			}

			//Протестировать!
			//Для текущего подключения
			async void PushActiveClients()
			{
				try
				{
					List<ActiveClientLogin> OutherActiveClients = new List<ActiveClientLogin>();
					foreach (var item in ActiveClients)
					{
						if (item.ActiveClient != tcpClient)
						{
							OutherActiveClients.Add(item);
						}
					}
					Content content = new Content();
					content.ServiceText = AnswerCatchActiveUsers;
					content.Entity = JsonSerializer.SerializeToUtf8Bytes(OutherActiveClients);
					var meassage = JsonSerializer.SerializeToUtf8Bytes(content);
					await stream.WriteAsync(meassage, 0, meassage.Length);
					await stream.FlushAsync();
				}
				catch (Exception ex)
				{

                    await Console.Out.WriteLineAsync(ex.Message);
                    await Console.Out.WriteLineAsync("Ошибка PushActiveClients");
                }

			}

			//Протестировать!
			//Для текущего подключения
			async void PushRegistredClients()
			{
				try
				{
				List<string> RegisteredClients = new List<string>();
				foreach (var item in RegistredClients)
				{
					RegisteredClients.Add(item.Value);
				}
				Content content = new Content();
				content.ServiceText = AnswerCatchUsers;
				content.Entity = JsonSerializer.SerializeToUtf8Bytes(RegisteredClients);
				var meassage = JsonSerializer.SerializeToUtf8Bytes(content);
				await stream.WriteAsync(meassage, 0, meassage.Length);
				await stream.FlushAsync();
				}
				catch (Exception ex)
				{
					await Console.Out.WriteLineAsync(ex.Message);
					await Console.Out.WriteLineAsync("Ошибка PushRegistredClients");
				}

			}

			//Протестировать!
			//Для текущего клиента
			async void PushUnreadMessages()
			{
				try
				{
					string userReciverLogin = (ActiveClients.First(c => c.ActiveClient == tcpClient)).Login;
					int userId = (RegistredClients.First(l => l.Value == userReciverLogin)).Key;
					MessageService service = new MessageService();
					//Получен список непрочитанных сообщений с ссылками на вложения
					List<BLLMessageModel> unreadMessages = service.GetAllUnreadMessages(userId, RegistredClients);

					foreach (var message in unreadMessages)
					{
						byte[] outputArray = CourierServices.Packer(message.UserSender.Login, userReciverLogin, AnswerCatchMessages, message.MessageText, message.MessageContentNames);
						await stream.WriteAsync(outputArray, 0, outputArray.Length);
						await stream.FlushAsync();

						//внесение данных в БД об отправлении сообщения ПОЛУЧЕНИЕ НЕ ФИКСИРУЕТЯ
						message.IsDelivered = 1;
						service.UpdateMessage(message, RegistredClients);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine("Ошибка поиска и отправления непрочитанных сообщений");
				}
			}

			//Протестировать!
			//Для текущего клиента
			async void SendNewMessage(BLLMessageModel _incomeMessage)
			{
				MessageService service = new MessageService();
				service.InsertMessage(_incomeMessage, RegistredClients);
				try
				{
					string targetClientLogin = _incomeMessage.UserSender.Login;
					ActiveClientLogin nashClient = ActiveClients.First(l => l.Login == targetClientLogin);
					
					//Работа под большим вопросом
					NetworkStream tempstream = nashClient.ActiveClient.GetStream();

				    byte[] outputArray = CourierServices.Packer(_incomeMessage.UserSender.Login, _incomeMessage.UserReciver.Login, CommandTakeMessage, _incomeMessage.MessageText, _incomeMessage.MessageContentNames);
					await stream.WriteAsync(outputArray, 0, outputArray.Length);
					await stream.FlushAsync();


					tempstream.Close();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine("Ошибка отправки сообщения");
				}
			}

		}

		void CheckActiveClients(List<ActiveClientLogin> _clients)
		{
			object LockObj = new object();
			do
			{
				//было Task.Delay(delay)
				Task.Delay(delay).Wait();
				lock (LockObj)
				{
					for (int i = 0; i < _clients.Count; i++)
					{
						if (!_clients[i].ActiveClient.Connected)
						{
							Console.WriteLine($"Клиент {_clients[i].ActiveClient.Client.RemoteEndPoint} отключился\t{DateTime.Now}");
							_clients.Remove(_clients[i]);
							Console.WriteLine($"Активные клиенты {_clients.Count}");
						}
					}
				}
			} while (true);
		}






	}
}
