using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	[Obsolete("Please use [InspectorNullable] instead of [InspectorNotDefaultConstructed]")]
	public sealed class InspectorNotDefaultConstructedAttribute : Attribute
	{
	}
}
