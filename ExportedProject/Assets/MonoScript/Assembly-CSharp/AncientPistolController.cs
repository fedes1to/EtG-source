using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class AncientPistolController : BraveBehaviour, IPlaceConfigurable
{
	[NonSerialized]
	public List<RoomHandler> RoomSequence;

	public List<bool> RoomSequenceEnemies;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		StartCoroutine(HandleDelayedInitialization(room));
	}

	private IEnumerator HandleDelayedInitialization(RoomHandler room)
	{
		yield return null;
		room.TransferInteractableOwnershipToDungeon(base.talkDoer);
		RoomSequence = new List<RoomHandler>();
		RoomSequenceEnemies = new List<bool>();
		for (int i = 1; i < room.injectionFrameData.Count; i++)
		{
			RoomSequence.Add(room.injectionFrameData[i]);
			RoomSequenceEnemies.Add(i < room.injectionFrameData.Count - 1);
		}
		if (RoomSequence.Count < 1)
		{
			Debug.LogError("Failed to initialize Ancient Pistol1");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
