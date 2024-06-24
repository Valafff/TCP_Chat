using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.DAL.Models;

namespace Server.DAL.Interfaces
{
    internal interface IcrudMesseges
    {
        IEnumerable<DALMessageModel> GetAllMessegesReciverID(int _recId, string tableName, string columnName);
		IEnumerable<DALMessageModel> GetAllMessegesSenderID(int _senderId, string tableName, string columnName);
        void UpdateMessage(DALMessageModel _message);
        void InsertMessage(DALMessageModel _message);
        void DeleteMessage(DALMessageModel _message);
    }
}
