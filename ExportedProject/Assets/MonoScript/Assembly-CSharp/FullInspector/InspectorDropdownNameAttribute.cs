using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	public sealed class InspectorDropdownNameAttribute : Attribute
	{
		public string DisplayName;

		public InspectorDropdownNameAttribute(string displayName)
		{
			DisplayName = displayName;
		}
	}
}
