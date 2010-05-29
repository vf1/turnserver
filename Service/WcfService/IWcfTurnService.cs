using System;
using System.Net;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace WcfService
{
	[DataContract(Name = "WcfTurnConfiguration", Namespace = "http://officesip.com/turn.server.control")]
	public class WcfTurnConfiguration
	{
		[DataMember]
		public IPAddress PublicIp { set; get; }

		[DataMember]
		public int TurnUdpPort { set; get; }

		[DataMember]
		public int TurnTcpPort { set; get; }

		[DataMember]
		public int TurnTlsPort { set; get; }

		[DataMember]
		public int MinPort { set; get; }

		[DataMember]
		public int MaxPort { set; get; }

		[DataMember]
		public string Realm { set; get; }

		[DataMember]
		public byte[] Key1 { set; get; }

		[DataMember]
		public byte[] Key2 { set; get; }

		[DataMember]
		public string AdminName { get; set; }

		[DataMember]
		public string AdminPass { get; set; }
	}

	[ServiceContract(Namespace = "http://officesip.com/turn.server.control")]
	interface IWcfTurnService
	{
		[OperationContract]
		WcfTurnConfiguration GetConfiguration();

		[OperationContract]
		void SetConfiguration(WcfTurnConfiguration configuration);
	}
}
