using Client.ViewModels;
using Microsoft.Xaml.Behaviors_Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client.Windows
{
	/// <summary>
	/// Interaction logic for AuthtorizeWindow.xaml
	/// </summary>
	public partial class AuthtorizeWindow : Window
	{
		MainMenu main;
		public AuthtorizeWindow(MainMenu _main )
		{
			InitializeComponent();
			this.main = _main;
			DataContext = this.main;
			main.CloseAuthWindowEvent += CloseMe;
			TbUserLogin.Text = "";
			TbUserPassword.Password = "";

			main.UICLients.Clear();
			main.ActiveClients.Clear();
			main.UnreadMessagesTextOnly.Clear();
			main.AllMessagesList.Clear();
			main.UserConfigData.AutoAuthtorization = false;
		}

		private void Bt_Authtorize_Click(object sender, RoutedEventArgs e)
		{
			if (TbUserLogin.Text == "")
			{
				MessageBox.Show("Введите логин!");
			}
			else if (TbUserPassword.Password == "")
			{
				MessageBox.Show("Введите пароль!");
			}
			else
			{
				main.Disconnect();
				main.ReloadConnection();
				(DataContext as MainMenu).BLLClient.Password = TbUserPassword.Password;
				main.Authtorizeme.Execute(main);
			}
		}
		void CloseMe()
		{
			Application.Current.Dispatcher.Invoke(() => { this.Close(); });
		}
	}
}
