using Dapper;
using Microsoft.Data.Sqlite;
using Server.DAL.Interfaces;
using Server.DAL.Models;

namespace Server.DAL.Services
{
	internal class SQLLiteServiceUsers : IcrudUsers
	{

		private readonly SqliteConnection _db;
		public SQLLiteServiceUsers(string connectionString = "Data Source=ServerDB.db;")
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
			var sql = $"SELECT * FROM {tableName} WHERE Status = 1"; //0 удален
			IEnumerable<DALClientModel> result = _db.Query<DALClientModel>(sql);
			_db.Close();
			return result;
		}

		public DALClientModel FindUserByLogin(string _login, string tableName = "Users", string columnName = "Login")
		{
			_db.Open();
			var sql = $"SELECT * FROM {tableName} WHERE {columnName} = '{_login}'";
			var result = _db.QueryFirst<DALClientModel>(sql);
			_db.Close();
			return result;
		}

		public DALClientModel FindUserById(int _id, string tableName = "Users", string columnName = "Id")
		{
			_db.Open();
			var sql = $"SELECT * FROM {tableName} WHERE {columnName} = {_id}";
			DALClientModel result = _db.QueryFirst<DALClientModel>(sql);
			_db.Close();
			return result;
		}

		public void InsertUser(DALClientModel _newUser)
		{
			string sqlRequest = $"INSERT INTO Users (Login, Password, FirstName, SecondName, Status, LastVisit)" +
				   $"VALUES ('{_newUser.Login}', '{_newUser.Password}', '{_newUser.FirstName}', '{_newUser.SecondName}', {_newUser.Status}, '{_newUser.LastVisit}')";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}

		public void UpdateUser(DALClientModel _user)
		{
			string sqlRequest = $"UPDATE Users SET  Login = '{_user.Login}', Password = '{_user.Password}'," +
				$"FirstName = '{_user.FirstName}', SecondName = '{_user.SecondName}', Status = {_user.Status}, LastVisit = '{_user.LastVisit}' " +
				$"WHERE Id = {_user.Id}";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}

		public void DeleteUser(DALClientModel _user)
		{
			string sqlRequest = $"DELETE FROM Users WHERE Id = {_user.Id}";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}

		public void DeleteUser(int Id)
		{
			string sqlRequest = $"DELETE FROM Users WHERE Id = {Id}";
			_db.Open();
			_db.Execute(sqlRequest);
			_db.Close();
		}
	}
}
