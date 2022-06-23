using System;

namespace FullSerializer
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class fsPropertyAttribute : Attribute
	{
		public string Name;

		public bool DeserializeOnly;

		public fsPropertyAttribute()
			: this(string.Empty)
		{
		}

		public fsPropertyAttribute(string name)
		{
			Name = name;
		}

		public fsPropertyAttribute(bool deserializeOnly)
		{
			DeserializeOnly = deserializeOnly;
		}
	}
}
