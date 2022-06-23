using UnityEngine;

public class StringTableStringAttribute : PropertyAttribute
{
	public enum TargetStringTableType
	{
		DEFAULT,
		ENEMIES,
		ITEMS,
		UI
	}

	public string stringTableTarget;

	public bool isInToggledState;

	public string keyToWrite = string.Empty;

	public TargetStringTableType targetStringTable;

	public StringTableStringAttribute(string tableTarget = null)
	{
		stringTableTarget = tableTarget;
	}
}
