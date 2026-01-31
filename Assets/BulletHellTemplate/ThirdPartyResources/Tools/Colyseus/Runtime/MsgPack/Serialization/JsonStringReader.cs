using System;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public sealed class JsonStringReader : JsonReader
	{
		private readonly string jsonString;
		private int position;

		public JsonStringReader(string jsonString, SerializationContext context, char[] buffer = null)
			: base(context, buffer)
		{
			if (jsonString == null)
				throw new ArgumentNullException("jsonString");


			this.jsonString = jsonString;
			this.position = 0;
		}

		protected override int FillBuffer(char[] buffer, int index)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (index < 0 || index >= buffer.Length)
				throw new ArgumentOutOfRangeException("index");


			var block = Math.Min(this.jsonString.Length - this.position, buffer.Length - index);
			if (block <= 0)
				return index;

			this.jsonString.CopyTo(this.position, buffer, index, block);

			this.position += block;

			return index + block;
		}
	}
}
