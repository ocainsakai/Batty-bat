using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace GameDevWare.Serialization.Metadata
{
	internal abstract class MemberDescription
	{
		protected const string DATA_CONTRACT_ATTRIBUTE_NAME = "DataContractAttribute";
		protected const string DATA_MEMBER_ATTRIBUTE_NAME = "DataMemberAttribute";
		protected const string IGNORE_DATA_MEMBER_ATTRIBUTE_NAME = "IgnoreDataMemberAttribute";

		private readonly string name;
		private readonly MemberInfo member;
		private readonly ReadOnlyCollection<Attribute> attributes;
		private readonly ILookup<Type, Attribute> attributesByType;

		public MemberInfo Member { get { return this.member; } }
		public ReadOnlyCollection<Attribute> Attributes { get { return this.attributes; } }
		public string Name { get { return this.name; } }

		protected MemberDescription(TypeDescription typeDescription, MemberInfo member)
		{
			if (member == null) throw new ArgumentNullException("member");

			this.member = member;
			this.name = member.Name;

			var attributesList = new List<Attribute>();
			foreach (Attribute attr in member.GetCustomAttributes(true))
				attributesList.Add(attr);

			if (typeDescription != null && typeDescription.IsDataContract)
			{
				var dataMemberAttribute = attributesList.FirstOrDefault(a => a.GetType().Name == DATA_MEMBER_ATTRIBUTE_NAME);
				if (dataMemberAttribute != null)
					this.name = ReflectionExtensions.GetDataMemberName(dataMemberAttribute) ?? this.name;
			}

			this.attributes = new ReadOnlyCollection<Attribute>(attributesList);
			this.attributesByType = attributesList.ToLookup(a => a.GetType());
		}

		public bool HasAttributes(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			return this.attributesByType.Contains(type);
		}

		public IEnumerable<Attribute> GetAttributesOrEmptyList(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			return this.attributesByType[type];
		}
	}
}
