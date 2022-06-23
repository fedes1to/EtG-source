using System.Collections.Generic;
using System.Linq;
using FullInspector.Internal;
using FullSerializer;
using UnityEngine;

namespace FullInspector
{
	public static class fiPersistentMetadata
	{
		private static readonly fiIPersistentMetadataProvider[] s_providers;

		private static Dictionary<fiUnityObjectReference, fiGraphMetadata> s_metadata;

		static fiPersistentMetadata()
		{
			s_metadata = new Dictionary<fiUnityObjectReference, fiGraphMetadata>();
			s_providers = fiRuntimeReflectionUtility.GetAssemblyInstances<fiIPersistentMetadataProvider>().ToArray();
			for (int i = 0; i < s_providers.Length; i++)
			{
				fiLog.Log(typeof(fiPersistentMetadata), "Using provider {0} to support metadata of type {1}", s_providers[i].GetType().CSharpName(), s_providers[i].MetadataType.CSharpName());
			}
		}

		public static fiGraphMetadata GetMetadataFor(Object target_)
		{
			fiUnityObjectReference fiUnityObjectReference = new fiUnityObjectReference(target_);
			fiGraphMetadata value;
			if (!s_metadata.TryGetValue(fiUnityObjectReference, out value))
			{
				value = new fiGraphMetadata(fiUnityObjectReference);
				s_metadata[fiUnityObjectReference] = value;
				for (int i = 0; i < s_providers.Length; i++)
				{
					s_providers[i].RestoreData(fiUnityObjectReference.Target);
				}
			}
			return value;
		}

		public static void Reset(Object target_)
		{
			fiUnityObjectReference fiUnityObjectReference = new fiUnityObjectReference(target_);
			if (s_metadata.ContainsKey(fiUnityObjectReference))
			{
				s_metadata.Remove(fiUnityObjectReference);
				for (int i = 0; i < s_providers.Length; i++)
				{
					s_providers[i].Reset(fiUnityObjectReference.Target);
				}
			}
		}
	}
}
