using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[fiInspectorOnly]
public abstract class AssetBundleDatabase<T, U> : ScriptableObject where T : Object where U : AssetBundleDatabaseEntry
{
	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<U> Entries;

	public U InternalGetDataByGuid(string guid)
	{
		int i = 0;
		for (int count = Entries.Count; i < count; i++)
		{
			U val = Entries[i];
			if (val != null && val.myGuid == guid)
			{
				return val;
			}
		}
		return (U)null;
	}

	public virtual void DropReferences()
	{
		int i = 0;
		for (int count = Entries.Count; i < count; i++)
		{
			U val = Entries[i];
			val.DropReference();
		}
	}
}
