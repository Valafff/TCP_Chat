using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BLL.Services
{
    static public class ClientDirectoryCreation
    {
        static public bool AddClientDirectory(string _login)
        {
            try
            {
                string currDir = Directory.GetCurrentDirectory();
                currDir += $"\\Clients\\{_login}";
                Directory.CreateDirectory(currDir);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine($"Ошибка создания директории для пользователя {_login}");
                return false;
            }

        }
    }
}
