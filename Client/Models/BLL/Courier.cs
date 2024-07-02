﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
	public class Courier
	{
		public string Header { get; set; }
		public string SenderLogin { get; set; }
		public string ReciverLogin { get; set; }
		public string MessageText { get; set; }
		public DateTime Date { get; set; }
		public int IsRead { get; set; }
		public int IsDelivered { get; set; }
		public Content Attachment { get; set; }
	}

	public class Content
	{
		public string ServiceText { get; set; }
		public byte[] Entity { get; set; }
		public Dictionary<string, byte[]> FileNameEntity { get; set; }

	}
}