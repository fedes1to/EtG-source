using System;

namespace FullInspector
{
	[Obsolete("Use [InspectorCollectionRotorzFlags(ShowIndices=true)] instead")]
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorCollectionShowIndicesAttribute : Attribute
	{
	}
}
