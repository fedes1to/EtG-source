using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorShowIfAttribute : Attribute
	{
		public string ConditionalMemberName;

		public InspectorShowIfAttribute(string conditionalMemberName)
		{
			ConditionalMemberName = conditionalMemberName;
		}
	}
}
