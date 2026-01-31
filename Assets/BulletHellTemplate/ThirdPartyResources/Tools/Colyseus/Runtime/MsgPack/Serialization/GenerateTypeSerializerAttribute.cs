using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	public class GenerateTypeSerializerAttribute : Attribute
	{
	}
}
