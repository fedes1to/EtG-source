using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class MetalGearRatRoomController : MonoBehaviour
{
	public GameObject brokenMetalGear;

	public GameObject floorCover;

	public IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		RoomHandler thisRoom = base.transform.position.GetAbsoluteRoom();
		RoomHandler targetRoom = null;
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			Transform hierarchyParent = GameManager.Instance.Dungeon.data.rooms[i].hierarchyParent;
			if ((bool)hierarchyParent && (bool)hierarchyParent.GetComponentInChildren<ResourcefulRatRewardRoomController>(true))
			{
				targetRoom = GameManager.Instance.Dungeon.data.rooms[i];
				break;
			}
		}
		thisRoom.TargetPitfallRoom = targetRoom;
		thisRoom.ForcePitfallForFliers = true;
		thisRoom.OnTargetPitfallRoom = (Action)Delegate.Combine(thisRoom.OnTargetPitfallRoom, new Action(HandlePitfallIntoReward));
		EnablePitfalls(false);
	}

	private void HandlePitfallIntoReward()
	{
		GameManager.Instance.Dungeon.StartCoroutine(HandlePitfallIntoRewardCR());
	}

	private IEnumerator HandlePitfallIntoRewardCR()
	{
		int numPlayers = GameManager.Instance.AllPlayers.Length;
		for (int i = 0; i < numPlayers; i++)
		{
			GameManager.Instance.AllPlayers[i].SetInputOverride("lich transition");
		}
		Pixelator.Instance.FadeToBlack(1f, true);
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			playerController.DoSpinfallSpawn(0.5f);
			playerController.WarpFollowersToPlayer();
		}
		float timer = 0f;
		for (float duration = 2f; timer < duration; timer += BraveTime.DeltaTime)
		{
			yield return null;
		}
		if (GameManager.HasInstance)
		{
			for (int k = 0; k < numPlayers; k++)
			{
				GameManager.Instance.AllPlayers[k].ClearInputOverride("lich transition");
			}
		}
	}

	public void EnablePitfalls(bool value)
	{
		floorCover.SetActive(!value);
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		IntVector2 intVector = absoluteRoom.area.basePosition + new IntVector2(19, 12);
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				IntVector2 key = intVector + new IntVector2(i, j);
				GameManager.Instance.Dungeon.data[key].fallingPrevented = !value;
			}
		}
	}

	public void TransformToDestroyedRoom()
	{
		brokenMetalGear.SetActive(true);
		EnablePitfalls(true);
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		if (absoluteRoom != null && absoluteRoom.DarkSoulsRoomResetDependencies != null)
		{
			absoluteRoom.DarkSoulsRoomResetDependencies.Clear();
		}
	}
}
