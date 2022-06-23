using System;
using System.Collections.Generic;
using System.Reflection;

namespace FullInspector.Internal
{
	public static class TypeCache
	{
		private static Dictionary<string, Type> _cachedTypes;

		private static Dictionary<string, Assembly> _assembliesByName;

		private static List<Assembly> _assembliesByIndex;

		static TypeCache()
		{
			_cachedTypes = new Dictionary<string, Type>();
			_assembliesByName = new Dictionary<string, Assembly>();
			_assembliesByIndex = new List<Assembly>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				_assembliesByName[assembly.FullName] = assembly;
				_assembliesByIndex.Add(assembly);
			}
			_cachedTypes = new Dictionary<string, Type>();
			AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
		}

		private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
		{
			_assembliesByName[args.LoadedAssembly.FullName] = args.LoadedAssembly;
			_assembliesByIndex.Add(args.LoadedAssembly);
			_cachedTypes = new Dictionary<string, Type>();
		}

		private static bool TryDirectTypeLookup(string assemblyName, string typeName, out Type type)
		{
			Assembly value;
			if (assemblyName != null && _assembliesByName.TryGetValue(assemblyName, out value))
			{
				type = value.GetType(typeName, false);
				return type != null;
			}
			type = null;
			return false;
		}

		private static bool TryIndirectTypeLookup(string typeName, out Type type)
		{
			for (int i = 0; i < _assembliesByIndex.Count; i++)
			{
				Assembly assembly = _assembliesByIndex[i];
				type = assembly.GetType(typeName);
				if (type != null)
				{
					return true;
				}
				Type[] types = assembly.GetTypes();
				foreach (Type type2 in types)
				{
					if (type2.FullName == typeName)
					{
						type = type2;
						return true;
					}
				}
			}
			type = null;
			return false;
		}

		public static void Reset()
		{
			_cachedTypes = new Dictionary<string, Type>();
		}

		public static Type FindType(string name)
		{
			return FindType(name, null);
		}

		public static Type FindType(string name, string assemblyHint)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			Type value;
			if (!_cachedTypes.TryGetValue(name, out value))
			{
				if (TryDirectTypeLookup(assemblyHint, name, out value) || !TryIndirectTypeLookup(name, out value))
				{
				}
				_cachedTypes[name] = value;
			}
			return value;
		}
	}
}
