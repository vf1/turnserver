using System.Net;
using System.Net.Sockets;
using Turn.Server;
using Turn.Message;
using NUnit.Framework;
using SocketServers;

namespace TestTurnServer
{
	[TestFixture()]
	public class AllocationsPoolTest
	{
		//[Test()]
		//public void GeneralTest()
		//{
		//    AllocationsPool pool = new AllocationsPool();
		//    pool.Removed += Pool_Removed;

		//    var conectionId1 = new ConnectionId(new byte[] { 1, 2, 3, 4, 5, });
		//    var conectionId11 = new ConnectionId(new byte[] { 1, 2, 3, 4, 5, });
		//    var conectionId2 = new ConnectionId(new byte[] { 1, 2, 3, 4, 6, });
		//    var conectionId3 = new ConnectionId(new byte[] { 1, 2, 3, 4, 7, });
		//    var conectionId4 = new ConnectionId(new byte[] { 1, 2, 3, 4, 8, });
		//    var conectionId5 = new ConnectionId(new byte[] { 1, 2, 3, 4, 9, });

		//    pool.Replace(new Allocation() { ConnectionId = conectionId1, Alocated = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 1), TransactionId = new Turn.Message.TransactionId() { Value = new byte[] { 1 }, }, Lifetime = 10, Local = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 1), Reflexive = new IPEndPoint(IPAddress.None, 1), });
		//    pool.Replace(new Allocation() { ConnectionId = conectionId2, Alocated = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 2), TransactionId = new Turn.Message.TransactionId() { Value = new byte[] { 2 }, }, Lifetime = 10, Local = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 2), Reflexive = new IPEndPoint(IPAddress.None, 1), });
		//    pool.Replace(new Allocation() { ConnectionId = conectionId3, Alocated = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 3), TransactionId = new Turn.Message.TransactionId() { Value = new byte[] { 3 }, }, Lifetime = 10, Local = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 3), Reflexive = new IPEndPoint(IPAddress.None, 1), });
		//    pool.Replace(new Allocation() { ConnectionId = conectionId4, Alocated = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 4), TransactionId = new Turn.Message.TransactionId() { Value = new byte[] { 4 }, }, Lifetime = 10, Local = new ServerEndPoint(ServerProtocol.Udp, IPAddress.None, 4), Reflexive = new IPEndPoint(IPAddress.None, 1), });

		//    System.Threading.Thread.Sleep(300);

		//    Assert.AreEqual(4, pool.Count);
			 
		//    Assert.AreEqual(pool.Get(conectionId1).ConnectionId, conectionId1);
		//    Assert.AreEqual(pool.Get(conectionId2).ConnectionId, conectionId2);
		//    Assert.AreEqual(pool.Get(conectionId3).ConnectionId, conectionId3);
		//    Assert.AreEqual(pool.Get(conectionId4).ConnectionId, conectionId4);
		//    Assert.AreEqual(pool.Get(conectionId5), null);

		//    Assert.AreEqual(pool.Get(conectionId11).ConnectionId, conectionId1);
		//}

		//private void Pool_Removed(Allocation allocation)
		//{
		//}
	}
}
