using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class InspectorButtonAttribute : Attribute
	{
		[Obsolete("Please use InspectorName to get the custom name of the button")]
		public string DisplayName;

		public InspectorButtonAttribute()
		{
		}

		[Obsolete("Please use InspectorName to set the name of the button")]
		public InspectorButtonAttribute(string displayName)
		{
		}
	}
}
