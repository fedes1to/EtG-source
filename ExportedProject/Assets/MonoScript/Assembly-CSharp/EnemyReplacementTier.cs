using System;
using System.Collections.Generic;

[Serializable]
public class EnemyReplacementTier
{
	public GlobalDungeonData.ValidTilesets TargetTileset;

	public float ChanceToReplace = 0.2f;

	public int MinRoomWidth = -1;

	[EnemyIdentifier]
	public List<string> TargetGUIDs;

	[EnemyIdentifier]
	public List<string> ReplacementGUIDs;
}
