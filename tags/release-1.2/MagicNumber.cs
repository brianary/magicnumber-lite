using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;

namespace MagicNumberLite
{
	public class DataType
	{
		public readonly string Extension;

		public readonly string MimeType;

		public readonly string Name;

		public DataType(string ext, string mime, string name)
		{
			Extension = ext;
			MimeType = mime;
			Name = name;
		}
	}

	public class Match : DataType
	{
		public byte[] Bytes;

		public Match(byte[] bytes, string ext, string mime, string name)
			: base(ext, mime, name)
		{
			Bytes = bytes;
		}

		public bool Matches(byte[] data)
		{
			if (Bytes == null && data == null) return true;
			if (Bytes == null || data == null) return false;
			for (int i = 0; i < Bytes.Length; i++)
				if (Bytes[i] != data[i])
					return false;
			return true;
		}
	}

	public class Inspector
	{
		Dictionary<long, Dictionary<byte, Match[]>> Matches;

		public Inspector()
		{
			Matches = new Dictionary<long, Dictionary<byte, Match[]>>();
			XmlDocument xml = new XmlDocument();
			xml.Load(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Inspector)).Location),
				ConfigurationManager.AppSettings["MagicNumberLiteXml"]));
			foreach (XmlElement offset in xml.DocumentElement.ChildNodes)
			{
				Dictionary<byte, Match[]> bundle = new Dictionary<byte, Match[]>();
				foreach (XmlElement bundlenode in offset.ChildNodes)
				{
					List<Match> matches = new List<Match>();
					foreach (XmlElement matchnode in bundlenode.ChildNodes)
					{
						string[] nibpairs = matchnode.GetAttribute("bytes").Split(null);
						byte[] bytes = new byte[nibpairs.Length];
						for (byte i = 0; i < nibpairs.Length; i++)
							bytes[i] = Byte.Parse(nibpairs[i], System.Globalization.NumberStyles.HexNumber);
						matches.Add(new Match(bytes, matchnode.GetAttribute("ext"),
							matchnode.GetAttribute("mime"), matchnode.GetAttribute("name")));
					}
					bundle.Add(Byte.Parse(bundlenode.GetAttribute("byte"),
						System.Globalization.NumberStyles.HexNumber), matches.ToArray());
				}
				Matches.Add(Int32.Parse(offset.GetAttribute("at")), bundle);
			}
		}

		public DataType GetDataType(byte[] data)
		{
			return GetDataType(new BinaryReader(new MemoryStream(data)));
		}

		public DataType GetDataType(Stream data)
		{
			return GetDataType(new BinaryReader(data));
		}

		public DataType GetDataType(BinaryReader data)
		{
			foreach (long offset in Matches.Keys)
			{
				data.BaseStream.Seek(offset, SeekOrigin.Begin);
				byte[] databytes = new byte[64];
				data.BaseStream.Seek(offset, SeekOrigin.Begin);
				data.Read(databytes, 0, databytes.Length);
				if (Matches[offset].ContainsKey(databytes[0]))
					foreach (Match match in Matches[offset][databytes[0]])
						if (match.Matches(databytes)) return match as DataType;
			}
			return new DataType("","application/octet-stream","Unknown");
		}
	}
}
