using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tools
{
	internal static class ByteToString
	{
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
			return command.ToString();
		}
	}
}
