using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public sealed class JsonStreamReader : JsonReader
	{
		private readonly StreamReader reader;

		public JsonStreamReader(Stream stream, SerializationContext context, char[] buffer = null)
			: base(context, buffer)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (!stream.CanRead) throw JsonSerializationException.StreamIsNotReadable();

			this.reader = new StreamReader(stream, context.Encoding);
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
