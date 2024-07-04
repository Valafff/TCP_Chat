using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services
{
	static public class ClientDirectoryCreation
	{
		static public bool AddClientDirectory(string _login)
		{
			try
			{
				//string currDir = Directory.GetCurrentDirectory();
				string currDir = Directory.GetCurrentDirectory() + $"\\Clients\\{_login}" ;
				if (!Directory.Exists(currDir))
				{
					Directory.CreateDirectory(currDir);
					return true;
				}
				return false;

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
