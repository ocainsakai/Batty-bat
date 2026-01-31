using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Metadata
{
	internal sealed class FieldDescription : DataMemberDescription
	{
		private readonly FieldInfo fieldInfo;
		private readonly Func<object, object> getFn;
		private readonly Action<object, object> setFn;

		public override bool CanGet { get { return true; } }
		public override bool CanSet { get { return this.fieldInfo.IsInitOnly == false; } }
		public override Type ValueType { get { return this.fieldInfo.FieldType; } }

		public FieldDescription(TypeDescription typeDescription, FieldInfo fieldInfo)
			: base(typeDescription, fieldInfo)
		{
			if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");

			this.fieldInfo = fieldInfo;

			MetadataReflection.TryGetMemberAccessFunc(fieldInfo, out this.getFn, out this.setFn);

		}

		public override object GetValue(object target)
		{
			if (!this.CanGet) throw new InvalidOperationException("Field is write-only.");

			if (this.getFn != null)
				return this.getFn(target);
			else
				return fieldInfo.GetValue(target);
		}

		public override void SetValue(object target, object value)
		{
			if (!this.CanSet) throw new InvalidOperationException("Field is read-only.");

			if (this.setFn != null)
				this.setFn(target, value);
			else
				this.fieldInfo.SetValue(target, value);
		}
	}
}
