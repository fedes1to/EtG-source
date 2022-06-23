using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class PrototypeDungeonRoomCellAppearance
{
	[SerializeField]
	public int overrideDungeonMaterialIndex = -1;

	[SerializeField]
	public bool IsPhantomCarpet;

	[SerializeField]
	public bool ForceDisallowGoop;

	[SerializeField]
	public CellVisualData.CellFloorType OverrideFloorType;

	[SerializeField]
	public PrototypeIndexOverrideData globalOverrideIndices;

	[SerializeField]
	private List<PrototypeIndexOverrideData> m_overrideIndices;

	public PrototypeDungeonRoomCellAppearance()
	{
		globalOverrideIndices = new PrototypeIndexOverrideData();
		m_overrideIndices = new List<PrototypeIndexOverrideData>();
	}

	public bool HasChanges()
	{
		return overrideDungeonMaterialIndex != -1 || IsPhantomCarpet || ForceDisallowGoop || OverrideFloorType != 0 || globalOverrideIndices.indices.Count != 0 || m_overrideIndices.Count != 0;
	}

	public bool HasAnyOverride()
	{
		for (int i = 0; i < m_overrideIndices.Count; i++)
		{
			if (m_overrideIndices[i].indices != null && m_overrideIndices[i].indices.Count != 0)
			{
				return true;
			}
		}
		return false;
	}

	public List<int> GetOverridesForTilemap(PrototypeDungeonRoom sourceRoom, GlobalDungeonData.ValidTilesets tileset)
	{
		if ((sourceRoom.overriddenTilesets & tileset) == tileset)
		{
			int num = Mathf.RoundToInt(Mathf.Log((float)tileset, 2f));
			while (num >= m_overrideIndices.Count)
			{
				m_overrideIndices.Add(new PrototypeIndexOverrideData());
			}
			if (m_overrideIndices[num].indices.Count > 0)
			{
				return m_overrideIndices[num].indices;
			}
			if (globalOverrideIndices.indices.Count > 0)
			{
				return globalOverrideIndices.indices;
			}
		}
		return null;
	}

	public void SetAllOverridesForTilemap(GlobalDungeonData.ValidTilesets tileset, List<int> overrides)
	{
		int num = Mathf.RoundToInt(Mathf.Log((float)tileset, 2f));
		PrototypeIndexOverrideData prototypeIndexOverrideData = new PrototypeIndexOverrideData();
		prototypeIndexOverrideData.indices = overrides;
		if (num < m_overrideIndices.Count)
		{
			m_overrideIndices[num] = prototypeIndexOverrideData;
			return;
		}
		while (num != m_overrideIndices.Count)
		{
			m_overrideIndices.Add(new PrototypeIndexOverrideData());
		}
		m_overrideIndices.Add(prototypeIndexOverrideData);
	}

	public void ClearOverrideForTilemap(GlobalDungeonData.ValidTilesets tileset)
	{
		int num = Mathf.RoundToInt(Mathf.Log((float)tileset, 2f));
		if (num < m_overrideIndices.Count)
		{
			m_overrideIndices[num] = new PrototypeIndexOverrideData();
		}
	}

	public void ClearAllOverrideData()
	{
		m_overrideIndices.Clear();
		globalOverrideIndices.indices.Clear();
	}

	public void MirrorData(PrototypeDungeonRoomCellAppearance source)
	{
		overrideDungeonMaterialIndex = source.overrideDungeonMaterialIndex;
		IsPhantomCarpet = source.IsPhantomCarpet;
		ForceDisallowGoop = source.ForceDisallowGoop;
		OverrideFloorType = source.OverrideFloorType;
		globalOverrideIndices = new PrototypeIndexOverrideData();
		globalOverrideIndices.indices = new List<int>();
		if (source.globalOverrideIndices.indices != null)
		{
			for (int i = 0; i < source.globalOverrideIndices.indices.Count; i++)
			{
				globalOverrideIndices.indices.Add(source.globalOverrideIndices.indices[i]);
			}
		}
		m_overrideIndices = new List<PrototypeIndexOverrideData>();
		for (int j = 0; j < source.m_overrideIndices.Count; j++)
		{
			m_overrideIndices.Add(new PrototypeIndexOverrideData());
			m_overrideIndices[j].indices = new List<int>();
			if (source.m_overrideIndices[j].indices != null)
			{
				for (int k = 0; k < source.m_overrideIndices[j].indices.Count; k++)
				{
					m_overrideIndices[j].indices.Add(source.m_overrideIndices[j].indices[k]);
				}
			}
		}
	}
}
