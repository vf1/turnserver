using System;
using Turn.Message;

namespace Turn.Server
{
	class ConnectionId
	{
		public ConnectionId()
		{ 
		}

		public ConnectionId(byte[] value)
		{
			Value = value;
		}

		public byte[] Value { get; set; }

		public override bool Equals(Object obj)
		{
			if (Value == null)
				return false;

			if (obj == null)
				return false;

			byte[] value2;
			if (obj is ConnectionId)
				value2 = (obj as ConnectionId).Value;
			else if (obj is byte[])
				value2 = obj as byte[];
			else
				return false;

			if (value2 == null)
				return false;

			if (Value.Length != value2.Length)
				return false;

			for (int i = 0; i < Value.Length; i++)
				if (Value[i] != value2[i])
					return false;

			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			int startIndex = 0;

			while (Value.Length - startIndex >= 4)
			{
				hashCode ^= BitConverter.ToInt32(Value, startIndex);
				startIndex += 4;
			}

			if (Value.Length - startIndex >= 2)
			{
				hashCode ^= BitConverter.ToInt16(Value, startIndex);
				startIndex += 2;
			}

			if (startIndex < Value.Length)
				hashCode ^= (int)Value[startIndex++] << 16;

			return hashCode;
		}

		public static bool operator ==(ConnectionId id1, ConnectionId id2)
		{
			return Equals(id1, id2);
		}

		public static bool operator !=(ConnectionId id1, ConnectionId id2)
		{
			return !Equals(id1, id2);
		}
	}

	static class ConnectionIdHelper
	{
		public static ConnectionId GetConnectionId(this TurnMessage request)
		{
			if (request.MsSequenceNumber != null)
			{
				return new ConnectionId()
				{
					Value = request.MsSequenceNumber.ConnectionId,
				};
			}

			return null;
		}
	}
}
