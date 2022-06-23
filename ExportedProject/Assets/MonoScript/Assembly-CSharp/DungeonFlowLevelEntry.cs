using System;

[Serializable]
public class DungeonFlowLevelEntry
{
	public string flowPath;

	public float weight = 1f;

	public FlowLevelEntryMode levelMode;

	public DungeonPrerequisite[] prerequisites;

	public bool forceUseIfAvailable;

	public bool overridesLevelDetailText;

	public string overrideLevelDetailText = string.Empty;
}
