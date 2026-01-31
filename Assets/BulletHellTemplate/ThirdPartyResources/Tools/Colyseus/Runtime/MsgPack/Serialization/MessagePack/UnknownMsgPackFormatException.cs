using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.MessagePack
{
	[Serializable]
	public sealed class UnknownMsgPackFormatException : SerializationException
	{
		public UnknownMsgPackFormatException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public UnknownMsgPackFormatException(string message) : base(message)
		{
		}

		public UnknownMsgPackFormatException(byte invalidValue)
			: base(string.Format("Unknown MessagePack format '{0}' was readed from stream.", invalidValue))
		{
		}

#if !NETSTANDARD
		private UnknownMsgPackFormatException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
