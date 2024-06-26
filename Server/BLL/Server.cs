using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using Server.Tools;

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
		const string AnswerCatchUsers = "CatchUsers";
		const string CommandGiveMeUnReadMes = "GiveMeUnReadMes";
		const string AnswerCatchMessages = "CatchMessages";
		const string CommandMessageTo = "MessageTo"; //Команда серверу - отправь сообщение такому то пользователю
		const string CommandMessageFrom = "MessageFrom"; //Команда клиенту - прими сообщение от такого то пользователя

		List<TcpClient> ActiveClients = new List<TcpClient>();
		TcpListener tcpListener;
		public Server(int _port)
        {
            Port = _port;
			tcpListener =
			tcpListener = new TcpListener(IPAddress.Any, Port); // сервер для прослушивания
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
			var inputcommand = ByteToString.GetStringFromStream(stream);
			if (inputcommand.ToString() == CommandHelloMsr)
			{
				await Console.Out.WriteLineAsync("OK");
				byte[] buffer = Encoding.UTF8.GetBytes(AnswerHelloUser);
				await stream.WriteAsync(buffer, 0, buffer.Length);
				await stream.FlushAsync();
			}
			else
			{
				await Console.Out.WriteLineAsync("Failed");
				tcpClient.Close();
			}




        }






	}
}
