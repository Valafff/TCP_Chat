using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tools
{
	static public class DataToBinaryWriter
	{
		static public void WriteData(Stream _stream, byte[] _data)
		{
			BinaryWriter writer = new BinaryWriter(_stream);
			writer.Write(_data.Length);
			writer.Write(_data);
		}
	}
}
