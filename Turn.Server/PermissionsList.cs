using System;
using System.Collections.Generic;
using System.Net;

namespace Turn.Server
{
	class PermissionsList
	{
		private Dictionary<string, object> permited;

		public PermissionsList()
		{
			permited = new Dictionary<string, object>();
		}

		public void Permit(EndPoint endPoint)
		{
			if (permited != null)
			{
				if (permited.ContainsKey(endPoint.ToString()) == false)
					permited.Add(endPoint.ToString(), null);
			}
		}

		public bool IsPermited(EndPoint endPoint)
		{
			if (permited == null)
				return true;

			return permited.ContainsKey(endPoint.ToString());
		}
	}
}
