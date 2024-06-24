using Dapper;
using Microsoft.Data.Sqlite;
using Server.DAL.Interfaces;
using Server.DAL.Models;

namespace Server.DAL.Services
{
	internal class SQLLiteServiceUsers : IcrudUsers
	{

		private readonly SqliteConnection _db;
		public SQLLiteServiceUsers(string connectionString = "ServerDB.db")
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentException($"Проблема считывания строки подключения");
			}

			_db = new SqliteConnection(connectionString);
			Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
		}

		public IEnumerable<DALClientModel> GetAllRegistredUsers(string tableName = "Users")
		{
			_db.Open();
			var sql = $"SELECT * FROM {tableName}";
			IEnumerable<DALClientModel> result = _db.Query<DALClientModel>(sql);
			_db.Close();
			return result;
		}

		public DALClientModel FindUserByLogin(string _login, string tableName = "Users", string columnName = "Login")
		{
			_db.Open();
			var sql = $"SELECT * FROM {tableName} WHERE {columnName} = {_login}";
			DALClientModel result = _db.QuerySingle(sql);
			_db.Close();
			return result;
		}

		public DALClientModel FindUserById(int _id, string tableName = "Users", string columnName = "Id")
		{
			_db.Open();
			var sql = $"SELECT * FROM {tableName} WHERE {columnName} = {_id}";
			DALClientModel result = _db.QuerySingle(sql);
			_db.Close();
			return result;
		}

		public void InsertUser(DALClientModel _newUser)
		{
			string sqlRequest = $"INSERT INTO Users (Login, Password, FirstName, SecondName, Status, LastVisit)"+ 
                   $"VALUES ('{_newUser.Login}', '{_newUser.Password}', '{_newUser.FirstName}', '{_newUser.SecondName}', {_newUser.Status}, '{_newUser.LastVisit}')";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}

		public void UpdateUser(DALClientModel _user)
		{
			string sqlRequest = $"UPDATE Users SET  Login = N'{_user.Login}', Password = '{_user.Password}'," +
				$"FirstName = N'{_user.FirstName}', SecondName = N'{_user.SecondName}', Status = {_user.Status}, LastVisit = N'{_user.LastVisit}')"+
				$"WHERE Id = {_user.Id}";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}

		void IcrudUsers.DeleteUser(DALClientModel _user)
		{
			string sqlRequest = $"DELETE FROM Users WHERE Id = {_user.Id}";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}
	}
}
