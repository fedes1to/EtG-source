using System;

namespace FullSerializer
{
	public sealed class fsDuplicateVersionNameException : Exception
	{
		public fsDuplicateVersionNameException(Type typeA, Type typeB, string version)
			: base(string.Concat(typeA, " and ", typeB, " have the same version string (", version, "); please change one of them."))
		{
		}
	}
}
