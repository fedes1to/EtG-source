using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class TeleporterPrototypeItem : PlayerItem
{
	public float ChanceToGoToSpecialRoom = 0.1f;

	public float ChanceToGoToEyeballRoom = 0.01f;

	public float ChanceToGoToNextFloor = 0.01f;

	public float ChanceToGoToSecretFloor = 0.01f;

	public float ChanceToGoToBossFoyer = 0.01f;

	[Header("Synergies")]
	public GameObject TelefragVFXPrefab;

	private float LastCooldownModifier = 1f;

	public override bool CanBeUsed(PlayerController user)
	{
		if (!user || user.IsInMinecart)
		{
			return false;
		}
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.RATGEON)
		{
			return false;
		}
		if (user.CurrentRoom != null)
		{
			if (user.CurrentRoom.CompletelyPreventLeaving)
			{
				return false;
			}
			if (GameManager.Instance.Dungeon != null && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON && user.CurrentRoom != null && user.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				return false;
			}
			if (user.CurrentRoom.IsSealed && Mathf.Abs(user.RealtimeEnteredCurrentRoom - Time.realtimeSinceStartup) < 0.5f)
			{
				return false;
			}
			if (!user.CurrentRoom.CanBeEscaped())
			{
				return false;
			}
		}
		return base.CanBeUsed(user);
	}

	protected void TelefragRandomEnemy(RoomHandler room)
	{
		AIActor randomActiveEnemy = room.GetRandomActiveEnemy();
		if (randomActiveEnemy.IsNormalEnemy && (bool)randomActiveEnemy.healthHaver && !randomActiveEnemy.healthHaver.IsBoss)
		{
			Vector2 vector = ((!randomActiveEnemy.specRigidbody) ? randomActiveEnemy.sprite.WorldBottomLeft : randomActiveEnemy.specRigidbody.UnitBottomLeft);
			Vector2 vector2 = ((!randomActiveEnemy.specRigidbody) ? randomActiveEnemy.sprite.WorldTopRight : randomActiveEnemy.specRigidbody.UnitTopRight);
			Object.Instantiate(TelefragVFXPrefab, randomActiveEnemy.CenterPosition.ToVector3ZisY(), Quaternion.identity);
			randomActiveEnemy.healthHaver.ApplyDamage(100000f, Vector2.zero, "Telefrag", CoreDamageTypes.None, DamageCategory.Normal, true);
		}
	}

	protected void TelefragRoom(RoomHandler room)
	{
		Pixelator.Instance.FadeToColor(0.25f, Color.white, true);
		List<AIActor> activeEnemies = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (activeEnemies[i].IsNormalEnemy && (bool)activeEnemies[i].healthHaver && !activeEnemies[i].healthHaver.IsBoss)
			{
				Vector2 vector = ((!activeEnemies[i].specRigidbody) ? activeEnemies[i].sprite.WorldBottomLeft : activeEnemies[i].specRigidbody.UnitBottomLeft);
				Vector2 vector2 = ((!activeEnemies[i].specRigidbody) ? activeEnemies[i].sprite.WorldTopRight : activeEnemies[i].specRigidbody.UnitTopRight);
				Object.Instantiate(TelefragVFXPrefab, activeEnemies[i].CenterPosition.ToVector3ZisY(), Quaternion.identity);
				activeEnemies[i].healthHaver.ApplyDamage(100000f, Vector2.zero, "Telefrag", CoreDamageTypes.None, DamageCategory.Normal, true);
			}
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		if (user.CurrentRoom != null && user.CurrentRoom.CompletelyPreventLeaving)
		{
			return;
		}
		AkSoundEngine.PostEvent("Play_OBJ_teleport_depart_01", base.gameObject);
		RoomHandler roomHandler = null;
		GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
		float value = Random.value;
		bool flag = user.HasActiveBonusSynergy(CustomSynergyType.DOUBLE_TELEPORTERS);
		LastCooldownModifier = 1f;
		if (value < ChanceToGoToNextFloor)
		{
			if (tilesetId != GlobalDungeonData.ValidTilesets.FORGEGEON && tilesetId != GlobalDungeonData.ValidTilesets.HELLGEON)
			{
				PlayTeleporterEffect(user);
				Pixelator.Instance.FadeToBlack(0.5f);
				GameManager.Instance.DelayedLoadNextLevel(0.5f);
			}
		}
		else if (value < ChanceToGoToNextFloor + ChanceToGoToEyeballRoom && !user.IsInCombat)
		{
			PlayTeleporterEffect(user);
			StartCoroutine(HandleCreepyEyeWarp(user));
			AkSoundEngine.PostEvent("Play_OBJ_teleport_depart_01", base.gameObject);
			if (flag)
			{
				LastCooldownModifier = 0.5f;
			}
		}
		else if (value < ChanceToGoToNextFloor + ChanceToGoToEyeballRoom + ChanceToGoToSpecialRoom)
		{
			List<int> list = Enumerable.Range(0, GameManager.Instance.Dungeon.data.rooms.Count).ToList().Shuffle();
			for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
			{
				RoomHandler roomHandler2 = GameManager.Instance.Dungeon.data.rooms[list[i]];
				if (roomHandler2.IsSecretRoom)
				{
					roomHandler = roomHandler2;
				}
			}
			if (roomHandler == null)
			{
				for (int j = 0; j < GameManager.Instance.Dungeon.data.rooms.Count; j++)
				{
					RoomHandler roomHandler3 = GameManager.Instance.Dungeon.data.rooms[list[j]];
					if (roomHandler3.IsShop || roomHandler3.IsSecretRoom || roomHandler3.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD)
					{
						if (roomHandler3.IsSecretRoom)
						{
							roomHandler3.secretRoomManager.HandleDoorBrokenOpen(roomHandler3.secretRoomManager.doorObjects[0]);
						}
						roomHandler = roomHandler3;
						break;
					}
				}
			}
			if (flag)
			{
				LastCooldownModifier = 0.5f;
			}
		}
		else if (value < ChanceToGoToNextFloor + ChanceToGoToEyeballRoom + ChanceToGoToSpecialRoom + ChanceToGoToBossFoyer)
		{
			RoomHandler roomHandler4 = ChestTeleporterItem.FindBossFoyer();
			if (roomHandler4 != null)
			{
				roomHandler = roomHandler4;
			}
			if (flag)
			{
				LastCooldownModifier = 0.5f;
			}
		}
		else if (value < ChanceToGoToNextFloor + ChanceToGoToEyeballRoom + ChanceToGoToSpecialRoom + ChanceToGoToSecretFloor && (tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON || tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON))
		{
			switch (tilesetId)
			{
			case GlobalDungeonData.ValidTilesets.CASTLEGEON:
				PlayTeleporterEffect(user);
				Pixelator.Instance.FadeToBlack(0.5f);
				GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.CATHEDRALGEON);
				GameManager.Instance.DelayedLoadCustomLevel(0.5f, "tt_sewer");
				break;
			case GlobalDungeonData.ValidTilesets.GUNGEON:
				PlayTeleporterEffect(user);
				Pixelator.Instance.FadeToBlack(0.5f);
				GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.CATHEDRALGEON);
				GameManager.Instance.DelayedLoadCustomLevel(0.5f, "tt_cathedral");
				break;
			}
		}
		else
		{
			List<int> list2 = Enumerable.Range(0, GameManager.Instance.Dungeon.data.rooms.Count).ToList().Shuffle();
			for (int k = 0; k < GameManager.Instance.Dungeon.data.rooms.Count; k++)
			{
				RoomHandler roomHandler5 = GameManager.Instance.Dungeon.data.rooms[list2[k]];
				if ((roomHandler5.area.PrototypeRoomNormalSubcategory != PrototypeDungeonRoom.RoomNormalSubCategory.TRAP && roomHandler5.IsStandardRoom && roomHandler5.EverHadEnemies) || roomHandler5.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD || roomHandler5.IsShop)
				{
					roomHandler = roomHandler5;
					break;
				}
			}
		}
		if (roomHandler != null)
		{
			user.EscapeRoom(PlayerController.EscapeSealedRoomStyle.TELEPORTER, true, roomHandler);
			if (roomHandler.IsSecretRoom && roomHandler.secretRoomManager != null && roomHandler.secretRoomManager.doorObjects.Count > 0)
			{
				roomHandler.secretRoomManager.doorObjects[0].BreakOpen();
			}
			bool allEnemies = flag;
			if ((roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.NORMAL && roomHandler.area.PrototypeRoomNormalSubcategory == PrototypeDungeonRoom.RoomNormalSubCategory.COMBAT) || roomHandler.area.IsProceduralRoom)
			{
				user.StartCoroutine(HandleTelefragDelay(roomHandler, allEnemies));
			}
		}
	}

	protected override void AfterCooldownApplied(PlayerController user)
	{
		if (LastCooldownModifier < 1f)
		{
			base.AfterCooldownApplied(user);
			DidDamage(user, base.CurrentDamageCooldown * (1f - LastCooldownModifier));
		}
	}

	private IEnumerator HandleTelefragDelay(RoomHandler targetRoom, bool allEnemies)
	{
		yield return new WaitForSeconds(1.5f);
		if (targetRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All))
		{
			if (allEnemies)
			{
				TelefragRoom(targetRoom);
			}
			else
			{
				TelefragRandomEnemy(targetRoom);
			}
		}
	}

	private IEnumerator HandleCreepyEyeWarp(PlayerController interactor)
	{
		RoomHandler creepyRoom = GameManager.Instance.Dungeon.AddRuntimeRoom(new IntVector2(24, 24), (GameObject)BraveResources.Load("Global Prefabs/CreepyEye_Room"));
		GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_YELLOW_CHAMBER, true);
		yield return new WaitForSeconds(0.25f);
		Pathfinder.Instance.InitializeRegion(GameManager.Instance.Dungeon.data, creepyRoom.area.basePosition, creepyRoom.area.dimensions);
		interactor.WarpToPoint((creepyRoom.area.basePosition + new IntVector2(12, 4)).ToVector2());
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(interactor).ReuniteWithOtherPlayer(interactor);
		}
	}

	private void PlayTeleporterEffect(PlayerController p)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (!GameManager.Instance.AllPlayers[i].IsGhost)
			{
				GameManager.Instance.AllPlayers[i].healthHaver.TriggerInvulnerabilityPeriod(1f);
				GameManager.Instance.AllPlayers[i].knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
			}
		}
		GameObject gameObject = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Teleport_Beam");
		if (gameObject != null)
		{
			GameObject gameObject2 = Object.Instantiate(gameObject);
			gameObject2.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(p.specRigidbody.UnitBottomCenter + new Vector2(0f, -0.5f), tk2dBaseSprite.Anchor.LowerCenter);
			gameObject2.transform.position = gameObject2.transform.position.Quantize(0.0625f);
			gameObject2.GetComponent<tk2dBaseSprite>().UpdateZDepth();
		}
	}
}
