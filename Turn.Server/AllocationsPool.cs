using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Turn.Message;
using SocketServers;

namespace Turn.Server
{
	class AllocationsPool
	{
		public delegate void AllocationsPoolDelegate(Allocation allocation);

		private object syncRoot;
		private Dictionary<TransactionId, Allocation> allocations1;
		private Dictionary<ServerEndPoint, Allocation> byAllocated;
		private Dictionary<ConnectionId, Allocation> allocations3;
		private Dictionary<string, Allocation> allocations4;
		private Timer timer;

		public AllocationsPool()
		{
			syncRoot = new object();
			allocations1 = new Dictionary<TransactionId, Allocation>();
			byAllocated = new Dictionary<ServerEndPoint, Allocation>();
			allocations3 = new Dictionary<ConnectionId, Allocation>();
			allocations4 = new Dictionary<string, Allocation>();
			timer = new Timer(Timer_EventHandler, null, 0, 10000);
		}

		public event AllocationsPoolDelegate Removed;

		public void Add(Allocation allocation)
		{
			lock (syncRoot)
			{
				allocations1.Add(allocation.TransactionId, allocation);
				byAllocated.Add(allocation.Alocated, allocation);
				allocations3.Add(allocation.ConnectionId, allocation);
				allocations4.Add(GetKey(allocation.Local, allocation.Reflexive), allocation);
			}
		}

		public void Remove(Allocation allocation)
		{
			lock (syncRoot)
			{
				allocations1.Remove(allocation.TransactionId);
				byAllocated.Remove(allocation.Alocated);
				allocations3.Remove(allocation.ConnectionId);
				allocations4.Remove(GetKey(allocation.Local, allocation.Reflexive));
			}

			OnRemoved(allocation);
		}

		public void Clear()
		{
			Allocation[] removeList = null;

			lock (syncRoot)
			{
				if (allocations1.Values.Count > 0)
				{
					removeList = new Allocation[allocations1.Values.Count];
					allocations1.Values.CopyTo(removeList, 0);
				}

				allocations1.Clear();
				byAllocated.Clear();
				allocations3.Clear();
				allocations4.Clear();
			}

			if (removeList != null)
				foreach (var allocation in removeList)
					OnRemoved(allocation);
		}

		public Allocation Get(TransactionId transactionId)
		{
			Allocation allocation = null;

			lock (syncRoot)
				allocations1.TryGetValue(transactionId, out allocation);

			return allocation != null && allocation.IsValid() ? allocation : null;
		}

		public Allocation Get(ServerEndPoint allocated)
		{
			Allocation allocation = null;

			lock (syncRoot)
				byAllocated.TryGetValue(allocated, out allocation);

			return allocation != null && allocation.IsValid() ? allocation : null;
		}

		public Allocation Get(ConnectionId connectionId)
		{
			Allocation allocation = null;

			lock (syncRoot)
				allocations3.TryGetValue(connectionId, out allocation);

			return allocation != null && allocation.IsValid() ? allocation : null;
		}

		public Allocation GetByTurn(ServerEndPoint local, IPEndPoint remote)
		{
			Allocation allocation = null;

			lock (syncRoot)
				allocations4.TryGetValue(GetKey(local, remote), out allocation);

			return allocation != null && allocation.IsValid() ? allocation : null;
		}

		public Allocation GetByPeer(ServerEndPoint local, IPEndPoint remote)
		{
			Allocation allocation = null;

			lock (syncRoot)
				allocations4.TryGetValue(GetKey(local, remote), out allocation);

			return allocation != null && allocation.IsValid() ? allocation : null;
		}

		public int Count
		{
			get
			{
				lock (syncRoot)
					return allocations1.Count;
			}
		}

		private string GetKey(ServerEndPoint local, IPEndPoint remote)
		{
			return local.ToString() + remote.ToString();
		}

		private void Timer_EventHandler(Object stateInfo)
		{
			List<Allocation> removeList = null;

			lock (syncRoot)
			{
				int now = Environment.TickCount;

				foreach (var tuple in allocations1)
					if (tuple.Value.IsValid(now) == false)
					{
						if (removeList == null)
							removeList = new List<Allocation>();
						removeList.Add(tuple.Value);
					}
			}

			if (removeList != null)
				foreach (var allocation in removeList)
					Remove(allocation);
		}

		private void OnRemoved(Allocation allocation)
		{
#if DEBUG
			try
			{
				Monitor.Exit(syncRoot);
				throw new Exception("Deadlock Warning!");
			}
			catch (SynchronizationLockException)
			{
			}
#endif

			Removed(allocation);
		}
	}
}
