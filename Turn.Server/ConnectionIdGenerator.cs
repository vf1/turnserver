using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using SocketServers;
using Turn.Message;

namespace Turn.Server
{
	static class ConnectionIdGenerator
	{
		private static byte[] sha1Key;
        private static long count;
		[ThreadStatic]
		private static HMACSHA1 sha1;

		static ConnectionIdGenerator()
		{
			sha1Key = new byte[8];
			(new Random(Environment.TickCount)).NextBytes(sha1Key);
		}

		public static ConnectionId Generate(ServerEndPoint reflexive, IPEndPoint local)
		{
			if (sha1 == null)
				sha1 = new HMACSHA1(sha1Key);

            string hashData = reflexive.ToString() + local.ToString() + Interlocked.Increment(ref count).ToString();

			var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(hashData));

			return new ConnectionId()
			{
				Value1 = BitConverter.ToInt64(bytes, 0),
				Value2 = BitConverter.ToInt64(bytes, 8),
				Value3 = BitConverter.ToInt32(bytes, 16),
			};
		}
	}
}
