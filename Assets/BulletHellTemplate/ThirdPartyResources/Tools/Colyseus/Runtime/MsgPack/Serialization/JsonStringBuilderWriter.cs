using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public sealed class JsonStringBuilderWriter : JsonWriter
	{
		private readonly StringBuilder stringBuilder;

		public StringBuilder Builder
		{
			get { return stringBuilder; }
		}

		public JsonStringBuilderWriter(StringBuilder stringBuilder, SerializationContext context, char[] buffer = null)
			: base(context, buffer)
		{
			if (stringBuilder == null)
				throw new ArgumentNullException("builder");


			this.stringBuilder = stringBuilder;
		}


		public override void Flush()
		{
		}

		public override void WriteJson(string jsonString)
		{
			if (jsonString == null)
				throw new ArgumentNullException("jsonString");


			stringBuilder.Append(jsonString);
			this.CharactersWritten += jsonString.Length;
		}

		public override void WriteJson(char[] jsonString, int offset, int charactersToWrite)
		{
			if (jsonString == null)
				throw new ArgumentNullException("jsonString");
			if (offset < 0 || offset >= jsonString.Length)
				throw new ArgumentOutOfRangeException("offset");
			if (charactersToWrite < 0 || offset + charactersToWrite > jsonString.Length)
				throw new ArgumentOutOfRangeException("charactersToWrite");


			if (charactersToWrite == 0)
				return;

			stringBuilder.Append(jsonString, offset, charactersToWrite);
			this.CharactersWritten += charactersToWrite;
		}

		public override string ToString()
		{
			return stringBuilder.ToString();
		}
	}
}
