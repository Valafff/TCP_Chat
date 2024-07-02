using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tools
{
	internal static class StreamToByte
	{
		const int buffer10MB = 1048576;
		const int buffer100MB = 1073741824;
		public static byte[] StreamToByteArr(Stream stream, int buffersize = buffer100MB)
		{
			try
			{
				List<byte[]> bytesArrays = new List<byte[]>();
				int size = buffersize;
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
					Console.WriteLine(resultArray.Count);
				}

				return resultArray.ToArray();
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex.Message);
                throw;
			}
			
		}
	}
}
