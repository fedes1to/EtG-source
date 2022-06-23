using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorCollectionPagerAttribute : Attribute
	{
		public int PageMinimumCollectionLength;

		public bool AlwaysHide
		{
			get
			{
				return PageMinimumCollectionLength < 0;
			}
			set
			{
				if (value)
				{
					PageMinimumCollectionLength = -1;
				}
				else
				{
					PageMinimumCollectionLength = fiSettings.DefaultPageMinimumCollectionLength;
				}
			}
		}

		public bool AlwaysShow
		{
			get
			{
				return PageMinimumCollectionLength == 0;
			}
			set
			{
				if (value)
				{
					PageMinimumCollectionLength = 0;
				}
				else
				{
					PageMinimumCollectionLength = fiSettings.DefaultPageMinimumCollectionLength;
				}
			}
		}

		public InspectorCollectionPagerAttribute()
		{
			PageMinimumCollectionLength = fiSettings.DefaultPageMinimumCollectionLength;
		}

		public InspectorCollectionPagerAttribute(int pageMinimumCollectionLength)
		{
			PageMinimumCollectionLength = pageMinimumCollectionLength;
		}
	}
}
