using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public sealed class JsonTextWriter : JsonWriter
	{
		private readonly TextWriter writer;

		private TextWriter Writer
		{
			get { return writer; }
		}

		public JsonTextWriter(TextWriter writer, SerializationContext context, char[] buffer = null)
			: base(context, buffer)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");


			this.writer = writer;
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

			writer.Write(jsonString, offset, charactersToWrite);
			this.CharactersWritten += charactersToWrite;
		}

		public override string ToString()
		{
			return writer.ToString();
		}
	}
}
