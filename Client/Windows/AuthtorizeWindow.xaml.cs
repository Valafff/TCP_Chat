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
		MainMenu mainMenu;
		public AuthtorizeWindow(MainMenu _main )
		{
			InitializeComponent();
			this.mainMenu = _main;
			DataContext = this.mainMenu;
			mainMenu.CloseAuthWindowEvent += CloseMe;
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
				(DataContext as MainMenu).BLLClient.Password = TbUserPassword.Password;
				Lambda Auth = mainMenu.Authtorizeme;
				Auth.Execute(this);
			}
		}
		void CloseMe()
		{
			Application.Current.Dispatcher.Invoke(() => { this.Close(); });
		}
	}
}
