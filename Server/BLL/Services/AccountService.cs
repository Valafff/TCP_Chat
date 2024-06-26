using Server.BLL.Models;
using Server.DAL.Models;
using Server.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Services
{
    public class AccountService
    {
        DAL.Services.SQLLiteServiceUsers service = new DAL.Services.SQLLiteServiceUsers();

        public bool Register(BLLClientModel _user, Dictionary<int, string> _usersDictionary)
        {
            if (!_usersDictionary.ContainsValue(_user.Login))
            {
                try
                {
                    service.InsertUser(Mappers.BLMapper.MapClientBLLToClientDAL(_user));
                }
                catch (Exception ex)
                {
                    //Команда от сервера клиенту RegistrationFailed
                    Console.WriteLine(ex.Message);
                    return false;
                }

                //Команда от сервера клиенту RegistrationOK 
                return true;
            }
            else
            {
                //Команда от сервера клиенту RegistrationFailed
                return false;
            }
        }

        public bool Authorize(string _userLogin, string _userPassword)
        {
            try
            {
                DALClientModel originalUser = service.FindUserByLogin(_userLogin);
                if (originalUser.Password == _userPassword)
                {
                    //Команда от сервера клиенту AuthorizeOK
                    return true;
                }
                else
                {
                    //Команда от сервера клиенту AuthorizeFailed
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        //Установка метки об удалении клиента
        public bool SoftDeleting(string _userLogin, string _userPassword)
        {
            try
            {
                if (Authorize(_userLogin, _userPassword))
                {
                    var targetUser = service.FindUserByLogin(_userLogin);
                    targetUser.Status = 0;
                    service.UpdateUser(targetUser);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        //Полное удаление записи
        public bool HardDeleting(string _userLogin, string _userPassword, Dictionary<int, string> _usersDictionary)
        {
            try
            {
                if (Authorize(_userLogin, _userPassword))
                {
                    KeyValuePair<int, string> target = _usersDictionary.FirstOrDefault(u => u.Value == _userLogin);

                    service.DeleteUser(target.Key);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
