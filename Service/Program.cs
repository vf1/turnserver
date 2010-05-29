using System;
using System.ServiceProcess;

namespace Service
{
	static class Program
	{
		static void Main(string[] args)
		{
			bool console = false;
			string extension = null;

			if (args.Length >= 1)
			{
				if (args[0] == @"-debug" || args[0] == @"-console")
				{
					console = true;
					if (args.Length >= 2)
						extension = args[1];
				}
				else
				{
					extension = args[0];
				}
			}

			if (console)
			{
				new Service1(extension).Debug(args);
			}
			else
			{
				ServiceBase.Run(new Service1(extension));
			}
		}
	}
}
