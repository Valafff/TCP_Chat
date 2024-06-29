using Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModels
{
	public class MainMenu : INotifyCollectionChanged
	{
		const int HeaderBytes = 4;
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

		TcpClient tcpClient = new	TcpClient();

		public  void TCPClientWork(IPAddress _serverIP, int _serverPort )
		{
			try
			{
				 tcpClient.Connect(_serverIP, _serverPort);
				NetworkStream stream = tcpClient.GetStream();
				//Контрольное сообщение серверу
				byte[] data = Encoding.UTF8.GetBytes(CommandHelloMsr);


				//int messageSize = data.Length;

				////MessageBox.Show($"{messageSize}"); //8

				//byte[] header = new byte[HeaderBytes];
				//byte[] arrLenght =	JsonSerializer.SerializeToUtf8Bytes(messageSize);
				//arrLenght.CopyTo(header, 0);
				//byte[] resultArray = new byte[HeaderBytes+messageSize];
				//header.CopyTo(resultArray, 0);
				//data.CopyTo(resultArray, 4);
				//stream.Write(resultArray);

				stream.Write(data);
			}
			catch (Exception ex)
			{
				MessageBox.Show( ex.Message );
			}

		}

		public void NetworkStreamReader()
		{
			try
			{
				using NetworkStream stream = tcpClient.GetStream();
				while (true)
				{
					try
					{
						List<byte[]> request = new List<byte[]>();
						int temp;
						byte[] buffer = new byte[1024];

						while ((temp = stream.Read(buffer, 0, buffer.Length)) > 0)
						{
							byte[] data = new byte[temp];
							data = buffer[0..temp];
							request.Add(data);
							if (temp < 1024) { break; }
						}



						//Модифицировать блок
						StringBuilder command = new StringBuilder();
						foreach (byte[] data in request)
						{
							command.Append(Encoding.UTF8.GetString(data));
						}
						MessageBox.Show(command.ToString());



					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
						break;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

















		public event PropertyChangedEventHandler? PropertyChanged;
		public event NotifyCollectionChangedEventHandler? CollectionChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}

}
