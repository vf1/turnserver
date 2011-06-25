using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using SocketServers;
using Turn.Message;

namespace Turn.Server
{
	partial class TurnServer
	{
		private PseudoTlsMessage pseudoTlsMessage = new PseudoTlsMessage();
		private const int FirstXpacketLength = TcpFramingHeader.TcpFramingHeaderLength;

		private void TurnServer_NewConnection(ServersManager<Connection> s, Connection c)
		{
			c.Phase = TcpPhase.WaitingFirstXpacket;
			c.BytesExpected = FirstXpacketLength;
		}

		private bool TurnServer_Received(ServersManager<Connection> s, Connection c, ref ServerAsyncEventArgs e)
		{
			if (e.LocalEndPoint.Protocol == ServerProtocol.Udp)
			{
				if (TurnMessage.IsTurnMessage(e.Buffer, e.Offset, e.BytesTransferred))
					TurnServer_TurnDataReceived(ref e);
				else
					TurnServer_PeerDataReceived(ref e);
			}
			else if (e.LocalEndPoint.Protocol == ServerProtocol.Tcp)
			{
				if (c.Buffer.IsValid)
				{
					c.Buffer.Resize(Math.Max(4096, c.BytesExpected));

					if (c.Buffer.CopyTransferredFrom(e, 0) == false)
						return false;

					if (c.Buffer.Count < c.BytesExpected)
						return true;
				}
				else
				{
					if (e.BytesTransferred < c.BytesExpected)
						return c.Buffer.CopyTransferredFrom(e, 0);
				}

				int proccessed = 0;

				for (; ; )
				{
					if (c.Buffer.IsValid)
					{
						if (e == null)
						{
							e = s.BuffersPool.Get();
							e.CopyAddressesFrom(c);
						}

						e.AttachBuffer(c.Buffer);
						proccessed = 0;
					}

					if (e.BytesTransferred - proccessed < c.BytesExpected)
						return c.Buffer.CopyTransferredFrom(e, proccessed);

					switch (c.Phase)
					{
						case TcpPhase.WaitingFirstXpacket:

							if (pseudoTlsMessage.IsBeginOfClientHello(e.Buffer, e.Offset, FirstXpacketLength))
							{
								c.Phase = TcpPhase.WaitingClientHello;
								c.BytesExpected = PseudoTlsMessage.ClientHelloLength;
							}
							else
							{
								c.Phase = TcpPhase.WaitingTcpFrame;
								c.BytesExpected = TcpFramingHeader.TcpFramingHeaderLength;

								if (FirstXpacketLength <= TcpFramingHeader.TcpFramingHeaderLength)
									goto case TcpPhase.WaitingTcpFrame;
							}
							break;


						case TcpPhase.WaitingClientHello:

							if (pseudoTlsMessage.IsClientHello(e.Buffer, e.Offset) == false)
								return false;

							var x = s.BuffersPool.Get();

							x.CopyAddressesFrom(e);
							x.Count = PseudoTlsMessage.ServerHelloHelloDoneLength;

							pseudoTlsMessage.GetServerHelloHelloDoneBytes(x.Buffer, x.Offset);

							s.SendAsync(x);

							proccessed += c.BytesExpected;
							c.Phase = TcpPhase.WaitingTcpFrame;
							c.BytesExpected = TcpFramingHeader.TcpFramingHeaderLength;

							break;


						case TcpPhase.WaitingTcpFrame:

							TcpFramingHeader tcpHeader;
							if (TcpFramingHeader.TryParse(e.Buffer, e.Offset, out tcpHeader) == false)
								return false;

							proccessed += c.BytesExpected;
							c.Phase = (tcpHeader.Type == TcpFrameType.ControlMessage) ?
								TcpPhase.WaitingTurnControlMessage : TcpPhase.WaitingTurnEndToEndData;
							c.BytesExpected = tcpHeader.Length;

							break;



						case TcpPhase.WaitingTurnEndToEndData:
						case TcpPhase.WaitingTurnControlMessage:

							if (e.BytesTransferred - proccessed < c.BytesExpected)
								if (c.Buffer.CopyTransferredFrom(e, proccessed + c.BytesExpected) == false)
									return false;

							e.Count -= proccessed;
							e.Offset += proccessed;
							e.BytesTransferred = c.BytesExpected;

							if (c.Phase == TcpPhase.WaitingTurnEndToEndData)
								TurnServer_PeerDataReceived(ref e);
							else
								TurnServer_TurnDataReceived(ref e);

							proccessed = e.BytesTransferred;

							c.Phase = TcpPhase.WaitingTcpFrame;
							c.BytesExpected = TcpFramingHeader.TcpFramingHeaderLength;

							break;


						default:
							throw new NotImplementedException();
					}
				}
			}
			else
				throw new NotImplementedException();

			return true;
		}
	}
}
