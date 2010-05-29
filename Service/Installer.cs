using System.Configuration.Install;
using System.ComponentModel;
using System.ServiceProcess;
using System.Diagnostics;
using System.Collections;
using System;

namespace Service
{
	[RunInstallerAttribute(true)]
	public class Installer : System.Configuration.Install.Installer
	{
		private bool addOnce = true;

		public Installer()
		{
		}

		private void AddInstallers(string serviceSubname)
		{
			if (addOnce)
			{
				addOnce = false;

				var eventLogInstaller = new EventLogInstaller();
				eventLogInstaller.Log = Service1.GetEventlogName(serviceSubname);
				eventLogInstaller.Source = Service1.GetEventlogName(serviceSubname);
				Installers.Add(eventLogInstaller);

				var serviceInstaller = new ServiceInstaller();
				serviceInstaller.StartType = ServiceStartMode.Automatic;
				serviceInstaller.ServiceName = Service1.GetServiceName(serviceSubname);
				serviceInstaller.DisplayName = Service1.GetDisplayName(serviceSubname);
				serviceInstaller.Description = Service1.GetDescription(serviceSubname);
				Installers.Add(serviceInstaller);

				var processInstaller = new ServiceProcessInstaller();
				processInstaller.Account = ServiceAccount.LocalSystem;
				Installers.Add(processInstaller);
			}
		}

		protected override void OnBeforeInstall(IDictionary stateSaver)
		{
			stateSaver["subName"] = Context.Parameters["subName"];
			AddInstallers(stateSaver["subName"] as string);

			base.OnBeforeInstall(stateSaver);
		}

		public override void Install(IDictionary stateSaver)
		{
			this.Uninstall(stateSaver);

			var serviceSubname = stateSaver["subName"] as string;

			base.Install(stateSaver);
			ServiceEx.SetCommandLineArgs(Service1.GetServiceName(serviceSubname), serviceSubname); 

			using (var sc = new ServiceController(Service1.GetServiceName(serviceSubname)))
			{
				sc.Start();
				sc.Refresh();
			}
		}

		protected override void OnBeforeUninstall(IDictionary savedState)
		{
			var serviceSubname = savedState["subName"] as string;

			try
			{
				using (var sc = new ServiceController(Service1.GetServiceName(serviceSubname)))
				{
					sc.Stop();
					sc.Refresh();
					sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
				}
			}
			catch
			{
			}

			AddInstallers(serviceSubname);
		}

		public override void Uninstall(IDictionary savedState)
		{
			try
			{
			// не работает - наверное путь не правильный
			//	var appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
			//	System.IO.File.Delete(
			//		System.IO.Path.Combine(appPath, Settings.SettingsFileName));
			}
			catch
			{
			}

			try
			{
				base.Uninstall(savedState);
			}
			catch
			{
			}
		}
	}
}