using System;
using System.IO;
using System.Xml;
using System.Net;

namespace Service
{
	class Settings
	{
		private string pathName;
		private IPAddress realIp;

		public Settings(string pathName)
		{
			this.pathName = pathName;
		}

		public const string SettingsFileName = @"OfficeSIP.Turn.Server.Settings.xml";

		public void Load()
		{
			XmlDocument doc = null;

			try
			{
				doc = new XmlDocument();
				doc.Load(pathName);
			}
			catch
			{
				doc = null;
			}

			Load(doc);
		}

		public void Save()
		{
			var writer = XmlWriter.Create(pathName,
				new XmlWriterSettings() { Indent = true, IndentChars = @"    " });

			writer.WriteStartElement(@"settings");

			writer.WriteElementString(@"publicIp", PublicIp.ToString());
			writer.WriteElementString(@"turnUdpPort", TurnUdpPort.ToString());
			writer.WriteElementString(@"turnTcpPort", TurnTcpPort.ToString());
			writer.WriteElementString(@"turnTlsPort", TurnTlsPort.ToString());
			writer.WriteElementString(@"minPort", MinPort.ToString());
			writer.WriteElementString(@"maxPort", MaxPort.ToString());
			writer.WriteElementString(@"realm", Realm);
			writer.WriteElementString(@"key1", Convert.ToBase64String(Key1));
			writer.WriteElementString(@"key2", Convert.ToBase64String(Key2));
			writer.WriteElementString(@"adminName", AdminName);
			writer.WriteElementString(@"adminPass", AdminPass);

			writer.WriteEndElement();

			writer.Flush();
			writer.Close();
		}

		public void Reset()
		{
			File.Delete(pathName);
			Load();
		}

		public IPAddress PublicIp { get; set; }
		public int TurnUdpPort { get; set; }
		public int TurnTcpPort { get; set; }
		public int TurnTlsPort { get; set; }
		public int MinPort { get; set; }
		public int MaxPort { get; set; }
		public string Realm { get; set; }
		public byte[] Key1 { get; set; }
		public byte[] Key2 { get; set; }
		public string AdminName { get; set; }
		public string AdminPass { get; set; }

		public IPAddress RealIp
		{
			get
			{
				if (realIp != IPAddress.None)
					return realIp;
				return PublicIp;
			}
			set { realIp = value; }
		}

		protected void Load(XmlDocument doc)
		{
			PublicIp = GetSetting(doc, @"publicIp", IPAddress.Loopback);
			RealIp = GetSetting(doc, @"realIp", IPAddress.None);
			TurnUdpPort = GetSetting(doc, @"turnUdpPort", 3478);
			TurnTcpPort = GetSetting(doc, @"turnTcpPort", 3478);
			TurnTlsPort = GetSetting(doc, @"turnTlsPort", 443);
			MinPort = GetSetting(doc, @"minPort", 40000);
			MaxPort = GetSetting(doc, @"maxPort", 50000);
			Realm = GetSetting(doc, @"realm", @"officesip.local");
			Key1 = GetSetting(doc, @"key1", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, });
			Key2 = GetSetting(doc, @"key2", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, });
			AdminName = GetSetting(doc, @"adminName", @"administrator");
			AdminPass = GetSetting(doc, @"adminPass", @"");
		}

		protected static XmlNode SelectSingleNode(XmlDocument doc, string name)
		{
			if (doc != null)
				return doc.SelectSingleNode(@"settings/" + name);
			return null;
		}

		protected static string GetSetting(XmlDocument doc, string name, string defaultValue)
		{
			var node = SelectSingleNode(doc, name);
			if (node != null)
				return node.InnerText;
			return defaultValue;
		}

		protected static int GetSetting(XmlDocument doc, string name, int defaultValue)
		{
			try
			{
				var node = SelectSingleNode(doc, name);
				if (node != null)
					return int.Parse(node.InnerText);
			}
			catch
			{
			}

			return defaultValue;
		}

		protected static byte[] GetSetting(XmlDocument doc, string name, byte[] defaultValue)
		{
			try
			{
				var node = SelectSingleNode(doc, name);
				if (node != null)
					return Convert.FromBase64String(node.InnerText);
			}
			catch
			{
			}

			return defaultValue;
		}

		protected static IPAddress GetSetting(XmlDocument doc, string name, IPAddress defaultValue)
		{
			try
			{
				var node = SelectSingleNode(doc, name);
				if (node != null)
					return IPAddress.Parse(node.InnerText);
			}
			catch
			{
			}

			return defaultValue;
		}
	}
}
