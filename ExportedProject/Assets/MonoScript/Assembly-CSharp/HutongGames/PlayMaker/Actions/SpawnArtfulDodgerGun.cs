using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Spawns the Artful Dodger gun in the world or gives it directly to the player.")]
	[ActionCategory(".NPCs")]
	public class SpawnArtfulDodgerGun : FsmStateAction
	{
		public enum Mode
		{
			SpecifyPickup,
			LootTable
		}

		public enum SpawnLocation
		{
			GiveToPlayer,
			AtPlayer,
			AtTalkDoer,
			OffsetFromPlayer,
			OffsetFromTalkDoer
		}

		public Mode mode;

		[Tooltip("Item to spawn.")]
		public FsmInt pickupId;

		public FsmInt numberOfBouncesAllowed = 3;

		public FsmInt numberOfShotsAllowed = 3;

		[Tooltip("Loot table to choose an item from.")]
		public GenericLootTable lootTable;

		[Tooltip("Offset from the TalkDoer to spawn the item at.")]
		public Vector2 spawnOffset;

		public override void Reset()
		{
			mode = Mode.SpecifyPickup;
			pickupId = -1;
			numberOfBouncesAllowed = 3;
			numberOfShotsAllowed = 3;
			lootTable = null;
			spawnOffset = Vector2.zero;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (mode == Mode.SpecifyPickup && PickupObjectDatabase.GetById(pickupId.Value) == null)
			{
				text += "Invalid item ID.\n";
			}
			if (mode == Mode.LootTable && !lootTable)
			{
				text += "Invalid loot table.\n";
			}
			return text;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			GameObject item = null;
			if (mode == Mode.SpecifyPickup)
			{
				item = PickupObjectDatabase.GetById(pickupId.Value).gameObject;
			}
			else if (mode == Mode.LootTable)
			{
				item = lootTable.SelectByWeightWithoutDuplicatesFullPrereqs(null);
			}
			else
			{
				Debug.LogError("Tried to give an item to the player but no valid mode was selected.");
			}
			PlayerController playerController = ((!component.TalkingPlayer) ? GameManager.Instance.PrimaryPlayer : component.TalkingPlayer);
			Gun gun = null;
			if ((bool)playerController.CurrentGun)
			{
				MimicGunController component2 = playerController.CurrentGun.GetComponent<MimicGunController>();
				if ((bool)component2)
				{
					component2.ForceClearMimic(true);
				}
			}
			if (LootEngine.TryGivePrefabToPlayer(item, playerController, true))
			{
				gun = playerController.GetComponentInChildren<ArtfulDodgerGunController>().GetComponent<Gun>();
			}
			List<ArtfulDodgerRoomController> componentsAbsoluteInRoom = component.GetAbsoluteParentRoom().GetComponentsAbsoluteInRoom<ArtfulDodgerRoomController>();
			ArtfulDodgerRoomController artfulDodgerRoomController = ((componentsAbsoluteInRoom == null || componentsAbsoluteInRoom.Count <= 0) ? null : componentsAbsoluteInRoom[0]);
			gun.CurrentAmmo = ((!(artfulDodgerRoomController == null)) ? Mathf.RoundToInt(artfulDodgerRoomController.NumberShots) : numberOfShotsAllowed.Value);
			PostShootProjectileModifier postShootProjectileModifier = gun.gameObject.AddComponent<PostShootProjectileModifier>();
			postShootProjectileModifier.NumberBouncesToSet = ((!(artfulDodgerRoomController == null)) ? Mathf.RoundToInt(artfulDodgerRoomController.NumberBounces) : numberOfBouncesAllowed.Value);
			artfulDodgerRoomController.Activate(base.Fsm);
			Finish();
		}
	}
}
