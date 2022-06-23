using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InspectorKeyWidthAttribute : Attribute
	{
		public float WidthPercentage;

		public InspectorKeyWidthAttribute(float widthPercentage)
		{
			if (widthPercentage < 0f || widthPercentage >= 1f)
			{
				throw new ArgumentException("widthPercentage must be between [0,1]");
			}
			WidthPercentage = widthPercentage;
		}
	}
}
