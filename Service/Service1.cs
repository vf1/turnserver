using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;
using System.Text;

namespace Service
{
	public class Service1 
		: ServiceBase
	{
		private Settings settings;
		private Turn.Server.TurnServer turnServer;
		private WcfService.WcfTurnService wcfService;
		private EventLog eventLog;
		private FileStream fileLog;

		public const string BaseServiceName = @"OfficeSIP Turn Server";

		public Service1(string extension)
		{
			ServiceName = GetServiceName(extension);

			eventLog = new EventLog(GetEventlogName(extension));
			eventLog.Source = GetEventlogName(extension);
		}

		public static string GetValidExtension(string extension)
		{
			return String.IsNullOrEmpty(extension) ? "N1" : extension;
		}

		public static string GetServiceName(string extension)
		{
			return String.Format("{0} {1}", BaseServiceName, GetValidExtension(extension));
		}

		public static string GetDisplayName(string extension)
		{
			return GetServiceName(extension);
		}

		public static string GetDescription(string extension)
		{
			return GetServiceName(extension);
		}

		public static string GetEventlogName(string extension)
		{
			return String.Format("{1} {0}", @"Turn Server OfficeSIP", GetValidExtension(extension));
		}

		public void Debug(string[] args)
		{
			const int mutexDelay = 1000;

			Mutex mutex1 = new Mutex(false, @"{4A85555B-8675-4230-84A0-4C87FC234B42}-" + ServiceName.Replace(' ', '-'));
			Mutex mutex2 = new Mutex(false, @"{2F36CC7D-4486-46fd-AB4F-44C5D2157B46}-" + ServiceName.Replace(' ', '-'));

			mutex2.WaitOne();
			bool anotherStopped = mutex1.WaitOne(mutexDelay * 5);
			mutex2.ReleaseMutex();

			if (anotherStopped)
			{
				try
				{
					fileLog = new FileStream(GetAppPath(ServiceName.Replace(' ', '.') + @".log"), 
						FileMode.Create, FileAccess.Write, FileShare.Read);
					
					Start2();

					while (mutex2.WaitOne(0))
					{
						Thread.Sleep(mutexDelay);
						mutex2.ReleaseMutex();
#if DEBUG
						GC.Collect();
						GC.WaitForPendingFinalizers();
						GC.Collect();
#endif
					}
				}
				finally
				{
					Stop2();
				}

				mutex1.ReleaseMutex();
			}
		}

		protected override void OnStart(string[] args)
		{
			RequestAdditionalTime(30 * 1000);
			Start2();
		}

		protected override void OnStop()
		{
			RequestAdditionalTime(10 * 1000);
			Stop2();
		}

		private void Start2()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			settings = new Settings(GetAppPath(Settings.SettingsFileName));
			settings.Load();

			try
			{
				string wcfServiceUri = @"net.tcp://localhost:10002/officesip.turn.server.n1";

				WriteLogEntry(
					String.Format("Starting WCF Service\r\nService Uri: {0}", wcfServiceUri), 
					EventLogEntryType.Information);

				wcfService = new WcfService.WcfTurnService()
				{
					PfxPathName = GetAppPath(@"OfficeSIP.pfx"),
					ServiceUri = wcfServiceUri,
					ServiceSettings = settings,
				};

				wcfService.NewConfigurationSetted += WcfService_NewConfigurationSetted;

				wcfService.Start();

				WriteLogEntry("WCF Service started.", EventLogEntryType.Information);
			}
			catch(Exception ex)
			{
				WriteLogEntry("Failed to start WCF service: " + ex.ToString(), EventLogEntryType.Error);
			}

			try
			{
				WriteLogEntry(
					String.Format("Starting TurnServer\r\nTurn UDP Port: {0}\r\nTurn TCP Port: {1}\r\nTurn TLS Port: {2}\r\nPublic IP and Ports Range: {3}:{4}-{5}\r\nRealm: {6}",
					settings.TurnUdpPort, settings.TurnTcpPort, settings.TurnTlsPort, 
					settings.PublicIp, settings.MinPort, settings.MaxPort, settings.Realm),
					EventLogEntryType.Information);

				turnServer = new Turn.Server.TurnServer()
				{
					TurnUdpPort = settings.TurnUdpPort,
					TurnTcpPort = settings.TurnTcpPort,
					TurnPseudoTlsPort = settings.TurnTlsPort,
					PublicIp = settings.PublicIp,
					MinPort = settings.MinPort,
					MaxPort = settings.MaxPort,
					Authentificater = new Turn.Server.Authentificater()
					{
						Realm = settings.Realm,
						Key1 = settings.Key1,
						Key2 = settings.Key2,
					},
				};

				turnServer.Start();

				WriteLogEntry("TurnServer started.", EventLogEntryType.Information);
			}
			catch (Exception ex)
			{
				WriteLogEntry("Failed to start TurnServer: " + ex.ToString(), EventLogEntryType.Error);

				turnServer.Stop();
				turnServer = null;
			}
		}

		private void WcfService_NewConfigurationSetted(object sender, EventArgs e)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(RestartServiceThread));
		}

		private void RestartServiceThread(object sender)
		{
			Thread.Sleep(4000);

			Stop2();
			Start2();
		}

		private void Stop2()
		{
			try
			{
				if (turnServer != null)
				{
					turnServer.Stop();
					turnServer = null;
				}

				if (wcfService != null)
				{
					wcfService.NewConfigurationSetted -= WcfService_NewConfigurationSetted;
					wcfService.Dispose();
					wcfService = null;
				}

				AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
			}
			catch (Exception ex)
			{
				WriteLogEntry("Error ocurs while stoping: " + ex.ToString(), EventLogEntryType.Error);
			}
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception)
				WriteLogEntry("Unhandled Exception: " + (e.ExceptionObject as Exception).ToString(), EventLogEntryType.Error);
			else
				WriteLogEntry("Unhandled Exception w/o ExceptionObject", EventLogEntryType.Error);
		}

		private void WriteLogEntry(string message, EventLogEntryType entryType)
		{
			try
			{
				if (eventLog != null)
					eventLog.WriteEntry(message, entryType);
			}
			catch
			{
				eventLog = null;
			}

			try
			{
				if (fileLog != null)
				{
					lock (fileLog)
					{
						UTF8Encoding utf8 = new UTF8Encoding();
						var bytes = utf8.GetBytes(
							String.Format(@"{1} ({2}):{0}{3}{0}{0}",
								Environment.NewLine, entryType, DateTime.Now.ToLongTimeString(), message));
						fileLog.Write(bytes, 0, bytes.Length);
						fileLog.Flush();
					}
				}
			}
			catch
			{
				fileLog = null;
			}
		}

		private string GetAppPath(string filename)
		{
			return Path.Combine(
				Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), filename);
		}
	}
}
