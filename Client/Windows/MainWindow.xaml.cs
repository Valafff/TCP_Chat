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
		UserConfig user =  new UserConfig();
		ChatConfig chat = new ChatConfig();

		public MainWindow()
		{
			try
			{
				user = ConfigWriteReadJson.ReadConfig<UserConfig>("UserConfig.json");
				chat = ConfigWriteReadJson.ReadConfig<ChatConfig>("Options.json");
			}
			catch (Exception)
			{
				ConfigWriteReadJson.ReWriteConfig(user, "UserConfig.json");
				ConfigWriteReadJson.ReWriteConfig(chat, "Options.json");
			}

			if (chat.ServerIP == null || chat.ServerPort == 0 )
			{
				OptionsWindow options = new OptionsWindow();
				options.ShowDialog();
			}

			MainMenu main = new MainMenu();

			main.TCPClientWork(IPAddress.Parse(chat.ServerIP), chat.ServerPort);
			Task.Run(new Action(() => main.NetworkStreamReader()));
			
			DataContext = main;
			InitializeComponent();

			if (user.Login == null)
			{
				RegistrationWindow reqAuth = new RegistrationWindow();
				reqAuth.ShowDialog();
			}


		}


	}
}