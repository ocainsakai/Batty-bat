using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public sealed class JsonStreamWriter : JsonWriter
	{
		private readonly StreamWriter writer;

		public Stream Stream { get { return writer.BaseStream; } }

		public JsonStreamWriter(Stream stream, SerializationContext context, char[] buffer = null)
			: base(context, buffer)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (!stream.CanWrite) throw JsonSerializationException.StreamIsNotWriteable();


			writer = new StreamWriter(stream, context.Encoding);
		}

		public override void Flush()
		{
			writer.Flush();
		}

		public override void WriteJson(string jsonString)
		{
			if (jsonString == null)
				throw new ArgumentNullException("jsonString");


			writer.Write(jsonString);
			this.CharactersWritten += jsonString.Length;
		}

		public override void WriteJson(char[] jsonString, int index, int charactersToWrite)
		{
			if (jsonString == null)
				throw new ArgumentNullException("jsonString");
			if (index < 0 || index >= jsonString.Length)
				throw new ArgumentOutOfRangeException("index");
			if (charactersToWrite < 0 || index + charactersToWrite > jsonString.Length)
				throw new ArgumentOutOfRangeException("charactersToWrite");


			if (charactersToWrite == 0)
				return;

			writer.Write(jsonString, index, charactersToWrite);
			this.CharactersWritten += charactersToWrite;
		}
	}
}
