using System;
using GameDevWare.Serialization.MessagePack;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class BinarySerializer : TypeSerializer
	{
		public static readonly BinarySerializer Instance = new BinarySerializer();

		public override Type SerializedType { get { return typeof(byte[]); } }

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			if (reader.Token == JsonToken.Null)
				return null;

			if (reader.RawValue is byte[])
			{
				return reader.RawValue;
			}
			else
			{
				var value = reader.RawValue as string;
				if (value == null)
					return null;

				var buffer = Convert.FromBase64String(value);
				return buffer;
			}
		}

		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");

			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			if (value != null && value is byte[] == false) throw JsonSerializationException.TypeIsNotValid(this.GetType(), "be array of bytes");

			var bytes = (byte[])value;
			if (writer is MsgPackWriter)
			{
				((MsgPackWriter)writer).Write(bytes);
			}
			else
			{
				var base64String = Convert.ToBase64String(bytes);
				writer.WriteString(base64String);
			}
		}

		public override string ToString()
		{
			return "byte[] as Base64";
		}
	}
}
