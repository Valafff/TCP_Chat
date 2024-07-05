using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Models
{
	public static class com
	{
		//Система команд
		public const string CommandHelloMsr = "HelloMsr";
		public const string AnswerHelloUser = "HelloClientGoStep2";
		public const string CommandRegisterMe = "RegisterMe";
		public const string AnswerRegisterOk = "RegisterOkGoStep3";
		public const string AnswerRegisterFailed = "RegisterFailed";
		public const string CommandAuthorizeMe = "AuthorizeMe";
		public const string AnswerAuthorizationOk = "AuthorizationOkGoStep3";
		public const string AnswerAuthorizationFailed = "AuthorizationFailed";
		public const string CommandDeleteMe = "DeleteMe";
		public const string AnswerDeleteOk = "DeleteOkGoStep1";
		public const string AnswerDeleteFailed = "DeleteFailed";
		public const string CommandGetMeUsers = "GetMeUsers";
		public const string CommandGetMeActiveUsers = "GetMeActiveUsers";
		public const string AnswerCatchUsers = "CatchUsers";
		public const string AnswerCatchActiveUsers = "CatchActiveUsers";
		public const string CommandGiveMeUnReadMes = "GiveMeUnReadMes"; //Запрос непрочитанных сообщений(логин отправителя, количество, имеются или нет вложения)
		public const string AnswerCatchMessages = "CatchMessages";
		public const string CommandMessageTo = "MessageTo"; //Команда серверу - отправь сообщение такому то пользователю
		public const string AnswerMessageSendOk = "MessageSendOK";
		public const string CommandMessageDeliveredOK = "MessageDeliveredOK";
		public const string CommandGiveMeAttachments = "GiveMeAttachments";
		public const string AnswerCatchAttachments = "CathAttachments";

		public const string AnswerMessageSendFailed = "MessageSendFailed";
		public const string CommandTakeMessage = "TakeMessage"; //Команда клиенту - прими сообщение от такого то пользователя
	}
}
