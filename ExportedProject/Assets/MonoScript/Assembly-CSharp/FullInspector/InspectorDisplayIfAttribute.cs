using System;

namespace FullInspector
{
	[Obsolete("Please use InspectorShowIfAttribute instead")]
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorDisplayIfAttribute : Attribute
	{
		public string ConditionalMemberName;

		public InspectorDisplayIfAttribute(string conditionalMemberName)
		{
			ConditionalMemberName = conditionalMemberName;
		}
	}
}
