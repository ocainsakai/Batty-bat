using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Metadata
{
	internal abstract class DataMemberDescription : MemberDescription
	{
		public abstract bool CanGet { get; }
		public abstract bool CanSet { get; }
		public object DefaultValue { get; private set; }
		public abstract Type ValueType { get; }

		protected DataMemberDescription(TypeDescription typeDescription, MemberInfo member)
			: base(typeDescription, member)
		{
			var defaultValue =
				(DefaultValueAttribute) this.GetAttributesOrEmptyList(typeof (DefaultValueAttribute)).FirstOrDefault();
			if (defaultValue != null)
				this.DefaultValue = defaultValue.Value;
		}

		public abstract object GetValue(object target);
		public abstract void SetValue(object target, object value);
	}
}
