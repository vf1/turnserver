using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Turn.Message;

namespace Turn.Server
{
	class TransactionServer
	{
		private object sync;
		private Dictionary<string, CacheRecord> cache;
		private Queue<CacheRecord> timeout;
		private Timer timer;

		public TransactionServer()
		{
			sync = new object();
			cache = new Dictionary<string, CacheRecord>();
			timeout = new Queue<CacheRecord>();
			timer = new Timer(Timer_EventHandler, null, 0, 1000);
		}

		//public bool GetCachedResponse(SocketAsyncEventArgsEx e, out TurnMessage response)
		//{
		//    response = null;

		//    string key = GetCacheRecordKey(e);

		//    lock (sync)
		//    {
		//        CacheRecord record;
		//        if (cache.TryGetValue(key, out record))
		//            response = record.Response;
		//    }

		//    return response != null;
		//}

		//public void CacheResponse(SocketAsyncEventArgsEx e, TurnMessage response)
		//{
		//    string key = GetCacheRecordKey(e);

		//    lock (sync)
		//    {
		//        var record = new CacheRecord()
		//        {
		//            Key = key,
		//            Response = response,
		//        };

		//        cache.Add(record.Key, record);
		//        timeout.Enqueue(record);
		//    }
		//}

		public static TransactionId GenerateTransactionId()
		{
			return new TransactionId()
			{
				Value = Guid.NewGuid().ToByteArray(),
			};
		}

		//private static string GetCacheRecordKey(SocketAsyncEventArgs e)
		//{
		//    TransactionId id = TurnMessage.SafeGetTransactionId(e.Buffer, e.Count);

		//    if (id == null)
		//        return null;

		//    return String.Format("{0}-{1}-{2}", id.ToString(), e.GetSocket().GetHashCode(), e.GetRemoteEndPoint().ToString());
		//}

		private void Timer_EventHandler(Object stateInfo)
		{
			lock (sync)
			{
				if (timeout.Count > 0) // - error
					while (timeout.Peek().GetCreatedTime() > 5000)
					{
						cache.Remove(timeout.Dequeue().Key);
					}
			}
		}

		class CacheRecord
		{
			private int createdTickCount;

			public CacheRecord()
			{
				createdTickCount = Environment.TickCount;
			}

			public string Key
			{
				get;
				set;
			}

			public TurnMessage Response
			{
				get;
				set;
			}

			public int GetCreatedTime()
			{
				return createdTickCount - Environment.TickCount;
			}
		}
	}
}
