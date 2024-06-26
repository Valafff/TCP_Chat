using System;
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

namespace Server.BLL
{
	public class Server
	{
		static int Port = 2222;
		const int buffersize = 1024;
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
		const string CommandMessageFrom = "MessageFrom"; //Команда клиенту - прими сообщение от такого то пользователя
		
		Dictionary<int, string> RegistredClients = new Dictionary<int, string>();
		List<ActiveClientLogin> ActiveClients = new List<ActiveClientLogin>();
		
		TcpListener tcpListener;

		public Server(int _port)
		{
			Port = _port;
			tcpListener =
			tcpListener = new TcpListener(IPAddress.Any, Port); // сервер для прослушивания
			RegistredClients = BLL.Services.SlimUsersDictionatry.GetSlimUsersIdLogin();
		}


		public void StartServer()
		{

			try
			{
				//Запускаем сервер на прослущивание
				tcpListener.Start();
				Console.WriteLine("Сервер запущен");

				while (true)
				{
					//Ждем входящее подключение
					TcpClient tcpClient = tcpListener.AcceptTcpClient();
					//Создаем новое подключение для клиента в отдельном потоке
					Task.Factory.StartNew(async () => await ProcessClientAsync(tcpClient), TaskCreationOptions.LongRunning);
					Task.Factory.StartNew(()=> CheckActiveClients(ActiveClients), TaskCreationOptions.LongRunning);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				tcpListener.Stop();
			}
		}

		async Task ProcessClientAsync(TcpClient tcpClient)
		{
			NetworkStream stream = tcpClient.GetStream();

			bool StepOneOk = false;
			bool StepTwoOk = false;

			while (true)
			{			
				var inputcommand = ByteToString.GetStringFromStream(stream);

				////Отладка
				//await Console.Out.WriteLineAsync(inputcommand);

				//Контрольное слово для отсеивания тех, кого не звали
				if (inputcommand.ToString() == CommandHelloMsr && !StepOneOk)
				{
					await Console.Out.WriteLineAsync($"Попытка подключения {tcpClient.Client.RemoteEndPoint} одобрена\t{DateTime.Now}") ;					
					byte[] buffer = Encoding.UTF8.GetBytes(AnswerHelloUser);
					await stream.WriteAsync(buffer, 0, buffer.Length);
					await stream.FlushAsync();
					StepOneOk = true;
				}
				//Логика отправки, получения, удаления сообщений, запрос клиентов
				else if (StepTwoOk)
				{
					Courier MessageWorks = new Courier();
					BLL.Services.AccountService service = new BLL.Services.AccountService();
					try
					{
						MessageWorks = JsonSerializer.Deserialize<Courier>(inputcommand);
					}
					catch (Exception ex)
					{
						await Console.Out.WriteLineAsync(ex.Message);
						await Console.Out.WriteLineAsync("Ошибка Message-сервиса: некорректная десериализация сообщения");
						StepOneOk = false;
					}

					////Получение запроса на отправку зарегистрированных клиентов Header : GetMeUsersLogin
					if (MessageWorks.Header.Contains(CommandGetMeUsers))
					{
						string login = MessageWorks.Header.Replace(CommandGetMeUsers, "");
						PushActiveClients(ActiveClients, login);
					}











				}
				//Блок регистрации авторизации удаления аккаунта
				else if (StepOneOk)
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
						StepOneOk = false;
					}
					//РЕГИСТРАЦИЯ
					if (AccountWorks.ServiceText == CommandRegisterMe)
					{
						BLLClientModel newClient = JsonSerializer.Deserialize<BLLClientModel>(Encoding.UTF8.GetString(AccountWorks.Entity));
						bool registrationStatus = service.Register(newClient, RegistredClients);
						if (registrationStatus)
						{
							byte[] buffer = Encoding.UTF8.GetBytes(AnswerRegisterOk);
							await stream.WriteAsync(buffer, 0, buffer.Length);
							await stream.FlushAsync();
							ActiveClients.Add(new ActiveClientLogin() {ActiveClient = tcpClient, Login = newClient.Login });
							StepTwoOk = true;
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
							ActiveClients.Add(new ActiveClientLogin() { ActiveClient = tcpClient, Login = AuthorizeUser.Key});
							StepTwoOk = true;
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
							StepTwoOk = false;
							if (ActiveClients.Any(c => c.ActiveClient == tcpClient))
							{
								ActiveClients.RemoveAt( ActiveClients.FindIndex(c => c.ActiveClient == tcpClient));
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


			void PushActiveClients(List<ActiveClientLogin> _clients, string _sendByLogin)
			{

				

			}

		}

		void CheckActiveClients(List<ActiveClientLogin> _clients)
		{
			object LockObj = new object();
			do
			{
				Task.Delay(1000);
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
