using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	[Flags]
	public enum SerializationOptions
	{
		None = 0,
		SuppressTypeInformation = 0x1 << 1,
		PrettyPrint = 0x1 << 2,
	}
}
