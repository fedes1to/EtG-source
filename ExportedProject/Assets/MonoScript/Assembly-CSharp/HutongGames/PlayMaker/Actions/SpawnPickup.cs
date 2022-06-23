using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Spawns a pickup (gun or item) in the world or gives it directly to the player.")]
	[ActionCategory(".NPCs")]
	public class SpawnPickup : FsmStateAction
	{
		public enum Mode
		{
			SpecifyPickup,
			LootTable,
			DaveStyleFloorReward
		}

		public enum SpawnLocation
		{
			GiveToPlayer,
			AtPlayer,
			AtTalkDoer,
			OffsetFromPlayer,
			OffsetFromTalkDoer,
			RoomSpawnPoint,
			GiveToBothPlayers
		}

		public Mode mode;

		[Tooltip("Item to spawn.")]
		public FsmInt pickupId;

		[Tooltip("Loot table to choose an item from.")]
		public GenericLootTable lootTable;

		[Tooltip("Where to spawn the item at.")]
		public SpawnLocation spawnLocation;

		[Tooltip("Offset from the TalkDoer to spawn the item at.")]
		public Vector2 spawnOffset;

		public override void Reset()
		{
			mode = Mode.SpecifyPickup;
			pickupId = -1;
			lootTable = null;
			spawnLocation = SpawnLocation.GiveToPlayer;
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
			PlayerController playerController = ((!component.TalkingPlayer) ? GameManager.Instance.PrimaryPlayer : component.TalkingPlayer);
			GameObject item = null;
			bool flag = false;
			if (mode == Mode.SpecifyPickup)
			{
				item = PickupObjectDatabase.GetById(pickupId.Value).gameObject;
			}
			else if (mode == Mode.LootTable)
			{
				item = lootTable.SelectByWeightWithoutDuplicatesFullPrereqs(null);
				flag = true;
			}
			else if (mode == Mode.DaveStyleFloorReward)
			{
				item = GameManager.Instance.RewardManager.GetRewardObjectDaveStyle(playerController);
				flag = true;
			}
			else
			{
				Debug.LogError("Tried to give an item to the player but no valid mode was selected.");
			}
			if (flag && GameStatsManager.Instance.IsRainbowRun)
			{
				Vector2 position = GameManager.Instance.PrimaryPlayer.CenterPosition + new Vector2(-0.5f, -0.5f);
				LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteOtherSource, position, playerController.CurrentRoom, true);
			}
			else if (spawnLocation == SpawnLocation.GiveToPlayer)
			{
				LootEngine.TryGivePrefabToPlayer(item, playerController);
			}
			else if (spawnLocation == SpawnLocation.GiveToBothPlayers)
			{
				LootEngine.TryGivePrefabToPlayer(item, GameManager.Instance.PrimaryPlayer);
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					LootEngine.TryGivePrefabToPlayer(item, GameManager.Instance.SecondaryPlayer);
				}
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
				else if (spawnLocation == SpawnLocation.RoomSpawnPoint)
				{
					vector = playerController.CurrentRoom.GetBestRewardLocation(IntVector2.One, RoomHandler.RewardLocationStyle.Original, false).ToVector2();
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
				LootEngine.SpawnItem(item, vector, Vector2.zero, 0f);
				LootEngine.DoDefaultItemPoof(vector);
			}
			Finish();
		}
	}
}
