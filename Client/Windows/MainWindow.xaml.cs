using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.Windows;
using Client.ViewModels;
using Client.Models;
using ConfigSerializeDeserialize;
using System.Net;
using Client.Services;

namespace Client
{

	public partial class MainWindow : Window
	{
		object UpdateEventRef;
		MainMenu main = new MainMenu();
		UserConfig user = new UserConfig();
		ChatConfig chat = new ChatConfig();
		ChatRoom Room;


		public MainWindow()
		{
			try
			{
				user = ConfigWriteReadJson.ReadConfig<UserConfig>("UserConfig.json");
			}
			catch (Exception)
			{
				ConfigWriteReadJson.ReWriteConfig(user, "UserConfig.json");
			}
			try
			{
				chat = ConfigWriteReadJson.ReadConfig<ChatConfig>("Options.json");
			}
			catch (Exception)
			{
				ConfigWriteReadJson.ReWriteConfig(chat, "Options.json");
			}


			if (chat.ServerIP == null || chat.ServerPort == 0)
			{
				OptionsWindow options = new OptionsWindow();
				options.ShowDialog();
				chat = ConfigWriteReadJson.ReadConfig<ChatConfig>("Options.json");
			}

			main.UserConfigData = user;

			main.TCPClientWork(IPAddress.Parse(chat.ServerIP), chat.ServerPort);
			Task.Run(new Action(() => main.NetworkStreamReader()));


			InitializeComponent();
			DataContext = main;
			bool firstAuthtorize = false;
			if (user.FirstName == null || user.SecondName == null)
			{
				firstAuthtorize = true;
				RegistrationWindow window = new RegistrationWindow(main);
				window.ShowDialog();
			}
			if (!user.AutoAuthtorization && !firstAuthtorize)
			{
				AuthtorizeWindow window = new AuthtorizeWindow(main);
				window.ShowDialog();
			}

			main.UpdateWindowsWithClients += UpdateClients;

		}

		private void MenuItem_Click_CloseChat(object sender, RoutedEventArgs e)
		{
			Close();
		}

		//Глючит
		private void MenuItem_Click_ReloadConnection(object sender, RoutedEventArgs e)
		{
			//main.ReloadConnection();
		}

		private void MenuItem_Click_Authtorize(object sender, RoutedEventArgs e)
		{
			AuthtorizeWindow window = new AuthtorizeWindow(main);
			window.ShowDialog();
			//UpdateClients();
		}

		private void Button_Client_Click(object sender, RoutedEventArgs e)
		{
			if (!((Button)sender).Content.ToString().Contains(main.BLLClient.Login))
			{
				UIClientModel ReciverClient = main.UICLients.First(l => ((Button)sender).Content.ToString().Contains(l.ResultString));
				Room = new ChatRoom(main, ReciverClient, main.BLLClient.Login, this);
				Room.ShowDialog();
				Room.Close();
			}
		}

		private void MenuItem_NewRegistration_Click(object sender, RoutedEventArgs e)
		{
			main.BLLClient.Login = "";
			main.BLLClient.FirstName = "";
			main.BLLClient.SecondName = "";
			main.UserConfigData.AutoAuthtorization = false;
			//main.ReloadConnection();
			RegistrationWindow window = new RegistrationWindow(main);
			window.ShowDialog();
		}

		public void UpdateClients()
		{

			Application.Current.Dispatcher.Invoke(() =>
			{
				ClientsStack.Children.Clear();
				foreach (var item in main.UICLients)
				{
					Button bt = new Button() { Name = $"bt_{item.Login}",  Content = $"{item.ResultString}", BorderBrush = Brushes.White,  Background = item.IsActive == true ? Brushes.LawnGreen : Brushes.White };
					bt.Click += Button_Client_Click;
					bt.HorizontalAlignment = HorizontalAlignment.Left;
					Grid.SetRow(bt, 0);
					ClientsStack.Children.Add(bt);
				}
			});

		}

		private void MenuItem_Click_Connect(object sender, RoutedEventArgs e)
		{
			main.UICLients.Clear();
			main.ActiveClients.Clear();
			main.UnreadMessagesTextOnly.Clear();
			main.AllMessagesList.Clear();
			main.ReloadConnection();
		}

		private void MenuItem_Click_Disconect(object sender, RoutedEventArgs e)
		{
			main.UICLients.Clear();
			main.ActiveClients.Clear();
			main.UnreadMessagesTextOnly.Clear();
			main.AllMessagesList.Clear();
			main.Disconnect();
		}




		//void ReadAllMessages()
		//{
		//	if (Correspondence != null)
		//		//Сообщения из архива
		//		foreach (var item in Correspondence)
		//		{
		//			//var mes = item.Remove(0, 19);
		//			//var tempLogin = mes.Substring(0, mes.IndexOf(":"));

		//			if (item.FilesNames.Count > 0)
		//			{
		//				MakeAttachment(item.Sender, item.Reciver, item.TextMessage, item.FilesNames, sp_Messeges);
		//			}
		//			else
		//			{
		//				TextBlock ArcMessage = new TextBlock() { Text = item.Sender + ": " + item.TextMessage };
		//				ArcMessage.TextWrapping = TextWrapping.Wrap;
		//				if (item.Reciver == UIClientReciverModel.Login)
		//				{
		//					ArcMessage.HorizontalAlignment = HorizontalAlignment.Right;
		//				}
		//				else
		//				{
		//					ArcMessage.HorizontalAlignment = HorizontalAlignment.Left;
		//				}
		//				Grid.SetRow(ArcMessage, 1);
		//				sp_Messeges.Children.Add(ArcMessage);
		//			}
		//		}

		//	//Непрочитанные сообщения
		//	//Инвертировано тк. sender в контексте написания сообщения
		//	var newMessages = main.UnreadMessagesTextOnly.FindAll(l => l.UserSender.Login == UIClientReciverModel.Login);
		//	if (newMessages.Count > 0)
		//	{
		//		TextBlock m = new TextBlock() { Text = "Новые сообщения" };
		//		ArchiveMessage archive = new ArchiveMessage();
		//		Grid.SetRow(m, 1);
		//		sp_Messeges.Children.Add(m);
		//		foreach (var item in newMessages)
		//		{
		//			archive.Date = item.Date;
		//			archive.Sender = item.UserSender.Login;
		//			archive.Reciver = item.UserReciver.Login;
		//			archive.TextMessage = item.MessageText;
		//			archive.FilesNames = item.MessageContentNames;

		//			//Запись в архив
		//			WriteToArchive(archive, item.UserReciver.Login, item.UserSender.Login);

		//			// метод извещающий сервер о получении сообщения
		//			ClientCommands commands = new ClientCommands();
		//			commands.MessegesDelivered(main.STREAM, newMessages);
		//			//обновление записей непрочитанных сообщений
		//			for (int i = 0; i < main.UnreadMessagesTextOnly.Count; i++)
		//			{
		//				if (main.UnreadMessagesTextOnly[i].Id == item.Id)
		//				{
		//					main.UnreadMessagesTextOnly.RemoveAt(i);
		//				}
		//			}
		//			main.ResultStringBuilder(main.UICLients, main.UnreadMessagesTextOnly);
		//			Window.UpdateClients();


		//			if (item.MessageContentNames.Count > 0)
		//			{
		//				MakeAttachment(item.UserSender.Login, item.UserReciver.Login, item.MessageText, item.MessageContentNames, sp_Messeges);
		//			}
		//			else
		//			{
		//				TextBlock message = new TextBlock() { Text = item.UserSender.Login + ": " + item.MessageText };
		//				Grid.SetRow(message, 1);
		//				sp_Messeges.Children.Add(message);
		//			}
		//		}

		//	}
		//	ScrollDown();
		//}



		void ReadAllHotMessages()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				//Непрочитанные сообщения
				//Инвертировано тк. sender в контексте написания сообщения
				var newMessages = main.UnreadMessagesTextOnly.FindAll(l => l.UserSender.Login == Room.UIClientReciverModel.Login);
				if (newMessages.Count > 0)
				{
					ArchiveMessage archive = new ArchiveMessage();
					foreach (var item in newMessages)
					{
						archive.Date = item.Date;
						archive.Sender = item.UserSender.Login;
						archive.Reciver = item.UserReciver.Login;
						archive.TextMessage = item.MessageText;
						archive.FilesNames = item.MessageContentNames;

						//Запись в архив
						Room.WriteToArchive(archive, item.UserReciver.Login, item.UserSender.Login);

						// метод извещающий сервер о получении сообщения
						ClientCommands commands = new ClientCommands();
						commands.MessegesDelivered(main.STREAM, newMessages);
						//обновление записей непрочитанных сообщений
						for (int i = 0; i < main.UnreadMessagesTextOnly.Count; i++)
						{
							if (main.UnreadMessagesTextOnly[i].Id == item.Id)
							{
								main.UnreadMessagesTextOnly.RemoveAt(i);
							}
						}
						main.ResultStringBuilder(main.UICLients, main.UnreadMessagesTextOnly);
						UpdateClients();

						if (item.MessageContentNames.Count > 0)
						{
							Room.MakeAttachment(item.UserSender.Login, item.UserReciver.Login, item.MessageText, item.MessageContentNames, Room.sp_Messeges);
						}
						else
						{
							TextBlock message = new TextBlock() { Text = item.UserSender.Login + ": " + item.MessageText };
							Grid.SetRow(message, 1);
							Room.sp_Messeges.Children.Add(message);
						}
					}
				}
				Room.ScrollDown();
			});
		}














	}
}