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
        public enum RemoveReason
        {
            Timeout,
            Replace,
            Stopping,
        }

        public delegate void AllocationsPoolDelegate(Allocation allocation, RemoveReason reason);

        private object syncRoot;
        private Dictionary<ServerEndPoint, Allocation> byAllocated;
        private Dictionary<ServerEndPoint, Allocation> byReal;
        private Dictionary<ConnectionId, Allocation> byConnectionId;
        private Dictionary<string, Allocation> byKey;
        private Timer timer;

        public AllocationsPool()
        {
            syncRoot = new object();
            byAllocated = new Dictionary<ServerEndPoint, Allocation>();
            byReal = new Dictionary<ServerEndPoint, Allocation>();
            byConnectionId = new Dictionary<ConnectionId, Allocation>();
            byKey = new Dictionary<string, Allocation>();
            timer = new Timer(Timer_EventHandler, null, 0, 10000);
        }

        public event AllocationsPoolDelegate Removed;

        public void Replace(Allocation allocation)
        {
            Allocation oldAllocation;

            lock (syncRoot)
            {
                var key = GetKey(allocation.Local, allocation.Reflexive);

                if (byKey.TryGetValue(key, out oldAllocation))
                {
                    byAllocated.Remove(oldAllocation.Alocated);
                    byReal.Remove(oldAllocation.Real);
                    byConnectionId.Remove(oldAllocation.ConnectionId);
                    byKey.Remove(key);
                }

                byAllocated.Add(allocation.Alocated, allocation);
                byReal.Add(allocation.Real, allocation);
                byConnectionId.Add(allocation.ConnectionId, allocation);
                byKey.Add(key, allocation);
            }

            if (oldAllocation != null)
                OnRemoved(oldAllocation, RemoveReason.Replace);
        }

        public void Remove(Allocation oldAllocation, RemoveReason reason)
        {
            lock (syncRoot)
            {
                byAllocated.Remove(oldAllocation.Alocated);
                byReal.Remove(oldAllocation.Real);
                byConnectionId.Remove(oldAllocation.ConnectionId);
                byKey.Remove(GetKey(oldAllocation.Local, oldAllocation.Reflexive));
            }

            OnRemoved(oldAllocation, reason);
        }

        public void Clear()
        {
            Allocation[] removeList = null;

            lock (syncRoot)
            {
                if (byAllocated.Values.Count > 0)
                {
                    removeList = new Allocation[byAllocated.Values.Count];
                    byAllocated.Values.CopyTo(removeList, 0);
                }

                byAllocated.Clear();
                byReal.Clear();
                byConnectionId.Clear();
                byKey.Clear();
            }

            if (removeList != null)
                foreach (var allocation in removeList)
                    OnRemoved(allocation, RemoveReason.Stopping);
        }

        public Allocation Get(ServerEndPoint allocated)
        {
            Allocation allocation = null;

            lock (syncRoot)
            {
                if (byAllocated.TryGetValue(allocated, out allocation) == false)
                    byReal.TryGetValue(allocated, out allocation);
            }

            return allocation != null && allocation.IsValid() ? allocation : null;
        }

        public Allocation Get(ConnectionId connectionId)
        {
            Allocation allocation = null;

            lock (syncRoot)
                byConnectionId.TryGetValue(connectionId, out allocation);

            return allocation != null && allocation.IsValid() ? allocation : null;
        }

        public Allocation GetByPeer(ServerEndPoint local, IPEndPoint remote)
        {
            Allocation allocation = null;

            lock (syncRoot)
                byKey.TryGetValue(GetKey(local, remote), out allocation);

            return allocation != null && allocation.IsValid() ? allocation : null;
        }

        public int Count
        {
            get
            {
                lock (syncRoot)
                    return byAllocated.Count;
            }
        }

        private string GetKey(ServerEndPoint local, IPEndPoint remote)
        {
            return local.ToString() + @"|" + remote.ToString();
        }

        private void Timer_EventHandler(Object stateInfo)
        {
            List<Allocation> removeList = null;

            lock (syncRoot)
            {
                int now = Environment.TickCount;

                foreach (var tuple in byAllocated)
                    if (tuple.Value.IsValid(now) == false)
                    {
                        if (removeList == null)
                            removeList = new List<Allocation>();
                        removeList.Add(tuple.Value);
                    }
            }

            if (removeList != null)
                foreach (var allocation in removeList)
                    Remove(allocation, RemoveReason.Timeout);
        }

        private void OnRemoved(Allocation allocation, RemoveReason reason)
        {
            Removed(allocation, reason);
        }
    }
}
