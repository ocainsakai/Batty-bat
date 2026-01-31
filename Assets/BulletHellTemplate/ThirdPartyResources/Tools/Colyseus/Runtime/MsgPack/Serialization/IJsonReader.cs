// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public interface IJsonReader
	{
		SerializationContext Context { get; }

		JsonToken Token { get; }
		object RawValue { get; }
		IValueInfo Value { get; }

		bool NextToken();

		bool IsEndOfStream();

		/// <summary>
		///     Resets Line/Column numbers, CharactersRead and Token information of reader
		/// </summary>
		void Reset();
	}
}
