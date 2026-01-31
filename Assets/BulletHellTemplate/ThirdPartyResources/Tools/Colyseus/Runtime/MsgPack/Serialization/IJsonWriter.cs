using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public interface IJsonWriter
	{
		SerializationContext Context { get; }

		void Flush();

		void Write(string value);
		void Write(JsonMember value);
		void Write(int number);
		void Write(uint number);
		void Write(long number);
		void Write(ulong number);
		void Write(float number);
		void Write(double number);
		void Write(decimal number);
		void Write(bool value);
		void Write(DateTime dateTime);
		void Write(DateTimeOffset dateTimeOffset);
		void WriteObjectBegin(int numberOfMembers);
		void WriteObjectEnd();
		void WriteArrayBegin(int numberOfMembers);
		void WriteArrayEnd();
		void WriteNull();

		void WriteJson(string jsonString);
		void WriteJson(char[] jsonString, int index, int charCount);

		void Reset();
	}
}
