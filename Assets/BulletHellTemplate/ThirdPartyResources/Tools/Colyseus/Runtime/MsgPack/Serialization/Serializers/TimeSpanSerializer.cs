using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class TimeSpanSerializer : TypeSerializer
	{
		public override Type SerializedType { get { return typeof(TimeSpan); } }

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			if (reader.Token == JsonToken.Number)
				return new TimeSpan(reader.Value.AsInt64);

			var timeSpanStr = reader.ReadString(false);
			var value = TimeSpan.Parse(timeSpanStr);
			return value;
		}

		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			if (value == null) throw new ArgumentNullException("value");

			var timeSpan = (TimeSpan)value;
			writer.Write((long)timeSpan.Ticks);
		}
	}
}
