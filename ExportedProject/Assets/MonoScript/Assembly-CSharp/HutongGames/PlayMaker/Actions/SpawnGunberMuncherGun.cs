using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Spawns a pickup (gun or item) in the world or gives it directly to the player.")]
	[ActionCategory(".NPCs")]
	public class SpawnGunberMuncherGun : FsmStateAction
	{
		public enum SpawnLocation
		{
			GiveToPlayer,
			AtPlayer,
			AtTalkDoer,
			OffsetFromPlayer,
			OffsetFromTalkDoer
		}

		[Tooltip("Where to spawn the item at.")]
		public SpawnLocation spawnLocation;

		[Tooltip("Offset from the TalkDoer to spawn the item at.")]
		public Vector2 spawnOffset;

		[NonSerialized]
		public Gun firstRecycledGun;

		[NonSerialized]
		public Gun secondRecycledGun;

		public override void Reset()
		{
			spawnLocation = SpawnLocation.GiveToPlayer;
			spawnOffset = Vector2.zero;
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			GameObject gameObject = null;
			PlayerController playerController = ((!component.TalkingPlayer) ? GameManager.Instance.PrimaryPlayer : component.TalkingPlayer);
			if (spawnLocation == SpawnLocation.GiveToPlayer)
			{
				LootEngine.TryGivePrefabToPlayer(gameObject, playerController);
			}
			else
			{
				Vector2 vector;
				if (spawnLocation == SpawnLocation.AtPlayer || spawnLocation == SpawnLocation.OffsetFromPlayer)
				{
					vector = playerController.specRigidbody.UnitCenter;
				}
				else if (spawnLocation == SpawnLocation.AtTalkDoer || spawnLocation == SpawnLocation.OffsetFromTalkDoer)
				{
					vector = ((!(component.specRigidbody != null)) ? component.sprite.WorldCenter : component.specRigidbody.UnitCenter);
				}
				else
				{
					Debug.LogError("Tried to give an item to the player but no valid spawn location was selected.");
					vector = GameManager.Instance.PrimaryPlayer.CenterPosition;
				}
				if (spawnLocation == SpawnLocation.OffsetFromPlayer || spawnLocation == SpawnLocation.OffsetFromTalkDoer)
				{
					vector += spawnOffset;
				}
				LootEngine.SpewLoot(gameObject, vector);
			}
			Finish();
		}
	}
}
