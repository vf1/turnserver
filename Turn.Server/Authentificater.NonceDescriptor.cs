using System;

namespace Turn.Server
{
	partial class Authentificater
	{
		class NonceDescriptor
		{
			public NonceDescriptor()
			{
				Value = Guid.NewGuid().ToString();
				Created = Environment.TickCount;
			}

			public readonly string Value;
			public readonly int Created;
		}
	}
}
