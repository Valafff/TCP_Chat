using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.DAL.Models;

namespace Server.DAL.Interfaces
{
    internal interface IcrudUsers
    {
        IEnumerable<DALClientModel> GetAllRegistredUsers(string tableName);
		DALClientModel FindUserByLogin(string _login, string tableName, string columnName); //Получение пользователя при авторизации
		DALClientModel FindUserById(int _id, string tableName, string columnName);
        void UpdateUser(DALClientModel _user);
        void InsertUser(DALClientModel _newUser);
		void DeleteUser(DALClientModel _newUser);
	}
}
