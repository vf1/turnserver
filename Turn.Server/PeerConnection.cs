using System;
using SocketServers;

namespace Turn.Server
{
	class PeerConnection
		: BaseConnection
		, IDisposable
	{
		void IDisposable.Dispose()
		{
			base.Dispose();
		}
	}
}
