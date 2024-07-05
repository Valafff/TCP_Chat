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
		MainMenu main;
		public RegistrationWindow(MainMenu mainmenu)
		{
			InitializeComponent();
			this.main = mainmenu;
			DataContext = this.main;
			main.CloseRegistrationWindowEvent += CloseMe;

			main.UICLients.Clear();
			main.ActiveClients.Clear();
			main.UnreadMessagesTextOnly.Clear();
			main.AllMessagesList.Clear();
			main.UserConfigData.AutoAuthtorization = false;
		}

		private void Bt_Registration_Click(object sender, RoutedEventArgs e)
		{
			if (TbUserLogin.Text == "")
			{
				MessageBox.Show("Введите логин!");
			}
			else if (TbUserLogin.Text.Contains(" "))
			{
				MessageBox.Show("Несоответствующий формат логина. Запрещены символы: +=[]:*?;«,./\\<>|'пробел'");
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
				main.RegistrMe.Execute(main);
			}

		}
		void CloseMe()
		{
			Application.Current.Dispatcher.Invoke(() => { this.Close(); });
		}

	}
}
