using Client.Models;
using Client.Services;
using ConfigSerializeDeserialize;
using Microsoft.Xaml.Behaviors_Test;
using ProtoBuf;
using Server.BLL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;

namespace Client.ViewModels
{
	public delegate void VoidEvent();
	public delegate void SendContentToStream(byte[] content);
	public class MainMenu : INotifyCollectionChanged
	{
		public event SendContentToStream SendRegOrAuthClient;
		public event VoidEvent CloseRegistrationWindowEvent;
		public event VoidEvent CloseAuthWindowEvent;
		public event VoidEvent UpdateWindowsWithClients;

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

		bool AuthtorizationMode = false;
		bool WorkMode = false;

		const int delay = 10;
		const int buffersize = 1024;

		TcpClient tcpClient = new TcpClient();
		NetworkStream STREAM;
		IPAddress SERVERIPADDRESS;
		int SERVERPORT;


		//UIClientModel _uiclient;
		//      public UIClientModel UIClient 
		//{
		//	get => _uiclient; 
		//	set => SetField(ref _uiclient, value); 
		//}


		Server.BLL.Models.BLLMessageModel _outputMessage;
		public Server.BLL.Models.BLLMessageModel OutputMessage
		{
			get => _outputMessage;
			set => SetField(ref _outputMessage, value);
		}

		Dictionary<string, string> _loadingAttachments;
		public Dictionary<string, string> LoadingAttachments_KeyNameValuePath
		{
			get => _loadingAttachments;
			set => SetField(ref _loadingAttachments, value);
		}

		//Все сообщения прочитанные из папки clients
		Dictionary<UIClientModel, List<string>> _allmessages;
		public Dictionary<UIClientModel, List<string>> AllMessagesList
		{
			get => _allmessages;
			set => SetField(ref _allmessages, value);
		}

		//Все зарегистрированные клиенты (отправляются сервером)
		ObservableCollection<UIClientModel> _uiClients;
		public ObservableCollection<UIClientModel> UICLients
		{
			get => _uiClients;
			set => SetField(ref _uiClients, value);
		}
		//Вспомогательные поля для UICLients
		List<string> registredClients = new List<string>();
		List<string> ActiveClients;

		string _title;
		public string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		//Даныне пользователя, хранящиеся в постоянной памяти
		UserConfig _userConfig;
		public UserConfig UserConfigData
		{
			get => _userConfig;
			set => SetField(ref _userConfig, value);
		}


		//Пользователь после авторизации
		Server.BLL.Models.BLLClientModel _bllClientModel;
		public Server.BLL.Models.BLLClientModel BLLClient
		{
			get => _bllClientModel;
			set => SetField(ref _bllClientModel, value);
		}

		//Флаг устанавливающийся при регистрации - если true -  пароль и логин пользователя записываются в конфиг. При запуске клиента авторизация происходит атоматически
		bool _autoInput;
		public bool AutoAuthtorization
		{
			get => _autoInput;
			set => SetField(ref _autoInput, value);
		}

		public Lambda RegistrMe { get; set; }
		public Lambda Authtorizeme { get; set; }
		public Lambda PushMessage { get; set; }

		public MainMenu()
		{
			Title = "МиниЧат";
			BLLClient = new Server.BLL.Models.BLLClientModel();
			UICLients = new ObservableCollection<UIClientModel>();
			OutputMessage = new Server.BLL.Models.BLLMessageModel() { UserReciver = new Server.BLL.Models.BLLSlimClientModel(), UserSender = new Server.BLL.Models.BLLSlimClientModel(), MessageContentNames = new List<string>() };
			AllMessagesList = new Dictionary<UIClientModel, List<string>>();
			LoadingAttachments_KeyNameValuePath = new Dictionary<string, string>();
			ActiveClients = new List<string>();
			SendRegOrAuthClient += RegistrationOrAuthtorize;



			RegistrMe = new Lambda(
				execute: _ =>
				{
					BLLClient.Status = 1;
					BLLClient.LastVisit = DateTime.Now;

					Courier courier = new Courier() { Header = CommandRegisterMe, MessageText = JsonSerializer.Serialize(BLLClient) };
					var buffer = Server.BLL.Services.CourierServices.Packer(courier);
					SendRegOrAuthClient(buffer);
				},
				canExecute => AuthtorizationMode == true
				);

			//Update
			Authtorizeme = new Lambda(
				execute: _ =>
				{
					KeyValuePair<string, string> AuthorizeUser = new KeyValuePair<string, string>(key: BLLClient.Login, value: BLLClient.Password);
					Courier courier = new Courier() { Header = CommandAuthorizeMe, MessageText = JsonSerializer.Serialize(AuthorizeUser) };
					var buffer = Server.BLL.Services.CourierServices.Packer(courier);
					SendRegOrAuthClient(buffer);
				},
				canExecute => AuthtorizationMode == true
				);

			PushMessage = new Lambda(
				execute: _ =>
				{
					AllMessagesList = ReadAllMessagesFromMemory(UICLients);
					byte[] arr = Server.BLL.Services.CourierServices.Packer(OutputMessage, CommandMessageTo, LoadingAttachments_KeyNameValuePath);
					SendMessageToServer(STREAM, arr);
				},
				canExecute => OutputMessage.UserReciver != null
				);
		}


		ClientCommands commands = new ClientCommands();
		public void TCPClientWork(IPAddress _serverIP, int _serverPort)
		{
			try
			{

				SERVERIPADDRESS = _serverIP;
				SERVERPORT = _serverPort;
				tcpClient.Connect(_serverIP, _serverPort);
				NetworkStream stream = tcpClient.GetStream();
				STREAM = stream;

				commands.HelloServer(stream);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка подключения к серверу. {ex.Message}");
			}

		}

		//Чтение данных отправленных сервером и Логика работы клиента
		public void NetworkStreamReader()
		{
			//string serverAnswer = "";

			//Console.ReadKey();

			try
			{
				using NetworkStream stream = tcpClient.GetStream();

				while (true)
				{
					Server.BLL.Models.Courier courier = Server.Tools.StreamToCourierClass.StreamToCourier(stream);
					Console.WriteLine(courier.Header);
					try
					{
						//Автоматическая авторизация выполняется если сервер принимает подключение и стоит флаг автоматического входа
						if (courier.Header == AnswerHelloUser && UserConfigData.AutoAuthtorization)
						{
							commands.AutoAuthtoeize(BLLClient, this, UserConfigData);
						}
						if (courier.Header == AnswerRegisterFailed)
						{
							MessageBox.Show("Отказ в регистрации. Логин занят или введены некорректные символы");
						}
						if (courier.Header == AnswerAuthorizationFailed)
						{
							MessageBox.Show("Ошибка авторизации");
						}
						if (courier.Header == AnswerAuthorizationOk || courier.Header == AnswerRegisterOk)
						{
							commands.RequesRegistredClients(stream);
						}
						if (courier.Header == AnswerCatchUsers)
						{
							List<string> temp = commands.ReadRegistredClients(courier);
							foreach (var item in temp)
							{
								Application.Current.Dispatcher.Invoke(() => { UICLients.Add(new UIClientModel() { Login = item }); });
							}
						}
						if (courier.Header == AnswerCatchUsers)
						{
							commands.RequestActiveUsers(stream);
						}
						if (courier.Header == AnswerCatchActiveUsers)
						{
							ActiveClients = commands.ReadActiveClients(courier);
							CheckActiveClients();
							ResultStringBuilder(UICLients);
							//Обновление UI
							UpdateWindowsWithClients();
						}
						if (courier.Header == AnswerMessageSendOk)
						{
							MessageBox.Show("Сообщение отправлено");
						}
						if (courier.Header == AnswerMessageSendFailed)
						{
							MessageBox.Show("Сообщение не отправлено");
						}

						//Дай мне список непрочитанных сообщений





						//Расшифровка сообщений до авторизации
						if (!WorkMode)
						{
							//serverAnswer = ByteToString.GetStringFromStream(stream);
						}
						//Расшифровка сообщений после авторизации
						else
						{
							//workLeveldata = StreamToCourierClass.StreamToByteArr(stream);
						}

						if (!WorkMode)
						{
							//Контрольное прослушивание сообщений сервера
							//Console.WriteLine(serverAnswer);

							//запрос всех зарегистрированных пользователей
							//if (serverAnswer.Contains(AnswerAuthorizationOk))
							//{
							//	WorkMode = true;
							//	//Первый запрос зарегистрированных клиентов
							//	registredClients = ReadRegisterUsers(serverAnswer);
							//	foreach (var item in registredClients)
							//	{
							//		UICLients.Add(new UIClientModel() { Login = item });
							//	}
							//	if (CloseAuthWindowEvent != null)
							//	{
							//		CloseAuthWindowEvent();
							//	}
							//	//Первый запрос активных клиентов
							//	GiveMeActiveClients(stream);
							//}





							////Выполняется если сервер принимает подключение и стоит флаг автоматического входа
							//else if (serverAnswer == AnswerHelloUser && UserConfigData.AutoAuthtorization)
							//{
							//	BLLClient.Login = UserConfigData.Login;
							//	BLLClient.Password = UserConfigData.Password;
							//	Authtorizeme.Execute(this);
							//}





							////Если сервер принимает подключение
							//else if (serverAnswer == AnswerHelloUser)
							//{
							//	//Клиент может регистрироваться, авторизоваться, удалить аккаунт
							//	AuthtorizationMode = true;
							//}
							////Если регистрация прошла успешно
							//else if (serverAnswer.Contains(AnswerRegisterOk))
							//{
							//	if (AutoAuthtorization) UserConfigData.Login = BLLClient.Login;
							//	if (AutoAuthtorization) UserConfigData.Password = BLLClient.Password;
							//	UserConfigData.FirstName = BLLClient.FirstName;
							//	UserConfigData.SecondName = BLLClient.SecondName;
							//	UserConfigData.AutoAuthtorization = AutoAuthtorization;

							//	ConfigWriteReadJson.ReWriteConfig(UserConfigData, "UserConfig.json");
							//	MessageBox.Show("Регистрация прошла успешно!");
							//	WorkMode = true;
							//	registredClients = ReadRegisterUsers(serverAnswer);
							//	foreach (var item in registredClients)
							//	{
							//		UICLients.Add(new UIClientModel() { Login = item });
							//	}
							//	if (CloseRegistrationWindowEvent != null)
							//	{
							//		CloseRegistrationWindowEvent();
							//	}
							//	//Первый запрос активных клиентов
							//	GiveMeActiveClients(stream);
							//}
							//else if (serverAnswer == AnswerRegisterFailed)
							//{
							//	MessageBox.Show("Отказ в регистрации. Логин занят или введены некорректные символы");
							//}
							//else if (serverAnswer == AnswerAuthorizationFailed)
							//{
							//	MessageBox.Show("Ошибка авторизации");
							//}
							//Не обработано событие удаления клиента
						}
						else if (WorkMode)
						{
							Server.BLL.Models.BLLMessageModel IncomeMessage = new Server.BLL.Models.BLLMessageModel();
							string command = "NoCommand";
							try
							{
								//Services.AccountService service = new Services.AccountService();
								//IncomeMessage = Services.CourierServices.Unpacker(workLeveldata, out command, out Dictionary<string, byte[]> nulldata);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
								Console.WriteLine("Ошибка при чтении сообщения: некорректная десериализация сообщения");
								AuthtorizationMode = false;
								WorkMode = false;
							}

							if (command == AnswerCatchActiveUsers)
							{
								AllMessagesList = ReadAllMessagesFromMemory(UICLients);

								if (IncomeMessage.MessageText != null)
								{
									ActiveClients = JsonSerializer.Deserialize<List<string>>(IncomeMessage.MessageText);
								}
								foreach (var item in UICLients)
								{
									if (ActiveClients.Contains(item.Login))
									{
										item.IsActive = true;
										//item.BackColor = "LawnGreen";
									}
									else
									{
										item.IsActive = false;
										//item.BackColor = "White";
									}
								}

								//Первый запрос непрочитанных сообщений
								Server.BLL.Models.Courier courier_OLD = new Server.BLL.Models.Courier();
								courier.Header = CommandGiveMeUnReadMes;
								byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(courier);
								stream.Write(buffer, 0, buffer.Length);
								stream.Flush();

							}






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
				STREAM.Write(content);
				//STREAM.Flush();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка регистрации {ex.Message}");
				CloseRegistrationWindowEvent();
			}
		}

		//Работает не корректно исправить
		public void ReloadConnection()
		{
			tcpClient.Close();
			Task.Delay(100).Wait();
			tcpClient = new TcpClient();
			TCPClientWork(SERVERIPADDRESS, SERVERPORT);
			Task.Run(new Action(() => NetworkStreamReader()));
		}

		//public void ConnectWithNewrop(BLLClientModel _client, UserConfig _userConfig)
		//{
		//	BLLClient = _client;
		//	UserConfigData = _userConfig;
		//	tcpClient.Close();
		//	Task.Delay(100).Wait();
		//	tcpClient = new TcpClient();
		//	TCPClientWork(SERVERIPADDRESS, SERVERPORT);
		//	Task.Run(new Action(() => NetworkStreamReader()));
		//}

		//List<string> ReadRegisterUsers(string _inputString)
		//{
		//	try
		//	{
		//		Server.BLL.Models.Content content = JsonSerializer.Deserialize<Server.BLL.Models.Content>(_inputString);
		//		List<string> reg = new List<string>();
		//		reg = JsonSerializer.Deserialize<List<string>>(content.Entity);
		//		return reg;
		//	}
		//	catch (Exception ex)
		//	{
		//		//Console.WriteLine(ex);
		//		return null;
		//	}
		//}

		//void GiveMeActiveClients(Stream stream)
		//{
		//	byte[] outputArray = Server.BLL.Services.CourierServices.Packer(CommandGetMeActiveUsers);
		//	stream.Write(outputArray);
		//	stream.Flush();
		//}

		void SendMessageToServer(Stream stream, byte[] _arr)
		{
			stream.Write(_arr, 0, _arr.Length);
			stream.Flush();
		}


		//Получение списка всех сообщений от всех клиентов
		Dictionary<UIClientModel, List<string>> ReadAllMessagesFromMemory(ObservableCollection<UIClientModel> _clients)
		{
			Dictionary<UIClientModel, List<string>> dict = new Dictionary<UIClientModel, List<string>>();
			if (!Directory.Exists(Directory.GetCurrentDirectory() + $"\\Clients\\"))
			{
				Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\Clients\\");
			}

			DirectoryInfo ClientDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + $"\\Clients\\");
			DirectoryInfo[] directories = ClientDirectory.GetDirectories();

			foreach (var item in directories)
			{
				FileInfo ClientMessagesFile = new FileInfo(Directory.GetCurrentDirectory() + $"\\Clients\\" + $"\\{item.Name}\\" + item.Name + ".txt");

				if (ClientMessagesFile.Exists)
				{
					var tempMessages = new List<string>();
					using (StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + $"\\Clients\\" + $"\\{item.Name}\\" + item.Name + ".txt"))
					{
						while (!sr.EndOfStream)
						{
							tempMessages.Add(sr.ReadLine());
						}
					}
					dict.Add(key: _clients.First(c => c.Login == item.Name), value: tempMessages);
				}
			}
			return dict;
		}

		//_messagedata подгружаются из списка сообщений txt на диске 
		void ResultStringBuilder(ObservableCollection<UIClientModel> _clients)
		{
			foreach (var item in _clients)
			{
				if (item.Login == BLLClient.Login)
				{
					item.ResultString = $"{item.Login} (Вы)";
				}
				else
				{
					int countMessages = 0;
					if (AllMessagesList.ContainsKey(item))
					{
						countMessages = AllMessagesList[item].Count;
					}
					item.ResultString = $"{item.Login} Всего сообщений {countMessages} Был в сети {item.LastVisit}";
				}
			}
		}

		public void CheckActiveClients()
		{
            foreach (var item in UICLients)
            {
				if (ActiveClients.Contains(item.Login))
				{
					item.IsActive = true;
					//item.BackColor = "LawnGreen";
				}
				else
				{
					item.IsActive = false;
					//item.BackColor = "White";
				}
			}
        }

		//public void RefreshUsers(ObservableCollection<UIClientModel> _clients)
		//{
		//	//Поэлементное копирование
		//	var tempDict = ReadAllMessagesFromMemory(_clients);
		//	//AllMessagesList

		//	ResultStringBuilder(_clients);

		//}




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
