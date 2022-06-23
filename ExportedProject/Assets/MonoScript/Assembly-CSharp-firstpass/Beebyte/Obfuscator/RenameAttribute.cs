using System;

namespace Beebyte.Obfuscator
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public class RenameAttribute : Attribute
	{
		private readonly string target;

		private RenameAttribute()
		{
		}

		public RenameAttribute(string target)
		{
			this.target = target;
		}

		public string GetTarget()
		{
			return target;
		}
	}
}
