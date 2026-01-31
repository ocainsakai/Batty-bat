#if UNITY_5 || UNITY_4 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_5_3_OR_NEWER
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Serializers
{
	public sealed class Vector2Serializer : TypeSerializer
	{
		public override Type SerializedType { get { return typeof(Vector2); } }

		public override object Deserialize(IJsonReader reader)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			if (reader.Token == JsonToken.Null)
				return null;

			var value = new Vector2();
			reader.ReadObjectBegin();
			while (reader.Token != JsonToken.EndOfObject)
			{
				var memberName = reader.ReadMember();
				switch (memberName)
				{
					case "x": value.x = reader.ReadSingle(); break;
					case "y": value.y = reader.ReadSingle(); break;
					default: reader.ReadValue(typeof(object)); break;
				}
			}
			reader.ReadObjectEnd(nextToken: false);
			return value;
		}
		public override void Serialize(IJsonWriter writer, object value)
		{
			if (writer == null) throw new ArgumentNullException("writer");
			if (value == null) throw new ArgumentNullException("value");

			var vector2 = (Vector2)value;
			writer.WriteObjectBegin(2);
			writer.WriteMember("x");
			writer.Write(vector2.x);
			writer.WriteMember("y");
			writer.Write(vector2.y);
			writer.WriteObjectEnd();
		}
	}
}
#endif
