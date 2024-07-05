int ServerPort = 2222;
using (StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + "ServerPort.txt"))
{
	string temp = fs.ReadLine();
	Int32.TryParse(temp, out ServerPort);	
}


Server.BLL.Server server = new Server.BLL.Server(ServerPort);
Console.WriteLine($"Сервер стартует. Порт сервера: {ServerPort}");
server.StartServer();
Console.WriteLine("Server ShutDown!");