using System;
using GameDevWare.Serialization.Serializers;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.MessagePack
{
	[TypeSerializer(typeof(MsgPackTimestampSerializer))]
	public struct MessagePackTimestamp : IEquatable<MessagePackTimestamp>, IComparable<MessagePackTimestamp>
	{
		public const int MAX_NANO_SECONDS = 999999999;

		public readonly long Seconds;
		public readonly uint NanoSeconds;

		public MessagePackTimestamp(long seconds, uint nanoSeconds)
		{
			if (nanoSeconds > MAX_NANO_SECONDS)
				nanoSeconds = MAX_NANO_SECONDS;

			this.Seconds = seconds;
			this.NanoSeconds = nanoSeconds;
		}

		public static explicit operator DateTime(MessagePackTimestamp timestamp)
		{
			return new DateTime(JsonUtils.UnixEpochTicks + ((TimeSpan)timestamp).Ticks, DateTimeKind.Unspecified);
		}
		public static explicit operator TimeSpan(MessagePackTimestamp timestamp)
		{
			return TimeSpan.FromSeconds(timestamp.Seconds) + TimeSpan.FromTicks(timestamp.NanoSeconds / 100);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return unchecked(this.Seconds.GetHashCode() * 17 + this.NanoSeconds.GetHashCode());
		}
		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (obj is MessagePackTimestamp)
				return this.Equals((MessagePackTimestamp)obj);
			else
				return false;
		}
		/// <inheritdoc />
		public bool Equals(MessagePackTimestamp other)
		{
			return this.Seconds.Equals(other.Seconds) && this.NanoSeconds.Equals(other.NanoSeconds);
		}
		/// <inheritdoc />
		public int CompareTo(MessagePackTimestamp other)
		{
			var cmp = this.Seconds.CompareTo(other.Seconds);
			if (cmp != 0)
				return cmp;
			return this.NanoSeconds.CompareTo(other.NanoSeconds);
		}

		public static bool operator >(MessagePackTimestamp a, MessagePackTimestamp b)
		{
			return a.CompareTo(b) == 1;
		}
		public static bool operator <(MessagePackTimestamp a, MessagePackTimestamp b)
		{
			return a.CompareTo(b) == -1;
		}
		public static bool operator >=(MessagePackTimestamp a, MessagePackTimestamp b)
		{
			return a.CompareTo(b) != -1;
		}
		public static bool operator <=(MessagePackTimestamp a, MessagePackTimestamp b)
		{
			return a.CompareTo(b) != 1;
		}
		public static bool operator ==(MessagePackTimestamp a, MessagePackTimestamp b)
		{
			return a.Equals(b);
		}
		public static bool operator !=(MessagePackTimestamp a, MessagePackTimestamp b)
		{
			return !a.Equals(b);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return string.Format("seconds: {0}, nanoseconds: {1}", this.Seconds, this.NanoSeconds);
		}
	}
}
