using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tools
{
	 internal static class StreamToByte
	{
		public static byte[] StreamToByteArr(Stream stream, int buffersize = 1024)
		{
			List<byte[]> bytesArrays = new List<byte[]>();
			int size;
			byte[] buffer = new byte[buffersize];
			while ((size = stream.Read(buffer, 0, buffer.Length)) > 0)
			{
				byte[] data = new byte[size];
				data = buffer[0..size];
				bytesArrays.Add(data);
				if (size < buffersize) { break; }
			}
			List<byte> resultArray = new List<byte>();
			foreach (byte[] data in bytesArrays)
			{
				resultArray.AddRange(data);
			}			
			return resultArray.ToArray();
		}
	}
}
