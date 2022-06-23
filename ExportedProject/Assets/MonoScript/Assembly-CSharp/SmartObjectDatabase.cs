using System;
using System.Collections.Generic;
using FullInspector;
using UnityEngine;
using UnityEngine.Serialization;

[fiInspectorOnly]
public abstract class SmartObjectDatabase<T, U> : ScriptableObject where T : UnityEngine.Object where U : DatabaseEntry
{
	[InspectorCollectionRotorzFlags(DisableReordering = true, ShowIndices = true)]
	public List<T> Objects;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	[FormerlySerializedAs("GoodObjects")]
	public List<U> Entries;

	public T InternalGetByName(string name)
	{
		U val = Entries.Find((U obj) => obj != null && obj.name.Equals(name, StringComparison.OrdinalIgnoreCase));
		return (val == null) ? ((T)null) : val.GetPrefab<T>();
	}

	public T InternalGetByGuid(string guid)
	{
		U val = Entries.Find((U ds) => ds != null && ds.myGuid == guid);
		return (val == null) ? ((T)null) : val.GetPrefab<T>();
	}

	public U InternalGetDataByGuid(string guid)
	{
		return Entries.Find((U ds) => ds != null && ds.myGuid == guid);
	}

	public void DropReferences()
	{
		for (int i = 0; i < Entries.Count; i++)
		{
			U val = Entries[i];
			val.DropReference();
		}
	}
}
