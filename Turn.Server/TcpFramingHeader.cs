using System;

namespace Turn.Server
{
	class TcpFramingHeader
	{
		public const int TcpFramingHeaderLength = 4;

		public TcpFrameType Type { get; set; }
		public UInt16 Length { get; set; }

		public static bool TryParse(byte[] bytes, int offset, out TcpFramingHeader header)
		{
			try
			{
				header = new TcpFramingHeader();

				header.Type = (TcpFrameType)bytes[offset + 0];
				header.Length = bytes.BigendianToUInt16(offset + 2);

				return true;
			}
			catch
			{
				header = null;
				return false;
			}
		}

		public static void GetBytes(byte[] bytes, TcpFrameType frameType, int length)
		{
			bytes[0] = (byte)frameType;
			bytes[1] = 0;
			Array.Copy(((UInt16)length).GetBigendianBytes(), 0, bytes, 2, 2);
		}
	}
}
