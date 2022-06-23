using UnityEngine;

public class PlatformInterfaceGenericPC : PlatformInterface
{
	protected override void OnStart()
	{
		Debug.Log("Starting Generic PC platform interface.");
	}

	protected override void OnAchievementUnlock(Achievement achievement, int playerIndex)
	{
	}

	protected override void OnLateUpdate()
	{
	}

	protected override StringTableManager.GungeonSupportedLanguages OnGetPreferredLanguage()
	{
		return StringTableManager.GungeonSupportedLanguages.ENGLISH;
	}
}
