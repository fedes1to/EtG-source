using System;
using System.Linq;
using System.Reflection;
using FullSerializer.Internal;

namespace FullInspector.Internal
{
	public class fiAttributeProvider : MemberInfo
	{
		private readonly object[] _attributes;

		public override Type DeclaringType
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override MemberTypes MemberType
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override string Name
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override Type ReflectedType
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		private fiAttributeProvider(object[] attributes)
		{
			_attributes = attributes;
		}

		public static MemberInfo Create(params object[] attributes)
		{
			return new fiAttributeProvider(attributes);
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return _attributes;
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return _attributes.Where((object attr) => attr.GetType().Resolve().IsAssignableFrom(attributeType.Resolve())).ToArray();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			return _attributes.Where((object attr) => attr.GetType().Resolve().IsAssignableFrom(attributeType.Resolve())).Any();
		}
	}
}
