using System;
using GameDevWare.Serialization.MessagePack;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class GuidSerializer : TypeSerializer
	{
		public override Type SerializedType { get { return typeof(Guid); } }

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			var guidStr = reader.ReadString(false);
			var value = new Guid(guidStr);
			return value;
		}

		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			if (value == null) throw new ArgumentNullException("value");

			var messagePackWriter = writer as MsgPackWriter;
			if (messagePackWriter != null)
			{
				// try to write it as Message Pack extension type
				var extensionType = default(sbyte);
				var buffer = messagePackWriter.GetWriteBuffer();
				if (messagePackWriter.Context.ExtensionTypeHandler.TryWrite(value, out extensionType, ref buffer))
				{
					messagePackWriter.Write(extensionType, buffer);
					return;
				}
				// if not, continue default serialization
			}

			var guid = (Guid)value;
			var guidStr = guid.ToString();
			writer.Write(guidStr);
		}
	}
}
