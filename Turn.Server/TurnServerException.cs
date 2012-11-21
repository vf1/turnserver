using System;
using Turn.Message;

namespace Turn.Server
{
	class TurnServerException : Exception
	{
		public TurnServerException(ErrorCode errorCode)
			: base()
		{
			ErrorCode = errorCode;
		}

		public TurnServerException(ErrorCode errorCode, string message)
			: base(message)
		{
			ErrorCode = errorCode;
		}

		public ErrorCode ErrorCode { get; set; }

		public override string Message 
		{
			get { return ErrorCode.ToString(); }
		}
	}
}
