using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.Tools
{
	internal static class ByteToString
	{
		const int HeaderBytes = 4;
		public static string GetStringFromStream(Stream stream, int buffersize = 1024)
		{

			List<byte[]> request = new List<byte[]>();
			int temp;
			byte[] buffer = new byte[buffersize];


			while ((temp = stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				byte[] data = new byte[temp];
				data = buffer[0..temp];
				request.Add(data);
				if (temp < buffersize) { break; }
			}

			StringBuilder command = new StringBuilder();
			foreach (byte[] data in request)
			{
				command.Append(Encoding.UTF8.GetString(data));
			}
			//stream.Close();
			return command.ToString();
		}

		//public static async Task<string> GetStringFromStream(NetworkStream stream, int buffersize = 1024)
		//{
		//	try
		//	{
		//		int size;
		//		byte[] buffer = new byte[buffersize];
		//		//size = stream.Socket.Receive(buffer);
		//		size = await stream.ReadAsync(buffer, 0, buffer.Length);
		//		byte[] data = buffer[0..size];
		//		return Encoding.UTF8.GetString(data);

		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.Message);
		//		throw;
		//	}

		//}

		//public static string GetStringFromStream(NetworkStream stream, int buffersize = 1024)
		//{
		//	try
		//	{
		//		List<byte> bytes = new List<byte>();
		//		int bytesRead = 0;
		//		for (int i = 0; i < HeaderBytes; i++)
		//		{
		//			bytesRead = stream.ReadByte();
		//			bytes.Add((byte)bytesRead);
		//		}
		//		int ArrayLenght = Int32.Parse(Encoding.UTF8.GetString(bytes.ToArray()));
		//		bytes.Clear();
		//		for (int i = 0; i < ArrayLenght; i++)
		//		{
		//			bytesRead = stream.ReadByte();
		//			bytes.Add((byte)bytesRead);
		//		}
		//              return Encoding.UTF8.GetString(bytes.ToArray());
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine(ex.Message);
		//		throw;
		//	}

		//}

	}
}
