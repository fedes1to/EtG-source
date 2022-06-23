using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorCollectionShowItemDropdownAttribute : Attribute
	{
		public bool IsCollapsedByDefault = true;
	}
}
