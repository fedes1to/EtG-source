using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorRangeAttribute : Attribute
	{
		public float Min;

		public float Max;

		public float Step = float.NaN;

		public InspectorRangeAttribute(float min, float max)
		{
			Min = min;
			Max = max;
		}
	}
}
