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


		//UIClientModel _uiclient;
		//      public UIClientModel UIClient 
		//{
		//	get => _uiclient; 
		//	set => SetField(ref _uiclient, value); 
		//}


		BLLMessageModel _outputMessage;
		public BLLMessageModel OutputMessage
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
		List<UIClientModel> _uiClients;
		public List<UIClientModel> UICLients
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
		BLLClientModel _bllClientModel;
		public BLLClientModel BLLClient
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
			BLLClient = new BLLClientModel();
			UICLients = new List<UIClientModel>();
			OutputMessage = new BLLMessageModel() { UserReciver = new BLLSlimClientModel(), UserSender = new BLLSlimClientModel(), MessageContentNames = new List<string>()};
			AllMessagesList = new Dictionary<UIClientModel, List<string>>();
			LoadingAttachments_KeyNameValuePath = new Dictionary<string, string>();
			ActiveClients = new List<string>();
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

			PushMessage = new Lambda(
				execute: _ =>
				{
					AllMessagesList = ReadAllMessagesFromMemory(UICLients);
					byte[] arr = Services.CourierServices.Packer(OutputMessage, CommandMessageTo);
					SendMessageToServer(STREAM, arr);
				},
				canExecute => OutputMessage.UserReciver != null
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
									UICLients.Add(new UIClientModel() { Login = item });
								}
								if (CloseAuthWindowEvent != null)
								{
									CloseAuthWindowEvent();
								}
								GiveMeActiveClients(stream);
							}
							//Выполняется если сервер принимает подключение и стоит флаг автоматического входа
							else if (serverAnswer == AnswerHelloUser && UserConfigData.AutoAuthtorization)
							{
								BLLClient.Login = UserConfigData.Login;
								BLLClient.Password = UserConfigData.Password;
								Authtorizeme.Execute(this);
							}
							//Если сервер принимает подключение
							else if (serverAnswer == AnswerHelloUser)
							{
								//Клиент может регистрироваться, авторизоваться, удалить аккаунт
								AuthtorizationMode = true;
							}
							//Если регистрация прошла успешно
							else if (serverAnswer.Contains(AnswerRegisterOk))
							{
								if (AutoAuthtorization) UserConfigData.Login = BLLClient.Login;
								if (AutoAuthtorization) UserConfigData.Password = BLLClient.Password;
								UserConfigData.FirstName = BLLClient.FirstName;
								UserConfigData.SecondName = BLLClient.SecondName;
								UserConfigData.AutoAuthtorization = AutoAuthtorization;

								ConfigWriteReadJson.ReWriteConfig(UserConfigData, "UserConfig.json");
								MessageBox.Show("Регистрация прошла успешно!");
								WorkMode = true;
								registredClients = ReadRegisterUsers(serverAnswer);
								foreach (var item in registredClients)
								{
									UICLients.Add(new UIClientModel() { Login = item });
								}
								if (CloseRegistrationWindowEvent != null)
								{
									CloseRegistrationWindowEvent();
								}
								GiveMeActiveClients(stream);
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
							BLLMessageModel IncomeMessage = new BLLMessageModel();
							string command = "NoCommand";
							try
							{
								Services.AccountService service = new Services.AccountService();
								IncomeMessage = Services.CourierServices.Unpacker(workLeveldata, out command);
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
								ResultStringBuilder(UICLients);
								if (IncomeMessage.MessageText != null)
								{
									ActiveClients = JsonSerializer.Deserialize<List<string>>(IncomeMessage.MessageText);
								}
								foreach (var item in UICLients)
								{
									if (ActiveClients.Contains(item.Login))
									{
										item.IsActive = true;
										item.BackColor = "LawnGreen";
									}
									else
									{
										item.IsActive = false;
										item.BackColor = "White";
									}
								}
							}







							//Console.ReadKey();
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

		//Работает не корректно исправить
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

		void GiveMeActiveClients(Stream stream)
		{
			byte[] outputArray = Services.CourierServices.Packer(CommandGetMeActiveUsers);
			stream.Write(outputArray);
			stream.Flush();
		}

		void SendMessageToServer(Stream stream, byte[] _arr)
		{
			stream.Write(_arr);
			stream.Flush();
		}


		//Получение списка всех сообщений от всех клиентов
		Dictionary<UIClientModel, List<string>> ReadAllMessagesFromMemory(List<UIClientModel> _clients)
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
		void ResultStringBuilder(List<UIClientModel> _clients)
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

		public void RefreshUsers(List<UIClientModel> _clients)
		{
			Dictionary<UIClientModel, List<string>> dict = ReadAllMessagesFromMemory(_clients);
			ResultStringBuilder(_clients);

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
