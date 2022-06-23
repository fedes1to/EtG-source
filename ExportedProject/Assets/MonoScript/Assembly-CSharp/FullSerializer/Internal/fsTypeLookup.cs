using System;
using System.Reflection;

namespace FullSerializer.Internal
{
	internal static class fsTypeLookup
	{
		public static Type GetType(string typeName)
		{
			Type type = null;
			type = Type.GetType(typeName);
			if (type != null)
			{
				return type;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				type = assembly.GetType(typeName);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}
	}
}
