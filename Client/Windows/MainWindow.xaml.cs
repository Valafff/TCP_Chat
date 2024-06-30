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

namespace Client
{

	public partial class MainWindow : Window
	{

		MainMenu main = new MainMenu();
		UserConfig user =  new UserConfig();
		ChatConfig chat = new ChatConfig();

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


			if (chat.ServerIP == null || chat.ServerPort == 0 )
			{
				OptionsWindow options = new OptionsWindow();
				options.ShowDialog();
				chat = ConfigWriteReadJson.ReadConfig<ChatConfig>("Options.json");
			}

			main.UserConfigData = user;

			main.TCPClientWork(IPAddress.Parse(chat.ServerIP), chat.ServerPort);
			Task.Run(new Action(() => main.NetworkStreamReader()));
			
			DataContext = main;
			InitializeComponent();

			bool firstAuthtorize = false;
			if (user.FirstName == null || user.SecondName == null)
			{
				firstAuthtorize = true;
				RegistrationWindow window = new RegistrationWindow(main);
				window.ShowDialog();
			}

			if(!user.AutoAuthtorization && !firstAuthtorize)
			{
				AuthtorizeWindow window = new AuthtorizeWindow(main);
				window.ShowDialog();
			}


		}

		private void MenuItem_Click_CloseChat(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuItem_Click_ReloadConnection(object sender, RoutedEventArgs e)
		{
			main.ReloadConnection();
		}

		private void MenuItem_Click_Authtorize(object sender, RoutedEventArgs e)
		{
			AuthtorizeWindow window = new AuthtorizeWindow(main);
			window.ShowDialog();
		}
	}
}