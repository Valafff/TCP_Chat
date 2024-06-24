using Server.DAL.Services;

Server.DAL.Services.SQLLiteServiceMasseges serv = new SQLLiteServiceMasseges();
//serv.InsertMessage(new Server.DAL.Models.DALMessageModel() { FromUserID = 100, ToUserID = 200, Date = "24.06.2024", MessageText = "Hello World!", MessageContent = "PathPathPath", IsRead = 0, IsDelivered = 0 });
//serv.UpdateMessage(new Server.DAL.Models.DALMessageModel() {Id = 5, FromUserID = 333, ToUserID = 200, Date = "24.06.2024", MessageText = "Hello World!", MessageContent = "PathPathPath", IsRead = 0, IsDelivered = 0 });
//serv.DeleteMessage(new Server.DAL.Models.DALMessageModel() { Id = 5 });
//var t = serv.GetAllMessegesReciverID(200);

Server.DAL.Services.SQLLiteServiceUsers serviceUsers = new SQLLiteServiceUsers();
serviceUsers.GetAllRegistredUsers


Console.WriteLine("OK");