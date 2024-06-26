using Server.BLL;
using Server.DAL.Services;
using Server.BLL.Services;

const int ServerPort = 8888;

//Server.DAL.Services.SQLLiteServiceMasseges serv = new SQLLiteServiceMasseges();
//Server.DAL.Services.SQLLiteServiceUsers serviceUsers = new SQLLiteServiceUsers();
//SlimUsersDictionatry dictionatry = new SlimUsersDictionatry();

//serv.InsertMessage(new Server.DAL.Models.DALMessageModel() { FromUserID = 100, ToUserID = 200, Date = "24.06.2024", MessageText = "Hello World!", MessageContent = "PathPathPath", IsRead = 0, IsDelivered = 0 });
//serv.UpdateMessage(new Server.DAL.Models.DALMessageModel() {Id = 5, FromUserID = 333, ToUserID = 200, Date = "24.06.2024", MessageText = "Hello World!", MessageContent = "PathPathPath", IsRead = 0, IsDelivered = 0 });
//serv.DeleteMessage(new Server.DAL.Models.DALMessageModel() { Id = 5 });
//var t = serv.GetAllMessegesReciverID(200);


//serviceUsers.InsertUser(new Server.DAL.Models.DALClientModel() { Login = "Василий777", Password = "111", FirstName = "Vasily", SecondName = "Pupkin", Status = 1, LastVisit = "24.06.2024" });
//serviceUsers.InsertUser(new Server.DAL.Models.DALClientModel() { Login = "PigPetr", Password = "222", FirstName = "Pig", SecondName = "Petr", Status = 1, LastVisit = "24.06.2024" });
//serviceUsers.InsertUser(new Server.DAL.Models.DALClientModel() { Login = "DontLoad", Password = "333", FirstName = "Не должна грузиться", SecondName = "Не должна грузиться", Status = 0, LastVisit = "24.06.2024" });
//serviceUsers.InsertUser(new Server.DAL.Models.DALClientModel() { Login = "BorisRazor", Password = "444", FirstName = "Boris", SecondName = "Boris", Status = 1, LastVisit = "24.06.2024" });
//serviceUsers.UpdateUser(new Server.DAL.Models.DALClientModel() {Id = 1, Login = "Vas777", Password = "111", FirstName = "Василий", SecondName = "Пупкин", Status = 1, LastVisit = "24.06.2024" });

//var u = serviceUsers.FindUserByLogin("Vas777");
//var u = serviceUsers.FindUserById(1);
//var t = serviceUsers.GetAllRegistredUsers();

//serviceUsers.DeleteUser(new Server.DAL.Models.DALClientModel() { Id = 1 });

//var t = serviceUsers.GetAllRegistredUsers();
//var Users = dictionatry.GetSlimUsersIdLogin();





Server.BLL.Server server = new Server.BLL.Server(ServerPort);
server.StartServer();




Console.WriteLine("OK");