using System;
using System.Collections.Generic;
using System.Reflection;

namespace FullSerializer.Internal
{
	public static class fsVersionManager
	{
		private static readonly Dictionary<Type, fsOption<fsVersionedType>> _cache = new Dictionary<Type, fsOption<fsVersionedType>>();

		public static fsResult GetVersionImportPath(string currentVersion, fsVersionedType targetVersion, out List<fsVersionedType> path)
		{
			path = new List<fsVersionedType>();
			if (!GetVersionImportPathRecursive(path, currentVersion, targetVersion))
			{
				return fsResult.Fail("There is no migration path from \"" + currentVersion + "\" to \"" + targetVersion.VersionString + "\"");
			}
			path.Add(targetVersion);
			return fsResult.Success;
		}

		private static bool GetVersionImportPathRecursive(List<fsVersionedType> path, string currentVersion, fsVersionedType current)
		{
			for (int i = 0; i < current.Ancestors.Length; i++)
			{
				fsVersionedType fsVersionedType2 = current.Ancestors[i];
				if (fsVersionedType2.VersionString == currentVersion || GetVersionImportPathRecursive(path, currentVersion, fsVersionedType2))
				{
					path.Add(fsVersionedType2);
					return true;
				}
			}
			return false;
		}

		public static fsOption<fsVersionedType> GetVersionedType(Type type)
		{
			fsOption<fsVersionedType> value;
			if (!_cache.TryGetValue(type, out value))
			{
				fsObjectAttribute attribute = fsPortableReflection.GetAttribute<fsObjectAttribute>(type);
				if (attribute != null && (!string.IsNullOrEmpty(attribute.VersionString) || attribute.PreviousModels != null))
				{
					if (attribute.PreviousModels != null && string.IsNullOrEmpty(attribute.VersionString))
					{
						throw new Exception(string.Concat("fsObject attribute on ", type, " contains a PreviousModels specifier - it must also include a VersionString modifier"));
					}
					fsVersionedType[] array = new fsVersionedType[(attribute.PreviousModels != null) ? attribute.PreviousModels.Length : 0];
					for (int i = 0; i < array.Length; i++)
					{
						fsOption<fsVersionedType> versionedType = GetVersionedType(attribute.PreviousModels[i]);
						if (versionedType.IsEmpty)
						{
							throw new Exception(string.Concat("Unable to create versioned type for ancestor ", versionedType, "; please add an [fsObject(VersionString=\"...\")] attribute"));
						}
						array[i] = versionedType.Value;
					}
					fsVersionedType fsVersionedType2 = default(fsVersionedType);
					fsVersionedType2.Ancestors = array;
					fsVersionedType2.VersionString = attribute.VersionString;
					fsVersionedType2.ModelType = type;
					fsVersionedType fsVersionedType3 = fsVersionedType2;
					VerifyUniqueVersionStrings(fsVersionedType3);
					VerifyConstructors(fsVersionedType3);
					value = fsOption.Just(fsVersionedType3);
				}
				_cache[type] = value;
			}
			return value;
		}

		private static void VerifyConstructors(fsVersionedType type)
		{
			ConstructorInfo[] declaredConstructors = type.ModelType.GetDeclaredConstructors();
			for (int i = 0; i < type.Ancestors.Length; i++)
			{
				Type modelType = type.Ancestors[i].ModelType;
				bool flag = false;
				for (int j = 0; j < declaredConstructors.Length; j++)
				{
					ParameterInfo[] parameters = declaredConstructors[j].GetParameters();
					if (parameters.Length == 1 && parameters[0].ParameterType == modelType)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw new fsMissingVersionConstructorException(type.ModelType, modelType);
				}
			}
		}

		private static void VerifyUniqueVersionStrings(fsVersionedType type)
		{
			Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
			Queue<fsVersionedType> queue = new Queue<fsVersionedType>();
			queue.Enqueue(type);
			while (queue.Count > 0)
			{
				fsVersionedType fsVersionedType2 = queue.Dequeue();
				if (dictionary.ContainsKey(fsVersionedType2.VersionString) && dictionary[fsVersionedType2.VersionString] != fsVersionedType2.ModelType)
				{
					throw new fsDuplicateVersionNameException(dictionary[fsVersionedType2.VersionString], fsVersionedType2.ModelType, fsVersionedType2.VersionString);
				}
				dictionary[fsVersionedType2.VersionString] = fsVersionedType2.ModelType;
				fsVersionedType[] ancestors = fsVersionedType2.Ancestors;
				foreach (fsVersionedType item in ancestors)
				{
					queue.Enqueue(item);
				}
			}
		}
	}
}
