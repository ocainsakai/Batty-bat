using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.MessagePack
{
	[Serializable]
	public sealed class UnknownMsgPackExtentionTypeException : SerializationException
	{
		public UnknownMsgPackExtentionTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public UnknownMsgPackExtentionTypeException(string message) : base(message)
		{
		}

		public UnknownMsgPackExtentionTypeException(sbyte invalidExtType)
			: base(string.Format("Unknown MessagePack extention type '{0}' was readed from stream.", invalidExtType))
		{
		}

		private UnknownMsgPackExtentionTypeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
