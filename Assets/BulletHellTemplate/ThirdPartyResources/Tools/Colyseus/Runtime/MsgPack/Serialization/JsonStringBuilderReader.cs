using System;
using System.Text;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization
{
	public sealed class JsonStringBuilderReader : JsonReader
	{
		private readonly StringBuilder jsonString;
		private int position;

		public JsonStringBuilderReader(StringBuilder stringBuilder, SerializationContext context, char[] buffer = null)
			: base(context, buffer)
		{
			if (stringBuilder == null)
				throw new ArgumentNullException("str");


			this.jsonString = stringBuilder;
			this.position = 0;
		}

		protected override int FillBuffer(char[] buffer, int index)
		{
			if (buffer == null)
				throw new ArgumentNullException("buffer");
			if (index < 0 || index >= buffer.Length)
				throw new ArgumentOutOfRangeException("index");


			var block = Math.Min(this.jsonString.Length - position, buffer.Length - index);
			if (block <= 0)
				return index;

			jsonString.CopyTo(position, buffer, index, block);

			position += block;

			return index + block;
		}
	}
}
