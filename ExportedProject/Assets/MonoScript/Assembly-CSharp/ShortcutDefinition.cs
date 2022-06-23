using System;

[Serializable]
public struct ShortcutDefinition
{
	public string targetLevelName;

	[LongEnum]
	public GungeonFlags requiredFlag;

	public string sherpaTextKey;

	public string elevatorFloorSpriteName;

	public bool IsBossRush;

	public bool IsSuperBossRush;
}
