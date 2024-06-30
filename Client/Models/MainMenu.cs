using Client.Models;
using Client.Tools;
using ConfigSerializeDeserialize;
using Microsoft.Xaml.Behaviors_Test;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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
	public delegate void VoidEvent();
	public delegate void SendContentToStream(byte[] content);
	public class MainMenu : INotifyCollectionChanged
	{
		public event SendContentToStream SendRegOrAuthClient;
		public event VoidEvent CloseRegistrationWindowEvent;
		public event VoidEvent CloseAuthWindowEvent;

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

		bool AuthtorizationMode = false;
		bool WorkMode = false;
		const int delay = 10;
		const int buffersize = 1024;
		TcpClient tcpClient = new TcpClient();
		NetworkStream STREAM;
		IPAddress SERVERIPADDRESS;
		int SERVERPORT;
		List<string> registredClients = new List<string>();

		//UIClientModel _uiclient;
  //      public UIClientModel UIClient 
		//{
		//	get => _uiclient; 
		//	set => SetField(ref _uiclient, value); 
		//}

        List<UIClientModel> _uiClients;
		public List<UIClientModel> UICLients
		{
			get => _uiClients;
			set => SetField(ref _uiClients, value);
		}

		string _title;
		public string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		UserConfig _userConfig;
		public UserConfig UserConfigData
		{
			get => _userConfig;
			set => SetField(ref _userConfig, value);
		}

		BLLClientModel _bllClientModel;
		public BLLClientModel BLLClient
		{
			get => _bllClientModel;
			set => SetField(ref _bllClientModel, value);
		}

		bool _autoInput;
		public bool AutoAuthtorization
		{
			get => _autoInput;
			set => SetField(ref _autoInput, value);
		}

		public Lambda RegistrMe { get; set; }
		public Lambda Authtorizeme { get; set; }

		public MainMenu()
		{
			Title = "МиниЧат";
			BLLClient = new BLLClientModel();
			UICLients = new List<UIClientModel>();
			SendRegOrAuthClient += RegistrationOrAuthtorize;

			RegistrMe = new Lambda(
				execute: _ =>
				{
					BLLClient.Status = 1;
					BLLClient.LastVisit = DateTime.Now;
					var clientByteArr = JsonSerializer.SerializeToUtf8Bytes(BLLClient);
					Content content = new Content() { ServiceText = CommandRegisterMe, Entity = clientByteArr };
					var contentByteArr = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content));
					SendRegOrAuthClient(contentByteArr);
				},
				canExecute => AuthtorizationMode == true
				);
			Authtorizeme = new Lambda(
				execute: _ =>
				{
					KeyValuePair<string, string> AuthorizeUser = new KeyValuePair<string, string>(key: BLLClient.Login, value: BLLClient.Password);
					Content content = new Content() { ServiceText = CommandAuthorizeMe, Entity = JsonSerializer.SerializeToUtf8Bytes(AuthorizeUser) };
					var contentByteArr = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content));
					SendRegOrAuthClient(contentByteArr);
				},
				canExecute => AuthtorizationMode == true
				);
		}





		public void TCPClientWork(IPAddress _serverIP, int _serverPort)
		{
			try
			{
				SERVERIPADDRESS = _serverIP;
				SERVERPORT = _serverPort;
				tcpClient.Connect(_serverIP, _serverPort);
				NetworkStream stream = tcpClient.GetStream();
				STREAM = stream;
				//Контрольное сообщение серверу
				byte[] data = Encoding.UTF8.GetBytes(CommandHelloMsr);
				stream.Write(data);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка подключения к серверу. {ex.Message}");
			}

		}

		//Чтение данных отправленных сервером и Логика работы клиента
		public void NetworkStreamReader()
		{
			string serverAnswer = "";
			byte[] workLeveldata = null;
			try
			{
				using NetworkStream stream = tcpClient.GetStream();
				while (true)
				{
					try
					{
						//Расшифровка сообщений до авторизации
						if (!WorkMode)
						{
							serverAnswer = ByteToString.GetStringFromStream(stream);
						}
						//Расшифровка сообщений после авторизации
						else
						{
							workLeveldata = StreamToByte.StreamToByteArr(stream);
						}

						if (!WorkMode)
						{
							//Контрольное прослушивание сообщений сервера
							Console.WriteLine(serverAnswer);

							//запрос всех зарегистрированных пользователей
							if (serverAnswer.Contains(AnswerAuthorizationOk))
							{
								WorkMode = true;
								registredClients = ReadRegisterUsers(serverAnswer);
								foreach (var item in registredClients)
								{
									UIClientModel model = new UIClientModel() { Login = item};
									UICLients.Add(model);
								}

								if (CloseAuthWindowEvent != null)
								{
									CloseAuthWindowEvent();
								}
							}
							//Выполняется если сервер принимает подключение и стоит флаг автоматического входа
							else if (serverAnswer == AnswerHelloUser && UserConfigData.AutoAuthtorization)
							{
								BLLClient.Login = UserConfigData.Login;
								BLLClient.Password = UserConfigData.Password;
								Lambda Auth = Authtorizeme;
								Auth.Execute(this);
							}
							//Если сервер принимает подключение
							else if (serverAnswer == AnswerHelloUser)
							{
								//Клиент может регистрироваться, авторизоваться, удалить аккаунт
								AuthtorizationMode = true;
							}
							//Если регистрация прошла успешно
							else if (serverAnswer == AnswerRegisterOk)
							{
								if (AutoAuthtorization) UserConfigData.Login = BLLClient.Login;
								if (AutoAuthtorization) UserConfigData.Password = BLLClient.Password;
								UserConfigData.FirstName = BLLClient.FirstName;
								UserConfigData.SecondName = BLLClient.SecondName;
								UserConfigData.AutoAuthtorization = AutoAuthtorization;
								ConfigWriteReadJson.ReWriteConfig(UserConfigData, "UserConfig.json");
								MessageBox.Show("Регистрация прошла успешно!");
								CloseRegistrationWindowEvent();
							}
							else if (serverAnswer == AnswerRegisterFailed)
							{
								MessageBox.Show("Отказ в регистрации. Логин занят или введены некорректные символы");
							}
							else if (serverAnswer == AnswerAuthorizationFailed)
							{
								MessageBox.Show("Ошибка авторизации");
							}
							//Не обработано событие удаления клиента
						}
						else if (WorkMode)
						{









							Console.ReadKey();
						}


					}
					catch (Exception ex)
					{
						//MessageBox.Show(ex.Message);
						break;
					}
				}
			}
			catch (Exception ex)
			{
				//MessageBox.Show(ex.Message);
				Console.WriteLine(ex);
			}




		}


		void RegistrationOrAuthtorize(byte[] content)
		{
			try
			{
				STREAM.WriteAsync(content);
				STREAM.Flush();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка регистрации {ex.Message}");
				CloseRegistrationWindowEvent();
			}
		}
		public void ReloadConnection()
		{
			tcpClient.Close();
			tcpClient = new TcpClient();
			TCPClientWork(SERVERIPADDRESS, SERVERPORT);
			Task.Run(new Action(() => NetworkStreamReader()));
		}

		List<string> ReadRegisterUsers(string _inputString)
		{
			try
			{
				Content content = JsonSerializer.Deserialize<Content>(_inputString);
				List<string> reg = new List<string>();
				reg = JsonSerializer.Deserialize<List<string>>(content.Entity);
				return reg;
			}
			catch (Exception ex)
			{
				//Console.WriteLine(ex);
				return null;
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
