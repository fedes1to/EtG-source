using System;
using UnityEngine;

namespace FullInspector.Internal
{
	public abstract class fiPersistentEditorStorageMetadataProvider<TItem, TStorage> : fiIPersistentMetadataProvider where TItem : new()where TStorage : fiIGraphMetadataStorage, new()
	{
		public Type MetadataType
		{
			get
			{
				return typeof(TItem);
			}
		}

		public void RestoreData(UnityEngine.Object target)
		{
			fiPersistentEditorStorage.Read<TStorage>(target).RestoreData(target);
		}

		public void Reset(UnityEngine.Object target)
		{
			fiPersistentEditorStorage.Reset<TStorage>(target);
		}
	}
}
