using System;
using System.Collections.Generic;
using System.Threading;
using Turn.Message;

namespace Turn.Server
{
	public partial class Authentificater
	{
		private object syncRoot;
		private Realm realm;
		private Dictionary<string, NonceDescriptor> nonces;
		private Queue<NonceDescriptor> timeouts;
		private Timer nonceTimer;

		public Authentificater()
		{
			syncRoot = new object();
			realm = new Realm(TurnMessageRfc.MsTurn);
			nonces = new Dictionary<string, NonceDescriptor>();
			timeouts = new Queue<NonceDescriptor>();
			nonceTimer = new Timer(NonceTimer_EventHandler, null, 0, 1000);
		}

		public string Realm
		{
			get
			{
				return realm.Value;
			}
			set
			{
				realm.Value = value;
			}
		}

		public byte[] Key1 { get; set; }
		public byte[] Key2 { get; set; }


		public bool Process(TurnMessage request, out TurnMessage response)
		{
			lock (syncRoot)
			{
				ErrorCode? errorCode = ValidateRequest(request);

				if (errorCode != null)
				{
					response = new TurnMessage()
					{
						MessageType = request.MessageType.GetErrorResponseType(),
						TransactionId = request.TransactionId,
						MagicCookie = new MagicCookie(),
						ErrorCodeAttribute = new ErrorCodeAttribute()
						{
							ErrorCode = (int)errorCode,
							ReasonPhrase = ((ErrorCode)errorCode).GetReasonPhrase(),
						},
						Realm = new Realm(TurnMessageRfc.MsTurn) { Value = Realm, },
						Nonce = new Nonce(TurnMessageRfc.MsTurn) { Value = NewNonce(), },
						MsVersion = new MsVersion() { Value = 1, },
					};
				}
				else
				{
					response = null;
				}

				return response == null;
			}
		}

		private ErrorCode? ValidateRequest(TurnMessage request)
		{
			if (request.MessageIntegrity == null)
				return ErrorCode.Unauthorized;

			if (request.MessageType == MessageType.AllocateRequest)
			{
				if (request.Realm == null)
					return ErrorCode.MissingRealm;

				if (request.Nonce == null)
					return ErrorCode.MissingNonce;

				if (IsValidNonce(request.Nonce.Value) == false)
					return ErrorCode.StaleNonce;
			}

			if (request.MsUsername == null)
				return ErrorCode.MissingUsername;

			if (request.IsValidMsUsername(Key1) == false)
				return ErrorCode.UnknownUsername;


			if (request.Realm != null)
			{
				if (request.Realm.Value != Realm)
					return ErrorCode.MissingRealm;
			}
			else
				request.Realm = realm;


			if (request.IsValidMessageIntegrity(Key2) == false)
				return ErrorCode.IntegrityCheckFailure;

			return null;
		}

		private string NewNonce()
		{
			var nonce = new NonceDescriptor();

			nonces.Add(nonce.Value, nonce);
			timeouts.Enqueue(nonce);

			return nonce.Value;
		}

		private bool IsValidNonce(string nonce)
		{
			return nonces.ContainsKey(nonce);
		}

		private void NonceTimer_EventHandler(Object stateInfo)
		{
			lock (syncRoot)
			{
				int now = Environment.TickCount;

				while (timeouts.Count > 0 && now - timeouts.Peek().Created > MaxLifetime.Milliseconds)
					nonces.Remove(timeouts.Dequeue().Value);
			}
		}
	}
}
