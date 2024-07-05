using Client.Models;
using Client.ViewModels;
using ConfigSerializeDeserialize;
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
	/// Interaction logic for ClientOptionsWindow.xaml
	/// </summary>
	public partial class ClientOptionsWindow : Window
	{
		MainMenu main;
		UserConfig user = new UserConfig();

		public ClientOptionsWindow(MainMenu mainmenu)
		{
			InitializeComponent();
			this.main = mainmenu;
			DataContext = this.main;

			main.UICLients.Clear();
			main.ActiveClients.Clear();
			main.UnreadMessagesTextOnly.Clear();
			main.AllMessagesList.Clear();
			main.UserConfigData.AutoAuthtorization = false;

			TbUserLogin.Text = main.UserConfigData.Login;
			TbUserPassword.Password = main.UserConfigData.Password;
			TbUserName.Text = main.UserConfigData.FirstName;
			TbUserSecondName.Text = main.UserConfigData.SecondName;
			ChB_AutoAuthrize.IsChecked = main.UserConfigData.AutoAuthtorization;

		}

		private void Bt_UpdateConfig_Click(object sender, RoutedEventArgs e)
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
				user.Login = TbUserLogin.Text;
				user.Password =TbUserPassword.Password;
				user.FirstName = TbUserName.Text;
				user.SecondName = TbUserSecondName.Text;
				user.AutoAuthtorization = (bool)ChB_AutoAuthrize.IsChecked;

				ConfigWriteReadJson.ReWriteConfig(user, "UserConfig.json");
				MessageBox.Show("Настройки сохранены");
			}

		}
	}

}
