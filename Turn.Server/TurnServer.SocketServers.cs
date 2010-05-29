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

		bool TurnServer_Received(ServersManager s, ref ServerAsyncEventArgs e)
		{
			if (e.LocalEndPoint.Protocol == ServerIpProtocol.Udp)
			{
				if (TurnMessage.IsTurnMessage(e.Buffer, 0, e.BytesTransferred))
					TurnServer_TurnDataReceived(ref e);
				else
					TurnServer_PeerDataReceived(ref e);
			}
			else if (e.LocalEndPoint.Protocol == ServerIpProtocol.Tcp)
			{
				if (e.ContinueBuffer() == false)
				{
					TcpPhase phase = (TcpPhase)e.UserToken1;

					if (phase == TcpPhase.WaitingFirstXpacket)
					{
						if (pseudoTlsMessage.IsBeginOfClientHello(e.Buffer, FirstXpacketLength))
						{
							e.ContinueBuffer(pseudoTlsMessage.ClientHelloLength);
							e.UserToken1 = (int)TcpPhase.WaitingClientHello;
							return true;
						}
						else
						{
							e.UserToken1 = (int)TcpPhase.WaitingTcpFrame;
						}
					}

					phase = (TcpPhase)e.UserToken1;

					if (phase == TcpPhase.WaitingClientHello)
					{
						if (pseudoTlsMessage.IsClientHello(e.Buffer))
						{
							e.SetBuffer(TcpFramingHeader.TcpFramingHeaderLength);
							e.UserToken1 = (int)TcpPhase.WaitingTcpFrame;

							var r = s.BuffersPool.Get();
							r.SetBuffer(pseudoTlsMessage.ServerHelloHelloDoneLength);
							r.ConnectionId = ServerAsyncEventArgs.AnyConnectionId;

							pseudoTlsMessage.GetServerHelloHelloDoneBytes(r.Buffer);

							s.SendAsync(r);
						}
						else
						{
							// close connection
							return false;
						}
					}
					else if (phase == TcpPhase.WaitingTcpFrame)
					{
						TcpFramingHeader tcpHeader;
						if (TcpFramingHeader.TryParse(e.Buffer, 0, out tcpHeader))
						{
							e.SetBuffer(tcpHeader.Length);
							e.UserToken1 = (int)((tcpHeader.Type == TcpFrameType.ControlMessage) ?
								TcpPhase.WaitingTurnControlMessage : TcpPhase.WaitingTurnEndToEndData);
						}
						else
						{
							// close connection
							return false;
						}
					}
					else if (phase == TcpPhase.WaitingTurnControlMessage)
					{
						TurnServer_TurnDataReceived(ref e);

						e.SetBuffer(TcpFramingHeader.TcpFramingHeaderLength);
						e.UserToken1 = (int)TcpPhase.WaitingTcpFrame;
					}
					else if (phase == TcpPhase.WaitingTurnEndToEndData)
					{
						TurnServer_PeerDataReceived(ref e);

						e.SetBuffer(TcpFramingHeader.TcpFramingHeaderLength);
						e.UserToken1 = (int)TcpPhase.WaitingTcpFrame;
					}
					else
					{
						throw new InvalidOperationException();
					}
				}
			}
			else
				throw new NotImplementedException();

			return true;
		}

		enum TcpPhase
		{
			WaitingFirstXpacket = ServerAsyncEventArgs.DefaultUserToken1,
			WaitingClientHello,
			WaitingTcpFrame,
			WaitingTurnControlMessage,
			WaitingTurnEndToEndData,
		}
	}
}
