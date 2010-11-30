using System;

namespace Turn.Server
{
	class PseudoTlsMessage
	{
		private object sync = new object();
		private DateTime originDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		private Random random = new Random(Environment.TickCount);
		private byte[] randomValue = new byte[28];

		public const int ServerHelloHelloDoneLength = 83;
		public const int ClientHelloLength = 50;

		public PseudoTlsMessage()
		{
#if DEBUG
			if (clientHelloTemplate.Length != ClientHelloLength)
				throw new InvalidProgramException("Incorrect PseudoTlsMessage.ClientHelloLength");
			if (serverHelloHelloDoneTemplate.Length != ServerHelloHelloDoneLength)
				throw new InvalidProgramException("Incorrect PseudoTlsMessage.ServerHelloHelloDoneLength");
#endif
		}

		private byte[] serverHelloHelloDoneTemplate = new byte[]
		{
			0x16, 0x03, 0x01, 0x00,
			0x4E, 0x02, 0x00, 0x00,
			0x46, 0x03, 0x01,
							  0xff,		// Time Stamp (4 bytes)
			0xff, 0xff, 0xff, 
							  0xff,		// Random Value (28 bytes)
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 
							  0x20,
			0xC8, 0xD3, 0x4B, 0x01,
			0xAD, 0xA7, 0x22, 0xDB,
			0xEE, 0x30, 0x85, 0x1A,
			0xA9, 0x58, 0xA4, 0xDA,
			0x65, 0x4B, 0xE1, 0xAE,
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x18, 0x00, 0x0E,
			0x00, 0x00, 0x00, 
		};

		private byte[] clientHelloTemplate = new byte[]
		{
			0x16, 0x03, 0x01, 0x00,
			0x2D, 0x01, 0x00, 0x00,
			0x29, 0x03, 0x01, 
							  0xff,		// Time Stamp (4 bytes)
			0xff, 0xff, 0xff, 
							  0xff,		// Random Value (28 bytes)
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 
							  0x00,
			0x00, 0x02, 0x00, 0x18,
			0x01, 0x00, 
		};

		public void GetServerHelloHelloDoneBytes(byte[] bytes, int startIndex)
		{
			lock (sync)
			{
				Array.Copy(serverHelloHelloDoneTemplate, 0,
					bytes, startIndex, serverHelloHelloDoneTemplate.Length);

				Array.Copy(BitConverter.GetBytes((Int32)Math.Floor((DateTime.UtcNow - originDateTime).TotalSeconds)),
					0, bytes, startIndex + 11, 4);

				random.NextBytes(randomValue);

				Array.Copy(randomValue, 0, bytes, startIndex + 15, 28);
			}
		}

		public bool IsClientHello(byte[] bytes, int startIndex)
		{
			return IsBeginOfClientHello(bytes, startIndex, clientHelloTemplate.Length);
		}

		public bool IsBeginOfClientHello(byte[] bytes, int startIndex, int verifyLength)
		{
			for (int i = 0; i < verifyLength; i++)
			{
				if (clientHelloTemplate[i] != 0xff)
					if (clientHelloTemplate[i] != bytes[startIndex + i])
						return false;
			}

			return true;
		}
	}
}
