using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InspectorDisabledAttribute : Attribute
	{
	}
}
