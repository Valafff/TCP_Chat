using Client.Models;
using ConfigSerializeDeserialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
		ChatConfig Config = new ChatConfig();
		public OptionsWindow()
        {
            InitializeComponent();
        }

		private void Bt_SaveConfig_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Config.ServerIP = TbServerIp.Text;
				Config.ServerPort = Int32.Parse(TbServerPort.Text);
				if (IPAddress.TryParse(Config.ServerIP, out IPAddress ip))
				{
					ConfigWriteReadJson.ReWriteConfig(Config, "Options.json");
					MessageBox.Show("Настройки сохранены");
					Close();
				}
				else
				MessageBox.Show("Введены некорректные данные!");
			}
			catch (Exception)
			{
				MessageBox.Show("Введены некорректные данные!");
			}

		}
	}
}
