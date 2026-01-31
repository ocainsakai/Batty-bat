using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public abstract class TypeSerializer
	{
		public abstract Type SerializedType { get; }

		public abstract object Deserialize(IJsonReader reader);
		public abstract void Serialize(IJsonWriter writer, object value);
	}
}
