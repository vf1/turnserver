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

	class TurnConnection
		: BaseConnection
		, IDisposable
	{
		public TcpPhase Phase;
		public int BytesExpected;
		private StreamBuffer buffer;

		void IDisposable.Dispose()
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
