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
using Server.BLL.Services;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;

namespace Server.BLL
{
	public class Server
	{
		static int Port = 8888;
		const int delay = 10;

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
						_ = Task.Factory.StartNew(() => CheckActiveClients(ref ActiveClients), TaskCreationOptions.LongRunning);
						_ = Task.Factory.StartNew(() => ActiveClientsKeeper(ref ActiveClients), TaskCreationOptions.LongRunning);
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


			bool AuthtorizationMode = false;
			bool WorkMode = false;
			while (true)
			{
				Courier courier = StreamToCourierClass.StreamToCourier(stream);
				Services.ServerCommands commands = new Services.ServerCommands();

				//Контрольное слово для отсеивания тех, кого не звали
				if (courier.Header == com.CommandHelloMsr)
				{
					commands.HelloClient(stream, tcpClient);
				}
				else if (courier.Header == com.CommandRegisterMe)
				{
					commands.Registration(courier, ref RegistredClients, ActiveClients, tcpClient, stream);
				}
				else if (courier.Header == com.CommandAuthorizeMe)
				{
					commands.Authtorization(courier, RegistredClients, ActiveClients, tcpClient, stream);
				}
				else if (courier.Header == com.CommandGetMeUsers)
				{
					commands.TakeRegistredUsers(stream, RegistredClients);
				}
				else if (courier.Header == com.CommandGetMeActiveUsers)
				{
					commands.TakeActiveClients(stream, ActiveClients);
				}
				else if (courier.Header == com.CommandMessageTo)
				{
					commands.SendNewMessage(courier, RegistredClients, stream, ActiveClients);
				}
				else if (courier.Header == com.CommandGiveMeUnReadMes)
				{
					commands.SendUnreadMessagesForClient(RegistredClients, ActiveClients, tcpClient, stream);
				}
				else if (courier.Header == com.CommandMessageDeliveredOK)
				{
					commands.CheckDeliveredMesseges(courier, RegistredClients);
				}
				else if (courier.Header == com.CommandGiveMeAttachments)
				{
					commands.AttachmentsSend(stream, courier, RegistredClients);
				}

				//	//УДАЛЕНИЕ КЛИЕНТА
				//	if (AccountWorks.ServiceText == CommandDeleteMe)
				//	{
				//		KeyValuePair<string, string> DeletingUser = JsonSerializer.Deserialize<KeyValuePair<string, string>>(Encoding.UTF8.GetString(AccountWorks.Entity));
				//		bool deletingStatus = service.SoftDeleting(DeletingUser.Key, DeletingUser.Value);
				//		if (deletingStatus)
				//		{
				//			byte[] buffer = Encoding.UTF8.GetBytes(AnswerDeleteOk);
				//			await stream.WriteAsync(buffer, 0, buffer.Length);
				//			await stream.FlushAsync();
				//			WorkMode = false;
				//			if (ActiveClients.Any(c => c.ActiveClient == tcpClient))
				//			{
				//				ActiveClients.RemoveAt(ActiveClients.FindIndex(c => c.ActiveClient == tcpClient));
				//			}
				//			await Console.Out.WriteLineAsync($"Пользователь {DeletingUser.Key} удален\t{DateTime.Now}");
				//			await Console.Out.WriteLineAsync($"Активные клиенты {ActiveClients.Count}");
				//		}
				//		else
				//		{
				//			byte[] buffer = Encoding.UTF8.GetBytes(AnswerDeleteFailed);
				//			await stream.WriteAsync(buffer, 0, buffer.Length);
				//			await stream.FlushAsync();
				//		}
				//	}
				//}

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

		}
		void CheckActiveClients(ref List<ActiveClientLogin> _clients)
		{
			object LockObj = new object();
			do
			{
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

		void ActiveClientsKeeper(ref List<ActiveClientLogin> _clients)
		{

			int oldCount = 0;
			object LockObj = new object();
			do
			{
				int nowCount = _clients.Count;
				Task.Delay(delay*10).Wait();
				lock (LockObj)
				{
                    if (oldCount != nowCount)
					{
						oldCount = nowCount;
						//Извещает всех об отключении пользователя
						Courier courier = new Courier();
						courier.Header = com.AnswerCatchUsers;
						courier.MessageText = JsonSerializer.Serialize(RegistredClients.Values);
						var buffer = CourierServices.Packer(courier);
						Console.WriteLine(_clients.Count);
						foreach (var client in _clients)
						{
							Tools.DataToBinaryWriter.WriteData(client.ClientStream, buffer);
						}
					}
				}
			} while (true);
		}
	}
}
