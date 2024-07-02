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
using System.Reflection;
using System.CodeDom;

namespace Client.Windows
{
	public partial class RegistrationWindow : Window
	{
		MainMenu mainMenu;
		public RegistrationWindow(MainMenu mainmenu)
		{
			InitializeComponent();
			this.mainMenu = mainmenu;
			DataContext = this.mainMenu;
			mainMenu.CloseRegistrationWindowEvent += CloseMe;
		}

		private void Bt_Registration_Click(object sender, RoutedEventArgs e)
		{
			if (TbUserLogin.Text == "")
			{
				MessageBox.Show("Введите логин!");
			}
			if (TbUserPassword.Password == "")
			{
                MessageBox.Show("Введите пароль!");
			}
			else
			{
				(DataContext as MainMenu).BLLClient.Password = TbUserPassword.Password;

				mainMenu.RegistrMe.Execute(mainMenu);

				//Lambda registr = mainMenu.RegistrMe;
				//registr.Execute(this);
			}

		}
		void CloseMe()
		{
			Application.Current.Dispatcher.Invoke(() => { this.Close(); });
		}

	}
}
