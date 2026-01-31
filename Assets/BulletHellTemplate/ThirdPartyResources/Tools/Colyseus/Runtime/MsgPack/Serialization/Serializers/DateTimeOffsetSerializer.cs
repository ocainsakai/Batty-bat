using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class DateTimeOffsetSerializer : TypeSerializer
	{
		public override Type SerializedType { get { return typeof(DateTimeOffset); } }

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			if (reader.Value.Raw is DateTimeOffset)
				return reader.Value.Raw;
			else if (reader.Token == JsonToken.DateTime || reader.Value.Raw is DateTime)
				return new DateTimeOffset(reader.Value.AsDateTime);

			var dateTimeOffsetStr = reader.ReadString(false);
			try
			{
				var value = default(DateTimeOffset);
				if (!DateTimeOffset.TryParse(dateTimeOffsetStr, reader.Context.Format, DateTimeStyles.RoundtripKind, out value))
					value = DateTimeOffset.ParseExact(dateTimeOffsetStr, reader.Context.DateTimeFormats, reader.Context.Format, DateTimeStyles.RoundtripKind);

				return value;
			}
			catch (FormatException fe)
			{
				throw new SerializationException(string.Format("Failed to parse date '{0}' in with pattern '{1}'.", dateTimeOffsetStr, reader.Context.DateTimeFormats[0]), fe);
			}
		}

		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			if (value == null) throw new ArgumentNullException("value");

			var dateTimeOffset = (DateTimeOffset)value;
			writer.Write(dateTimeOffset);
		}
	}
}
