using System;
using System.Net;
using System.Net.Sockets;
using Turn.Message;
using SocketServers;

namespace Turn.Server
{
	class Allocation
	{
		private int created;
		private int lifetime;
		private IPEndPoint reflexEndPoint = new IPEndPoint(IPAddress.Any, 0);
		private IPEndPoint activeDestination = new IPEndPoint(IPAddress.Any, 0);

		public Allocation()
		{
			Permissions = new PermissionsList();
		}

		public TransactionId TransactionId { get; set; }
		public ConnectionId ConnectionId { get; set; }

		// для dataindication, а можно ли использовать другое имя?
		//public Turn.Message.MsUsername Username { get; set; }

		public ServerEndPoint Local { get; set; }
		public ServerEndPoint Alocated { get; set; }
		public IPEndPoint Reflexive { get; set; }


		public PermissionsList Permissions { get; private set; }


		public IPEndPoint ActiveDestination
		{
			get
			{
				return activeDestination;
			}
			set
			{
				activeDestination.CopyFrom(value);
			}
		}

		public bool IsActiveDestinationEnabled
		{
			get
			{
				return activeDestination.Address != IPAddress.Any;
			}
			set
			{
				if (value == false)
					activeDestination.Address = IPAddress.Any;
			}
		}

		public uint Lifetime 
		{
			get
			{
				return (uint)lifetime / 1000;
			}
			set
			{
				lifetime = (int)value * 1000;
				created = Environment.TickCount;
			}
		}

		public void TouchLifetime()
		{
			created = Environment.TickCount;
		}

		public bool IsValid()
		{
			return IsValid(Environment.TickCount);
		}

		public bool IsValid(int tickCount)
		{
			return tickCount - created < lifetime;
		}

		public uint SequenceNumber { get; set; }
		public TurnMessage Response { get; set; }
	}
}
