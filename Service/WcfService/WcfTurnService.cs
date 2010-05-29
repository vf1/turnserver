using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using System.IO;
using System.Reflection;

namespace WcfService
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
	class WcfTurnService
		: IWcfTurnService
		, IDisposable
	{
		protected ServiceHost serviceHost;

		public string ServiceUri { get; set; }
		public string PfxPathName { get; set; }
		public Service.Settings ServiceSettings { get; set; }
		public event EventHandler<EventArgs> NewConfigurationSetted;

		public void Start()
		{
			try
			{
				serviceHost = new ServiceHost(this, new Uri(ServiceUri));

				var validator = new CustomUserNamePasswordValidator(ServiceSettings);

				serviceHost.Credentials.ServiceCertificate.Certificate = new X509Certificate2(PfxPathName, "");
				serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
				serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = validator;

				var binding = new NetTcpBinding();
				binding.Security.Mode = SecurityMode.Message;
				binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

				serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior());
				serviceHost.AddServiceEndpoint(typeof(IWcfTurnService), binding, "");
				serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");

				serviceHost.Open();
			}
			catch
			{
				serviceHost = null;
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				if (serviceHost != null)
					serviceHost.Close();
			}
			catch
			{
			}
		}

		public WcfTurnConfiguration GetConfiguration()
		{
			return new WcfTurnConfiguration()
			{
				PublicIp = ServiceSettings.PublicIp,
				TurnUdpPort = ServiceSettings.TurnUdpPort,
				TurnTcpPort = ServiceSettings.TurnTcpPort,
				TurnTlsPort = ServiceSettings.TurnTlsPort,
				MinPort = ServiceSettings.MinPort,
				MaxPort = ServiceSettings.MaxPort,
				Realm = ServiceSettings.Realm,
				Key1 = ServiceSettings.Key1,
				Key2 = ServiceSettings.Key2,
			};
		}

		public void SetConfiguration(WcfTurnConfiguration configuration)
		{
			ServiceSettings.AdminName = configuration.AdminName;
			ServiceSettings.AdminPass = configuration.AdminPass;
			ServiceSettings.PublicIp = configuration.PublicIp;
			ServiceSettings.TurnUdpPort= configuration.TurnUdpPort;
			ServiceSettings.TurnTcpPort = configuration.TurnTcpPort;
			ServiceSettings.TurnTlsPort = configuration.TurnTlsPort;
			ServiceSettings.MinPort = configuration.MinPort;
			ServiceSettings.MaxPort = configuration.MaxPort;
			ServiceSettings.Realm = configuration.Realm;
			ServiceSettings.Key1 = configuration.Key1;
			ServiceSettings.Key2 = configuration.Key2;

			ServiceSettings.Save();

			if (NewConfigurationSetted != null)
				NewConfigurationSetted(this, new EventArgs());
		}

		protected class CustomUserNamePasswordValidator : UserNamePasswordValidator
		{
			private Service.Settings serviceSettings;

			public CustomUserNamePasswordValidator(Service.Settings serviceSettings)
			{
				this.serviceSettings = serviceSettings;
			}

			public override void Validate(string userName, string password)
			{
				if (userName.ToLower() != serviceSettings.AdminName || password != serviceSettings.AdminPass)
					throw new FaultException(new FaultReason(""), new FaultCode("AccessDenied"));
			}
		}

	}
}
