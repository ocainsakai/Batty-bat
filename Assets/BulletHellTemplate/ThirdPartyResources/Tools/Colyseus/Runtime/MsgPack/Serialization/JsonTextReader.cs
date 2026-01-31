using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public sealed class JsonTextReader : JsonReader
	{
		private readonly TextReader reader;

		public JsonTextReader(TextReader reader, SerializationContext context, char[] buffer = null)
			: base(context, buffer)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");


			this.reader = reader;
		}

		protected override int FillBuffer(char[] buffer, int index)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (index < 0 || index >= buffer.Length)
				throw new ArgumentOutOfRangeException("index");


			var count = buffer.Length - index;
			if (count <= 0)
				return index;

			var read = this.reader.Read(buffer, index, count);
			return index + read;
		}
	}
}
