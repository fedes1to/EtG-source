using System;
using UnityEngine;

public static class BraveMemory
{
	private static float LastGcTime;

	public static void EnsureHeapSize(int kilobytes)
	{
		object[] array = new object[kilobytes];
		for (int i = 0; i < kilobytes; i++)
		{
			array[i] = new byte[1024];
		}
		array = null;
		GC.Collect();
	}

	public static void DoCollect()
	{
		LastGcTime = Time.realtimeSinceStartup;
		GC.Collect();
	}

	private static void MaybeDoCollect()
	{
		LastGcTime = Time.realtimeSinceStartup;
	}

	private static bool CheckTime(float threshold)
	{
		return Time.realtimeSinceStartup - LastGcTime > threshold;
	}

	private static void TestPC()
	{
		for (int i = 0; i < 5; i++)
		{
			GC.Collect();
		}
	}

	public static void HandleBossCardFlashAnticipation()
	{
		if (CheckTime(20f))
		{
			MaybeDoCollect();
			GenericIntroDoer.SkipFrame = true;
		}
	}

	public static void HandleBossCardSkip()
	{
		if (CheckTime(20f))
		{
			MaybeDoCollect();
			GenericIntroDoer.SkipFrame = true;
		}
	}

	public static void HandleRoomEntered(int numOfEnemies)
	{
		float threshold = 360f;
		if (numOfEnemies >= 8)
		{
			threshold = 240f;
		}
		if (CheckTime(threshold))
		{
			MaybeDoCollect();
		}
	}

	public static void HandleGamePaused()
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && CheckTime(360f))
		{
			MaybeDoCollect();
		}
	}

	public static void HandleTeleportation()
	{
		if (CheckTime(300f))
		{
			MaybeDoCollect();
		}
	}
}
