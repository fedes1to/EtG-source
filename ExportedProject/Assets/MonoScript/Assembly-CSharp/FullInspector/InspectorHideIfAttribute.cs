using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorHideIfAttribute : Attribute
	{
		public string ConditionalMemberName;

		public InspectorHideIfAttribute(string conditionalMemberName)
		{
			ConditionalMemberName = conditionalMemberName;
		}
	}
}
