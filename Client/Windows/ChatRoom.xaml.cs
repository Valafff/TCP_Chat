﻿using Client.Models;
using Client.ViewModels;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors_Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
	/// Interaction logic for ChatRoom.xaml
	/// </summary>
	public partial class ChatRoom : Window
	{
		MainMenu main;
		UIClientModel UIClientReciverModel;
		string SenderLogin;
		List<string> Correspondence;
		public ChatRoom(MainMenu _main, UIClientModel _reciver, string _senderLogin)
		{
			this.main = _main;
			//!!!Получатель тот кому текущий клиент пишет письмо! При чтении сообщений он sender как бы
			UIClientReciverModel = _reciver;
			InitializeComponent();
			DataContext = main;
			Title = "Сообщение пользователю " + UIClientReciverModel.Login;
			SenderLogin = _senderLogin;
			if (main.AllMessagesList.Keys.Contains(UIClientReciverModel)) Correspondence = main.AllMessagesList[UIClientReciverModel];
			ReadAllMessages();

		}

		private void Button_FileLoad_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog attachments = new OpenFileDialog();
			attachments.Filter = "All files|*.*";
			if (attachments.ShowDialog() == true)
			{
				main.LoadingAttachments_KeyNameValuePath.Add(attachments.SafeFileName, attachments.FileName);
				TextBlock newAttachment = new TextBlock() { Text = attachments.SafeFileName };
				Grid.SetRow(newAttachment, 2);
				sp_Attachments.Children.Add(newAttachment);
			}


			////Пример работы с картинками в RichTextBox
			//OpenFileDialog ofdPicture = new OpenFileDialog();
			//ofdPicture.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";
			//ofdPicture.FilterIndex = 1;

			//if (ofdPicture.ShowDialog() == true)
			//{
			//	// Загрузите изображение из выбранного файла
			//	Image img = new Image();
			//	img.Stretch = Stretch.Fill;
			//	img.Source = new BitmapImage(new Uri(ofdPicture.FileName));
			//	rTB_MessegeTo.BeginChange();
			//	TextPointer tp = rTB_MessegeTo.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
			//	InlineUIContainer imgContainer = new InlineUIContainer(img, tp);
			//	// Вставьте изображение в RichTextBox
			//	rTB_MessegeTo.CaretPosition = imgContainer.ElementEnd;
			//	rTB_MessegeTo.EndChange();
			//}
		}

		private void Button_SendMessage_Click(object sender, RoutedEventArgs e)
		{
			string filePath = Directory.GetCurrentDirectory() + $"\\Clients\\{UIClientReciverModel.Login}\\{UIClientReciverModel.Login}.txt";
			//Если текстовый файл с сообщениями для данного клиента отсутствует - он создается. 
			if (!File.Exists(filePath))
			{
				DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\Clients\\");
				directory.CreateSubdirectory($"{UIClientReciverModel.Login}\\");
			}
			main.ArchhiveMessages.UserReciver.Login = UIClientReciverModel.Login;
			main.ArchhiveMessages.UserSender.Login = SenderLogin;
			main.ArchhiveMessages.MessageText = TB_MessegeTo.Text;
			main.ArchhiveMessages.Date = DateTime.Now;
			main.ArchhiveMessages.IsRead = 0;
			main.ArchhiveMessages.IsDelivered = 0;
			//Если файл есть - добавляется новое сообщение с датой.
			string outputMessageLog = $"{main.ArchhiveMessages.Date}\t{SenderLogin}:\t{main.ArchhiveMessages.MessageText}";
			using (var sw = new StreamWriter(filePath, true, Encoding.UTF8))
			{
				sw.WriteLine(outputMessageLog);
				sw.Flush();
			}
			main.PushMessage.Execute(main);

			TextBlock message = new TextBlock() { Text = $"{SenderLogin}:\t{main.ArchhiveMessages.MessageText}" };
			message.HorizontalAlignment = HorizontalAlignment.Right;
			if ($"{SenderLogin}:\t{main.ArchhiveMessages.MessageText}".Length > 20)
			{
				message.Width = 250;
			}
			message.TextWrapping = TextWrapping.Wrap;
			Grid.SetRow(message, 1);
			sp_Messeges.Children.Add(message);
			//main.RefreshUsers(main.UICLients);
			ScrollDown();


		}


		void ReadAllMessages()
		{
			if (Correspondence != null)
			//Сообщения из архива
			foreach (var item in Correspondence)
			{
				var mes = item.Remove(0, 19);
				var tempLogin = mes.Substring(0, mes.IndexOf(":"));
				TextBlock message = new TextBlock() { Text = mes };
				if (mes.Length > 20)
				{
					message.Width = 250;
				}
				message.TextWrapping = TextWrapping.Wrap;
				if (tempLogin.Contains(SenderLogin))
				{
					message.HorizontalAlignment = HorizontalAlignment.Right;
				}
				else
				{
					message.HorizontalAlignment = HorizontalAlignment.Left;
				}
				Grid.SetRow(message, 1);
				sp_Messeges.Children.Add(message);
			}
			//Непрочитанные сообщения
			var newMessages = main.UnreadMessagesTextOnly.FindAll(l => l.UserSender.Login == UIClientReciverModel.Login);
			if (newMessages.Count > 0)
			{
				TextBlock m = new TextBlock() { Text = "Новые сообщения" };
				Grid.SetRow(m, 1);
				sp_Messeges.Children.Add(m);
				foreach (var item in newMessages)
				{
					if (item.MessageContentNames.Count > 0)
					{

					}
					else
					{
						TextBlock message = new TextBlock() {Text = item.MessageText};
						Grid.SetRow(message, 1);
						sp_Messeges.Children.Add(message);
					}
				}
			}






			ScrollDown();
		}

		void ScrollDown()
		{
			scrollMessage.ScrollToVerticalOffset(int.MaxValue);
		}

	}
}
