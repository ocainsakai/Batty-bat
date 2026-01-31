using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class VersionSerializer : TypeSerializer
	{
		public override Type SerializedType { get { return typeof(Version); } }

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			var versionStr = reader.ReadString(false);
			var value = new Version(versionStr);
			return value;
		}

		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			if (value == null) throw new ArgumentNullException("value");

			var version = (Version)value;
			writer.WriteString(version.ToString());
		}
	}
}
