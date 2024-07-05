using Client.Models;
using Client.ViewModels;
using ProtoBuf;
using Server.BLL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Server.BLL.Services;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics.Metrics;


namespace Client.Services
{
	public class ClientCommands
	{
		public void HelloServer(Stream _stream)
		{
			var data = CourierServices.Packer(com.CommandHelloMsr);
			Server.Tools.DataToBinaryWriter.WriteData(_stream, data);


			//BinaryWriter writer = new BinaryWriter(_stream);
			//writer.Write(data.Length);
			//writer.Write(data);

			//_stream.Write(data);
		}

		public void AutoAuthtoeize(BLLClientModel _clientmodel, MainMenu _main, UserConfig _userConfig)
		{
			_clientmodel.Login = _userConfig.Login;
			_clientmodel.Password = _userConfig.Password;
			_main.Authtorizeme.Execute(this);
		}

		public void RequesRegistredClients(Stream _stream)
		{
			var buffer = CourierServices.Packer(com.CommandGetMeUsers);
			Server.Tools.DataToBinaryWriter.WriteData(_stream, buffer);

		}

		public List<string> ReadRegistredClients(Courier _courier)
		{
			try
			{
				return JsonSerializer.Deserialize<List<string>>(_courier.MessageText);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		public void RequestActiveUsers(Stream _stream)
		{
			var buffer = CourierServices.Packer(com.CommandGetMeActiveUsers);
			Server.Tools.DataToBinaryWriter.WriteData(_stream, buffer);
		}

		public void RequestUnreadMessages(Stream _stream)
		{
			var buffer = CourierServices.Packer(com.CommandGiveMeUnReadMes);
			Server.Tools.DataToBinaryWriter.WriteData(_stream, buffer);
		}

		public List<string> ReadActiveClients(Courier _courier)
		{
			try
			{
				return JsonSerializer.Deserialize<List<string>>(_courier.MessageText);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		public List<BLLMessageModel> ReadMessagesTextOnly(Courier _courier)
		{
			return JsonSerializer.Deserialize<List<BLLMessageModel>>(_courier.MessageText);
		}

		public void MessegesDelivered(Stream _stream, List<BLLMessageModel> _readedMessages)
		{
			Courier courier = new Courier() { Header = com.CommandMessageDeliveredOK, MessageText = JsonSerializer.Serialize(_readedMessages) };
			var buffer = Server.BLL.Services.CourierServices.Packer(courier);
			Server.Tools.DataToBinaryWriter.WriteData(_stream, buffer);
		}

		public string GiveMeAttachments(Stream _stream, string _attachments)
		{
			string[] names = _attachments.Split(':', StringSplitOptions.RemoveEmptyEntries);
			string savePath = "";

			var dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = true;
			CommonFileDialogResult result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok)
			{
				savePath = dialog.FileName;
			}

			Courier courier = new Courier() { Header = com.CommandGiveMeAttachments, MessageText = JsonSerializer.Serialize(names) };
			var buffer = Server.BLL.Services.CourierServices.Packer(courier);
			Server.Tools.DataToBinaryWriter.WriteData(_stream, buffer);

			return savePath;
		}
		public void SaveAttachments(string _directoryPath, Courier _courier)
		{
			foreach (var file in _courier.Attachments)
			{
				using (FileStream fs = new FileStream(_directoryPath + "\\" + file.Key, FileMode.OpenOrCreate))
				{
					fs.Write(file.Value);
				}
			}
		}

	}
}
