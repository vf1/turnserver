using System;

namespace Turn.Server
{
	struct TcpFramingHeader
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
				header = default(TcpFramingHeader);
				return false;
			}
		}

		public static void GetBytes(byte[] bytes, int offset, TcpFrameType frameType, int length)
		{
			bytes[offset + 0] = (byte)frameType;
			bytes[offset + 1] = 0;
			Array.Copy(((UInt16)length).GetBigendianBytes(), 0, bytes, offset + 2, 2);
		}
	}
}
