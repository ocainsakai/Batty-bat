using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class UriSerializer : TypeSerializer
	{
		public override Type SerializedType { get { return typeof(Uri); } }

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			var uriStr = reader.ReadString(false);
			var value = new Uri(uriStr);
			return value;
		}

		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			if (value == null) throw new ArgumentNullException("value");

			var uri = (Uri)value;
			writer.WriteString(uri.OriginalString);
		}
	}
}
