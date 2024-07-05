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


		private void MenuItem_Click_Authtorize(object sender, RoutedEventArgs e)
		{
			AuthtorizeWindow window = new AuthtorizeWindow(main);
			window.ShowDialog();
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


		private void MenuItem_ConnectOptions_Click(object sender, RoutedEventArgs e)
		{
			OptionsWindow options = new OptionsWindow();
			options.ShowDialog();
		}

		private void MenuItem_ClientOptions_Click(object sender, RoutedEventArgs e)
		{
			ClientOptionsWindow optionsWindow = new ClientOptionsWindow(main);
			optionsWindow.ShowDialog();
		}
	}
}