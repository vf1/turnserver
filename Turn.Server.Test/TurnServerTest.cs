using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using NUnit.Framework;
using Turn.Server;
using Turn.Message;

namespace TestTurnServer
{
	[TestFixture()]
	public class TurnServerTest
	{
		private TurnServer turnServer;
		private IPAddress turnIp = IPAddress.Parse(@"127.0.0.1");
		private const int udpPort = 1100;
		private const int tcpPort = 1101;
		private const int tlsPort = 1102;
		private IPAddress publicIp = IPAddress.Parse(@"127.0.0.1");
		private const int publicMinPort = 2000;
		private const int publicMaxPort = 3000;
		private const string realm = @"officesip.local";
		private byte[] key1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		private byte[] key2 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

		[SetUp()]
		public void CreateTurnServer()
		{
			turnServer = new Turn.Server.TurnServer(null)
			{
				TurnUdpPort = udpPort,
				TurnTcpPort = tcpPort,
				TurnPseudoTlsPort = tlsPort,
				PublicIp = publicIp,
				MinPort = publicMinPort,
				MaxPort = publicMaxPort,
				Authentificater = new Turn.Server.Authentificater()
				{
					Realm = realm,
					Key1 = key1,
					Key2 = key2,
				},
			};

			turnServer.Start();
		}

		[TearDown()]
		public void CloseTurnServer()
		{
			turnServer.Stop();
			turnServer = null;
		}

		[Test()]
		public void DoubleTcpAllocationTest()
		{
			var msUsername = new MsUsername()
			{
				Value = new byte[52],
			};

			using (HMACSHA1 sha1 = new HMACSHA1(key1))
			{
				sha1.ComputeHash(msUsername.Value, 0, msUsername.TokenBlobLength);
				Array.Copy(sha1.Hash, 0, msUsername.Value, msUsername.TokenBlobLength, MsUsername.HashOfTokenBlobLength);
			}

			var allocate1 = new TurnMessage()
			{
				MessageType = MessageType.AllocateRequest,
				TransactionId = TransactionId.Generate(),

				MagicCookie = new MagicCookie(),
				MsVersion = new MsVersion()
				{
					Value = 1,
				},
				MsUsername = msUsername,
			};

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect(turnIp, tcpPort);

			var allocate1response = SendReceiveMessage(socket, allocate1);

			Assert.AreEqual(MessageType.AllocateErrorResponse, allocate1response.MessageType);
			Assert.AreEqual(401, allocate1response.ErrorCodeAttribute.ErrorCode);

			var allocate2 = new TurnMessage()
			{
				MessageType = MessageType.AllocateRequest,
				TransactionId = TransactionId.Generate(),

				MagicCookie = new MagicCookie(),
				MsVersion = new MsVersion()
				{
					Value = 1,
				},
				MsUsername = msUsername,
				Nonce = allocate1response.Nonce,
				Realm = new Realm(TurnMessageRfc.MsTurn)
				{
					Value = realm,
				},
				MessageIntegrity = new MessageIntegrity(),
			};

			var allocate2response = SendReceiveMessage(socket, allocate2);

			Assert.AreEqual(MessageType.AllocateResponse, allocate2response.MessageType);

			var allocate3response = SendReceiveMessage(socket, allocate2);

			Assert.AreEqual(MessageType.AllocateResponse, allocate3response.MessageType);
		}

		private TurnMessage SendReceiveMessage(Socket socket, TurnMessage message)
		{
			SendMessage(socket, message);
			return ReceiveMessage(socket);
		}

		private void SendMessage(Socket socket, TurnMessage message)
		{
			byte[] buffer1 = new byte[4096];

			message.ComputeMessageLength();
			message.GetBytes(buffer1, TcpFramingHeader.TcpFramingHeaderLength, key2);

			TcpFramingHeader.GetBytes(buffer1, 0, TcpFrameType.ControlMessage, message.TotalMessageLength);

			int size = TcpFramingHeader.TcpFramingHeaderLength + message.TotalMessageLength;
			if (socket.Send(buffer1, size, SocketFlags.None) != size)
				throw new Exception("Send failed!");
		}

		private TurnMessage ReceiveMessage(Socket socket)
		{
			byte[] buffer1 = new byte[4096];
			socket.ReceiveTimeout = 10000;
			int length1 = socket.Receive(buffer1);

			return TurnMessage.Parse(buffer1, TcpFramingHeader.TcpFramingHeaderLength, length1 - TcpFramingHeader.TcpFramingHeaderLength, TurnMessageRfc.MsTurn);
		}
	}
}
