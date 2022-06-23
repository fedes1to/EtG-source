using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ShowInInspectorAttribute : Attribute
	{
	}
}
