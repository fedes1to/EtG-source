using System;
using System.Linq;
using System.Reflection;
using FullInspector.Internal;
using FullSerializer.Internal;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorCollectionItemAttributesAttribute : Attribute
	{
		public MemberInfo AttributeProvider;

		public InspectorCollectionItemAttributesAttribute(Type attributes)
		{
			if (!typeof(fiICollectionAttributeProvider).Resolve().IsAssignableFrom(attributes.Resolve()))
			{
				throw new ArgumentException("Must be an instance of FullInspector.fiICollectionAttributeProvider", "attributes");
			}
			fiICollectionAttributeProvider fiICollectionAttributeProvider2 = (fiICollectionAttributeProvider)Activator.CreateInstance(attributes);
			AttributeProvider = fiAttributeProvider.Create(fiICollectionAttributeProvider2.GetAttributes().ToArray());
		}
	}
}
