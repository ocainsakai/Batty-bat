// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public enum JsonToken
	{
		None = 0,
		BeginArray,
		EndOfArray,
		BeginObject,
		EndOfObject,
		Member,
		Number,
		StringLiteral,
		DateTime,
		Null,
		Boolean,
		EndOfStream
	}
}
