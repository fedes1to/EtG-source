using System;

namespace FullInspector
{
	[Obsolete("Please use [InspectorDatabaseEditor] instead")]
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class SingleItemListEditorAttribute : Attribute
	{
	}
}
