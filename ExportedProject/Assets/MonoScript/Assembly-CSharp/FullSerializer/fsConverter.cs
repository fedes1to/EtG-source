using System;

namespace FullSerializer
{
	public abstract class fsConverter : fsBaseConverter
	{
		public abstract bool CanProcess(Type type);
	}
}
