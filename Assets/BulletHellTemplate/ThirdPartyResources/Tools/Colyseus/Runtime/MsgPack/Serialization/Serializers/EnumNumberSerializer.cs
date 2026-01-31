using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class EnumNumberSerializer : TypeSerializer
	{
		private readonly Type enumType;
		private readonly Type enumBaseType;

		public override Type SerializedType { get { return this.enumType; } }

		public EnumNumberSerializer(Type enumType)
		{
			if (enumType == null) throw new ArgumentNullException("enumType");
			if (!enumType.IsEnum) throw JsonSerializationException.TypeIsNotValid(this.GetType(), "be a Enum");

			this.enumType = enumType;
			this.enumBaseType = Enum.GetUnderlyingType(enumType);
		}

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			if (reader.Token == JsonToken.StringLiteral)
				return Enum.Parse(this.enumType, reader.ReadString(false), true);
			else if (reader.Token == JsonToken.Number)
				return Enum.ToObject(this.enumType, reader.ReadValue(this.enumBaseType, false));
			else
				throw JsonSerializationException.UnexpectedToken(reader, JsonToken.Number, JsonToken.StringLiteral);
		}

		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			if (value == null) throw new ArgumentNullException("value");

			writer.WriteValue(Convert.ChangeType(value, this.enumBaseType), this.enumBaseType);
		}
	}
}
