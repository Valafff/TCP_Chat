using Dapper;
using Microsoft.Data.Sqlite;
using Server.DAL.Interfaces;
using Server.DAL.Models;

namespace Server.DAL.Services
{
	internal class SQLLiteServiceMasseges : IcrudMesseges
	{
		private readonly SqliteConnection _db;
		//public SQLLiteServiceMasseges(string connectionString = "ServerDB.db")
		public SQLLiteServiceMasseges(string connectionString = "Data Source=ServerDB.db;")
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentException($"Проблема считывания строки подключения");
			}
			_db = new SqliteConnection(connectionString);
			Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
		}



		public IEnumerable<DALMessageModel> GetAllMessegesReciverID(int _recId, string tableName = "Messages", string columnName = "ToUserID")
		{
			_db.Open();
			var sql = $"SELECT * FROM {tableName} WHERE {columnName} = {_recId}";
			IEnumerable<DALMessageModel> result = _db.Query<DALMessageModel>(sql);
			_db.Close();
			return result;
		}

		public IEnumerable<DALMessageModel> GetAllMessegesSenderID(int _senderId, string tableName = "Messages", string columnName = "FromUserID")
		{
			_db.Open();
			var sql = $"SELECT * FROM {tableName}";
			IEnumerable<DALMessageModel> result = _db.Query<DALMessageModel>(sql);
			_db.Close();
			return result;
		}

		public void InsertMessage(DALMessageModel _message)
		{
			string sqlRequest = $"INSERT INTO Messages (FromUserID, ToUserID, Date, MessageText, MessageContent, IsRead, IsDelivered)" +
			$"VALUES ({_message.FromUserID}, {_message.ToUserID}, '{_message.Date}', '{_message.MessageText}', '{_message.MessageContent}', {_message.IsRead}, {_message.IsDelivered})";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}

		public void UpdateMessage(DALMessageModel _message)
		{
			string sqlRequest = $"UPDATE Messages SET FromUserID = {_message.FromUserID}, ToUserID = {_message.ToUserID}," +
				$"Date = '{_message.Date}', MessageText = '{_message.MessageText}', MessageContent = '{_message.MessageContent}', IsRead = {_message.IsRead}, IsDelivered = {_message.IsDelivered} "+
				$"WHERE  Id = {_message.Id}";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}

		public void DeleteMessage(DALMessageModel _message)
		{
			string sqlRequest = $"DELETE FROM Messages WHERE Id = {_message.Id}";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}
	}	
}
