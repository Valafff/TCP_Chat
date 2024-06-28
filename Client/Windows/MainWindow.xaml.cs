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

namespace Client
{

	public partial class MainWindow : Window
	{
		MainMenu main =new MainMenu();
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

			InitializeComponent();
			DataContext = main;
			if (chat.ServerIP == null || chat.ServerPort == 0 )
			{
				OptionsWindow options = new OptionsWindow();
				options.ShowDialog();
			}

			if (user.Login == null)
			{
				RegistrationWindow reqAuth = new RegistrationWindow();
				reqAuth.ShowDialog();
			}
			else
			{
			
			}

		}


	}
}