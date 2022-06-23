using System;
using Dungeonator;
using UnityEngine;

[Serializable]
public class DungeonTilemapIndexData : ScriptableObject
{
	public tk2dSpriteCollectionData dungeonCollection;

	public AOTileIndices aoTileIndices;
}
