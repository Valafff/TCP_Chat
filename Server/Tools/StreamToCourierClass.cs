using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Server.BLL.Models;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;

namespace Server.Tools
{
	public static class StreamToCourierClass
	{
		const int DefaultBufferSize = 1024;
		const int buffer10MB = 1048576;
		const int buffer100MB = 1073741824;
		public static Courier StreamToCourier(NetworkStream stream)
		{

			try
			{
				BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
				int size = reader.ReadInt32();
				byte[] bytes = reader.ReadBytes(size);
				using MemoryStream bms = new MemoryStream(bytes);
				BinaryFormatter bformatter = new BinaryFormatter();
				return (Courier)bformatter.Deserialize(bms);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message} Ошибка чтения потока на входе");
				return new Courier() { Header = "NoCommand" };
			}

			//using ProtoBuf;
			//try
			//{
			//	var ms = new MemoryStream();
			//	byte[] b = new byte[buffersize];
			//	int read;
			//	while ((read = stream.Read(b, 0, b.Length)) > 0)
			//	{
			//		if (read < buffersize)
			//		{
			//			var data = b[0..read];
			//			ms.Write(data, 0, read);
			//			break;
			//		}
			//		else
			//		{
			//			ms.Write(b, 0, b.Length);
			//		}
			//	}
			//	Console.WriteLine(ms.ToArray().Length);
			//	//Десериализация
			//	ms.Position = 0;
			//	return (Courier)Serializer.Deserialize(typeof(Courier), ms);
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex.Message);
			//	throw;
			//}


			//Старое исполнение
			//try
			//{
			//	List<byte[]> bytesArrays = new List<byte[]>();
			//	int size = buffersize;
			//	byte[] buffer = new byte[buffersize];
			//	while ((size = stream.Read(buffer, 0, buffer.Length)) > 0)
			//	{
			//		byte[] data = new byte[size];
			//		data = buffer[0..size];
			//		bytesArrays.Add(data);
			//		if (size < buffersize) { break; }
			//	}
			//	List<byte> resultArray = new List<byte>();
			//	foreach (byte[] data in bytesArrays)
			//	{
			//		resultArray.AddRange(data);
			//	}

		}
	}
}
