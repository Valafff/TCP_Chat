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

namespace Client.Services
{
	public class ClientCommands
	{
		//Система команд
		const string CommandHelloMsr = "HelloMsr";
		const string AnswerHelloUser = "HelloClientGoStep2";
		const string CommandRegisterMe = "RegisterMe";
		const string AnswerRegisterOk = "RegisterOkGoStep3";
		const string AnswerRegisterFailed = "RegisterFailed";
		const string CommandAuthorizeMe = "AuthorizeMe";
		const string AnswerAuthorizationOk = "AuthorizationOkGoStep3";
		const string AnswerAuthorizationFailed = "AuthorizationFailed";
		const string CommandDeleteMe = "DeleteMe";
		const string AnswerDeleteOk = "DeleteOkGoStep1";
		const string AnswerDeleteFailed = "DeleteFailed";
		const string CommandGetMeUsers = "GetMeUsers";
		const string CommandGetMeActiveUsers = "GetMeActiveUsers";
		const string AnswerCatchUsers = "CatchUsers";
		const string AnswerCatchActiveUsers = "CatchActiveUsers";
		const string CommandGiveMeUnReadMes = "GiveMeUnReadMes";
		const string AnswerCatchMessages = "CatchMessages";
		const string CommandMessageTo = "MessageTo"; //Команда серверу - отправь сообщение такому то пользователю
		const string AnswerMessageSendOk = "MessageSendOK";
		const string AnswerMessageSendFailed = "MessageSendFailed";
		const string CommandTakeMessage = "TakeMessage"; //Команда клиенту - прими сообщение от такого то пользователя

		public void HelloServer(Stream _stream)
		{
			var data = Server.BLL.Services.CourierServices.Packer(CommandHelloMsr);
			_stream.Write(data);
		}

		public void AutoAuthtoeize(BLLClientModel _clientmodel, MainMenu _main, UserConfig _userConfig)
		{
			_clientmodel.Login = _userConfig.Login;
			_clientmodel.Password = _userConfig.Password;
			_main.Authtorizeme.Execute(this);
		}

		public void RequesRegistredClients(Stream _stream)
		{
			Courier courier = new Courier() { Header = CommandGetMeUsers };
			using var ms = new MemoryStream();
			Serializer.Serialize(ms, courier);
			var buffer = ms.ToArray();
			_stream.Write(buffer, 0, buffer.Length);
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
			Courier courier = new Courier() { Header = CommandGetMeActiveUsers };
			using var ms = new MemoryStream();
			Serializer.Serialize(ms, courier);
			var buffer = ms.ToArray();
			_stream.Write(buffer, 0, buffer.Length);
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

	}
}
