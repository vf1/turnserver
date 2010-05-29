using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Service
{
	class ServiceEx
	{
		public static void SetCommandLineArgs(string serviceName, string parameters)
		{
			IntPtr hScm = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
			if (hScm == IntPtr.Zero)
				throw new Win32Exception();
			try
			{
				IntPtr hSvc = OpenService(hScm, serviceName, SERVICE_ALL_ACCESS);
				if (hSvc == IntPtr.Zero)
					throw new Win32Exception();
				try
				{
					QUERY_SERVICE_CONFIG oldConfig;
					uint bytesAllocated = 8192; // Per documentation, 8K is max size.
					IntPtr ptr = Marshal.AllocHGlobal((int)bytesAllocated);
					try
					{
						uint bytesNeeded;
						if (!QueryServiceConfig(hSvc, ptr, bytesAllocated, out bytesNeeded))
						{
							throw new Win32Exception();
						}
						oldConfig = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ptr, typeof(QUERY_SERVICE_CONFIG));
					}
					finally
					{
						Marshal.FreeHGlobal(ptr);
					}

					string newBinaryPathAndParameters = oldConfig.lpBinaryPathName + " " + parameters;

					if (!ChangeServiceConfig(hSvc, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE,
						newBinaryPathAndParameters, null, IntPtr.Zero, null, null, null, null))
						throw new Win32Exception();
				}
				finally
				{
					if (!CloseServiceHandle(hSvc))
						throw new Win32Exception();
				}
			}
			finally
			{
				if (!CloseServiceHandle(hScm))
					throw new Win32Exception();
			}
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr OpenSCManager(
			string lpMachineName,
			string lpDatabaseName,
			uint dwDesiredAccess);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr OpenService(
			IntPtr hSCManager,
			string lpServiceName,
			uint dwDesiredAccess);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct QUERY_SERVICE_CONFIG
		{
			public uint dwServiceType;
			public uint dwStartType;
			public uint dwErrorControl;
			public string lpBinaryPathName;
			public string lpLoadOrderGroup;
			public uint dwTagId;
			public string lpDependencies;
			public string lpServiceStartName;
			public string lpDisplayName;
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryServiceConfig(
			IntPtr hService,
			IntPtr lpServiceConfig,
			uint cbBufSize,
			out uint pcbBytesNeeded);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ChangeServiceConfig(
			IntPtr hService,
			uint dwServiceType,
			uint dwStartType,
			uint dwErrorControl,
			string lpBinaryPathName,
			string lpLoadOrderGroup,
			IntPtr lpdwTagId,
			string lpDependencies,
			string lpServiceStartName,
			string lpPassword,
			string lpDisplayName);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseServiceHandle(
			IntPtr hSCObject);

		private const uint SERVICE_NO_CHANGE = 0xffffffffu;
		private const uint SC_MANAGER_ALL_ACCESS = 0xf003fu;
		private const uint SERVICE_ALL_ACCESS = 0xf01ffu;
	}
}
