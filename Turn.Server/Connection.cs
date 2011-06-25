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
		private StreamBuffer buffer;

		public new void Dispose()
		{
			base.Dispose();

			if (buffer != null)
				buffer.Dispose();
		}

		public StreamBuffer Buffer
		{
			get
			{
				if (buffer == null)
					buffer = new StreamBuffer();
				return buffer;
			}
		}
	}
}
