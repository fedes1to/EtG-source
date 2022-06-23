using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorTextAreaAttribute : Attribute
	{
		public float Height;

		public InspectorTextAreaAttribute()
			: this(250f)
		{
		}

		public InspectorTextAreaAttribute(float height)
		{
			Height = height;
		}
	}
}
