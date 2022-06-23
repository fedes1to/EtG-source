using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericRoomTable : ScriptableObject
{
	public WeightedRoomCollection includedRooms;

	public List<GenericRoomTable> includedRoomTables;

	[NonSerialized]
	protected List<WeightedRoom> m_compiledList;

	[NonSerialized]
	protected WeightedRoomCollection m_compiledCollection;

	public WeightedRoom SelectByWeight()
	{
		return GetCompiledCollection().SelectByWeight();
	}

	public WeightedRoom SelectByWeightWithoutDuplicates(List<PrototypeDungeonRoom> extant)
	{
		return GetCompiledCollection().SelectByWeightWithoutDuplicates(extant);
	}

	public List<WeightedRoom> GetCompiledList()
	{
		if (m_compiledList != null)
		{
			return m_compiledList;
		}
		List<WeightedRoom> list = new List<WeightedRoom>();
		for (int i = 0; i < includedRooms.elements.Count; i++)
		{
			list.Add(includedRooms.elements[i]);
		}
		for (int j = 0; j < includedRoomTables.Count; j++)
		{
			WeightedRoomCollection compiledCollection = includedRoomTables[j].GetCompiledCollection();
			for (int k = 0; k < compiledCollection.elements.Count; k++)
			{
				list.Add(compiledCollection.elements[k]);
			}
		}
		if (Application.isPlaying)
		{
			m_compiledList = list;
		}
		return list;
	}

	protected WeightedRoomCollection GetCompiledCollection()
	{
		WeightedRoomCollection weightedRoomCollection = new WeightedRoomCollection();
		for (int i = 0; i < includedRooms.elements.Count; i++)
		{
			weightedRoomCollection.Add(includedRooms.elements[i]);
		}
		for (int j = 0; j < includedRoomTables.Count; j++)
		{
			WeightedRoomCollection compiledCollection = includedRoomTables[j].GetCompiledCollection();
			for (int k = 0; k < compiledCollection.elements.Count; k++)
			{
				weightedRoomCollection.Add(compiledCollection.elements[k]);
			}
		}
		m_compiledCollection = weightedRoomCollection;
		return weightedRoomCollection;
	}
}
