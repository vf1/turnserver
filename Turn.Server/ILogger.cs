using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turn.Server
{
	public interface ILogger
	{
		void WriteError(string message);
		void WriteWarning(string message);
		void WriteInformation(string message);
	}

	class NullLogger
		: ILogger
	{
		public void WriteError(string message)
		{
		}

		public void WriteWarning(string message)
		{
		}

		public void WriteInformation(string message)
		{
		}
	}
}
