using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.MessagePack
{
	public abstract class MessagePackExtensionTypeHandler
	{
		public abstract IEnumerable<Type> ExtensionTypes { get; }

		public abstract bool TryRead(sbyte type, ArraySegment<byte> data, out object value);
		public abstract bool TryWrite(object value, out sbyte type, ref ArraySegment<byte> data);

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("Extension Types: {0}", string.Join(", ", this.ExtensionTypes.Select(t => t.ToString()).ToArray()));
		}
	}
}
