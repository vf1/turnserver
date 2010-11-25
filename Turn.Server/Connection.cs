using System;
using SocketServers;

namespace Turn.Server
{
	enum TcpPhase
	{
		WaitingFirstXpacket,
		WaitingClientHello,
		WaitingTcpFrame,
		WaitingTurnControlMessage,
		WaitingTurnEndToEndData,
	}

	class Connection
		: BaseConnection
	{
		public TcpPhase Phase;
		public int BytesExpected;
	}
}
