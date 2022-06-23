using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class PaydayDrillItem : PlayerItem, IPaydayItem
{
	public GameObject DrillVFXPrefab;

	public VFXPool VFXDustPoof;

	public VFXPool DisappearDrillPoof;

	public PrototypeDungeonRoom GenericFallbackCombatRoom;

	[Header("Timing")]
	public float DelayPreExpansion = 2.5f;

	public float DelayPostExpansionPreEnemies = 2f;

	[Header("Waves")]
	public DrillWaveDefinition[] D_Quality_Waves;

	public DrillWaveDefinition[] C_Quality_Waves;

	public DrillWaveDefinition[] B_Quality_Waves;

	public DrillWaveDefinition[] A_Quality_Waves;

	public DrillWaveDefinition[] S_Quality_Waves;

	private bool m_inEffect;

	[NonSerialized]
	public bool HasSetOrder;

	[NonSerialized]
	public string ID01;

	[NonSerialized]
	public string ID02;

	[NonSerialized]
	public string ID03;

	private Vector3 m_baseChestOffset = new Vector3(0.5f, 0.25f, 0f);

	private Vector3 m_largeChestOffset = new Vector3(0.4375f, 0.0625f, 0f);

	private string[] c_rewardRoomObjects = new string[2] { "Gungeon_Treasure_Dais(Clone)", "GodRay_Placeable(Clone)" };

	public void StoreData(string id1, string id2, string id3)
	{
		ID01 = id1;
		ID02 = id2;
		ID03 = id3;
		HasSetOrder = true;
	}

	public bool HasCachedData()
	{
		return HasSetOrder;
	}

	public string GetID(int placement)
	{
		switch (placement)
		{
		case 0:
			return ID01;
		case 1:
			return ID02;
		default:
			return ID03;
		}
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add(HasSetOrder);
		data.Add(ID01);
		data.Add(ID02);
		data.Add(ID03);
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (data.Count == 4)
		{
			HasSetOrder = (bool)data[0];
			ID01 = (string)data[1];
			ID02 = (string)data[2];
			ID03 = (string)data[3];
		}
	}

	public override bool CanBeUsed(PlayerController user)
	{
		if (!user || user.CurrentRoom == null)
		{
			return false;
		}
		if (user.CurrentRoom.CompletelyPreventLeaving)
		{
			return false;
		}
		if (user.CurrentRoom.area.PrototypeLostWoodsRoom)
		{
			return false;
		}
		IPlayerInteractable nearestInteractable = user.CurrentRoom.GetNearestInteractable(user.CenterPosition, 1f, user);
		if (nearestInteractable is InteractableLock || nearestInteractable is Chest || nearestInteractable is DungeonDoorController)
		{
			if (nearestInteractable is InteractableLock)
			{
				InteractableLock interactableLock = nearestInteractable as InteractableLock;
				if ((bool)interactableLock && !interactableLock.IsBusted && interactableLock.transform.position.GetAbsoluteRoom() == user.CurrentRoom && interactableLock.IsLocked && !interactableLock.HasBeenPicked && interactableLock.lockMode == InteractableLock.InteractableLockMode.NORMAL)
				{
					return base.CanBeUsed(user);
				}
			}
			else if (nearestInteractable is DungeonDoorController)
			{
				DungeonDoorController dungeonDoorController = nearestInteractable as DungeonDoorController;
				if (dungeonDoorController != null && dungeonDoorController.Mode == DungeonDoorController.DungeonDoorMode.COMPLEX && dungeonDoorController.isLocked && !dungeonDoorController.lockIsBusted)
				{
					return base.CanBeUsed(user);
				}
			}
			else if (nearestInteractable is Chest)
			{
				Chest chest = nearestInteractable as Chest;
				if (!chest)
				{
					return false;
				}
				if (chest.GetAbsoluteParentRoom() != user.CurrentRoom)
				{
					return false;
				}
				if (!chest.IsLocked)
				{
					return false;
				}
				if (chest.IsLockBroken)
				{
					return false;
				}
				return base.CanBeUsed(user);
			}
		}
		return false;
	}

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		AkSoundEngine.PostEvent("Play_OBJ_paydaydrill_start_01", GameManager.Instance.gameObject);
		AkSoundEngine.PostEvent("Play_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
		IPlayerInteractable nearestInteractable = user.CurrentRoom.GetNearestInteractable(user.CenterPosition, 1f, user);
		if (nearestInteractable is InteractableLock || nearestInteractable is Chest || nearestInteractable is DungeonDoorController)
		{
			if (nearestInteractable is InteractableLock)
			{
				InteractableLock interactableLock = nearestInteractable as InteractableLock;
				if (interactableLock.lockMode == InteractableLock.InteractableLockMode.NORMAL)
				{
					interactableLock.ForceUnlock();
					AkSoundEngine.PostEvent("m_OBJ_lock_pick_01", GameManager.Instance.gameObject);
				}
				AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
			}
			else if (nearestInteractable is DungeonDoorController)
			{
				DungeonDoorController dungeonDoorController = nearestInteractable as DungeonDoorController;
				if (dungeonDoorController != null && dungeonDoorController.Mode == DungeonDoorController.DungeonDoorMode.COMPLEX && dungeonDoorController.isLocked)
				{
					dungeonDoorController.Unlock();
					AkSoundEngine.PostEvent("m_OBJ_lock_pick_01", GameManager.Instance.gameObject);
				}
				AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
			}
			else
			{
				if (!(nearestInteractable is Chest))
				{
					return;
				}
				Chest chest = nearestInteractable as Chest;
				if (!chest.IsLocked)
				{
					return;
				}
				if (chest.IsLockBroken)
				{
					AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
					return;
				}
				if (chest.IsMimic && (bool)chest.majorBreakable)
				{
					chest.majorBreakable.ApplyDamage(1000f, Vector2.zero, false, false, true);
					AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
					return;
				}
				chest.ForceKillFuse();
				chest.PreventFuse = true;
				RoomHandler absoluteRoom = chest.transform.position.GetAbsoluteRoom();
				m_inEffect = true;
				if (absoluteRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD)
				{
					GameManager.Instance.Dungeon.StartCoroutine(HandleSeamlessTransitionToCombatRoom(absoluteRoom, chest));
				}
				else
				{
					GameManager.Instance.Dungeon.StartCoroutine(HandleTransitionToFallbackCombatRoom(absoluteRoom, chest));
				}
			}
		}
		else
		{
			AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
		}
	}

	protected IEnumerator HandleCombatWaves(Dungeon d, RoomHandler newRoom, Chest sourceChest)
	{
		DrillWaveDefinition[] wavesToUse = D_Quality_Waves;
		switch (GameManager.Instance.RewardManager.GetQualityFromChest(sourceChest))
		{
		case ItemQuality.C:
			wavesToUse = C_Quality_Waves;
			break;
		case ItemQuality.B:
			wavesToUse = B_Quality_Waves;
			break;
		case ItemQuality.A:
			wavesToUse = A_Quality_Waves;
			break;
		case ItemQuality.S:
			wavesToUse = S_Quality_Waves;
			break;
		}
		for (int waveIndex = 0; waveIndex < wavesToUse.Length; waveIndex++)
		{
			DrillWaveDefinition currentWave = wavesToUse[waveIndex];
			int numEnemiesToSpawn = UnityEngine.Random.Range(currentWave.MinEnemies, currentWave.MaxEnemies + 1);
			for (int i = 0; i < numEnemiesToSpawn; i++)
			{
				newRoom.AddSpecificEnemyToRoomProcedurally(d.GetWeightedProceduralEnemy().enemyGuid, true);
			}
			yield return new WaitForSeconds(3f);
			while (newRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) > 0)
			{
				yield return new WaitForSeconds(1f);
			}
			if (newRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) <= 0)
			{
				continue;
			}
			List<AIActor> activeEnemies = newRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			for (int j = 0; j < activeEnemies.Count; j++)
			{
				if (activeEnemies[j].IsNormalEnemy)
				{
					activeEnemies[j].EraseFromExistence();
				}
			}
		}
	}

	protected IEnumerator HandleTransitionToFallbackCombatRoom(RoomHandler sourceRoom, Chest sourceChest)
	{
		Dungeon d = GameManager.Instance.Dungeon;
		sourceChest.majorBreakable.TemporarilyInvulnerable = true;
		sourceRoom.DeregisterInteractable(sourceChest);
		RoomHandler newRoom = d.AddRuntimeRoom(GenericFallbackCombatRoom);
		newRoom.CompletelyPreventLeaving = true;
		Vector3 oldChestPosition = sourceChest.transform.position;
		sourceChest.transform.position = newRoom.Epicenter.ToVector3();
		if (sourceChest.transform.parent == sourceRoom.hierarchyParent)
		{
			sourceChest.transform.parent = newRoom.hierarchyParent;
		}
		SpeculativeRigidbody component = sourceChest.GetComponent<SpeculativeRigidbody>();
		if ((bool)component)
		{
			component.Reinitialize();
			PathBlocker.BlockRigidbody(component, false);
		}
		tk2dBaseSprite component2 = sourceChest.GetComponent<tk2dBaseSprite>();
		if ((bool)component2)
		{
			component2.UpdateZDepth();
		}
		Vector3 chestOffset = m_baseChestOffset;
		if (sourceChest.name.Contains("_Red") || sourceChest.name.Contains("_Black"))
		{
			chestOffset += m_largeChestOffset;
		}
		GameObject spawnedVFX = SpawnManager.SpawnVFX(DrillVFXPrefab, sourceChest.transform.position + chestOffset, Quaternion.identity);
		tk2dBaseSprite spawnedSprite = spawnedVFX.GetComponent<tk2dBaseSprite>();
		spawnedSprite.HeightOffGround = 1f;
		spawnedSprite.UpdateZDepth();
		Vector2 oldPlayerPosition = GameManager.Instance.BestActivePlayer.transform.position.XY();
		Vector2 newPlayerPosition = newRoom.Epicenter.ToVector2() + new Vector2(0f, -3f);
		Pixelator.Instance.FadeToColor(0.25f, Color.white, true, 0.125f);
		Pathfinder.Instance.InitializeRegion(d.data, newRoom.area.basePosition, newRoom.area.dimensions);
		GameManager.Instance.BestActivePlayer.WarpToPoint(newPlayerPosition);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(GameManager.Instance.BestActivePlayer).ReuniteWithOtherPlayer(GameManager.Instance.BestActivePlayer);
		}
		yield return null;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].WarpFollowersToPlayer();
			GameManager.Instance.AllPlayers[i].WarpCompanionsToPlayer();
		}
		yield return new WaitForSeconds(DelayPostExpansionPreEnemies);
		yield return StartCoroutine(HandleCombatWaves(d, newRoom, sourceChest));
		DisappearDrillPoof.SpawnAtPosition(spawnedSprite.WorldBottomLeft + new Vector2(-0.0625f, 0.25f), 0f, null, null, null, 3f);
		UnityEngine.Object.Destroy(spawnedVFX.gameObject);
		AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
		AkSoundEngine.PostEvent("Play_OBJ_item_spawn_01", GameManager.Instance.gameObject);
		sourceChest.ForceUnlock();
		bool goodToGo = false;
		while (!goodToGo)
		{
			goodToGo = true;
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				float num = Vector2.Distance(sourceChest.specRigidbody.UnitCenter, GameManager.Instance.AllPlayers[j].CenterPosition);
				if (num > 3f)
				{
					goodToGo = false;
				}
			}
			yield return null;
		}
		Pixelator.Instance.FadeToColor(0.25f, Color.white, true, 0.125f);
		GameManager.Instance.BestActivePlayer.WarpToPoint(oldPlayerPosition);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(GameManager.Instance.BestActivePlayer).ReuniteWithOtherPlayer(GameManager.Instance.BestActivePlayer);
		}
		sourceChest.transform.position = oldChestPosition;
		if (sourceChest.transform.parent == newRoom.hierarchyParent)
		{
			sourceChest.transform.parent = sourceRoom.hierarchyParent;
		}
		SpeculativeRigidbody component3 = sourceChest.GetComponent<SpeculativeRigidbody>();
		if ((bool)component3)
		{
			component3.Reinitialize();
		}
		tk2dBaseSprite component4 = sourceChest.GetComponent<tk2dBaseSprite>();
		if ((bool)component4)
		{
			component4.UpdateZDepth();
		}
		sourceRoom.RegisterInteractable(sourceChest);
		m_inEffect = false;
	}

	protected IEnumerator HandleSeamlessTransitionToCombatRoom(RoomHandler sourceRoom, Chest sourceChest)
	{
		Dungeon d = GameManager.Instance.Dungeon;
		sourceChest.majorBreakable.TemporarilyInvulnerable = true;
		sourceRoom.DeregisterInteractable(sourceChest);
		int tmapExpansion = 13;
		RoomHandler newRoom = d.RuntimeDuplicateChunk(sourceRoom.area.basePosition, sourceRoom.area.dimensions, tmapExpansion, sourceRoom, true);
		newRoom.CompletelyPreventLeaving = true;
		List<Transform> movedObjects = new List<Transform>();
		for (int i = 0; i < c_rewardRoomObjects.Length; i++)
		{
			Transform transform = sourceRoom.hierarchyParent.Find(c_rewardRoomObjects[i]);
			if ((bool)transform)
			{
				movedObjects.Add(transform);
				MoveObjectBetweenRooms(transform, sourceRoom, newRoom);
			}
		}
		MoveObjectBetweenRooms(sourceChest.transform, sourceRoom, newRoom);
		if ((bool)sourceChest.specRigidbody)
		{
			PathBlocker.BlockRigidbody(sourceChest.specRigidbody, false);
		}
		Vector3 chestOffset = m_baseChestOffset;
		if (sourceChest.name.Contains("_Red") || sourceChest.name.Contains("_Black"))
		{
			chestOffset += m_largeChestOffset;
		}
		GameObject spawnedVFX = SpawnManager.SpawnVFX(DrillVFXPrefab, sourceChest.transform.position + chestOffset, Quaternion.identity);
		tk2dBaseSprite spawnedSprite = spawnedVFX.GetComponent<tk2dBaseSprite>();
		spawnedSprite.HeightOffGround = 1f;
		spawnedSprite.UpdateZDepth();
		Vector2 oldPlayerPosition = GameManager.Instance.BestActivePlayer.transform.position.XY();
		Vector2 playerOffset = oldPlayerPosition - sourceRoom.area.basePosition.ToVector2();
		Vector2 newPlayerPosition = newRoom.area.basePosition.ToVector2() + playerOffset;
		Pixelator.Instance.FadeToColor(0.25f, Color.white, true, 0.125f);
		Pathfinder.Instance.InitializeRegion(d.data, newRoom.area.basePosition, newRoom.area.dimensions);
		GameManager.Instance.BestActivePlayer.WarpToPoint(newPlayerPosition);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(GameManager.Instance.BestActivePlayer).ReuniteWithOtherPlayer(GameManager.Instance.BestActivePlayer);
		}
		yield return null;
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			GameManager.Instance.AllPlayers[j].WarpFollowersToPlayer();
			GameManager.Instance.AllPlayers[j].WarpCompanionsToPlayer();
		}
		yield return d.StartCoroutine(HandleCombatRoomExpansion(sourceRoom, newRoom, sourceChest));
		DisappearDrillPoof.SpawnAtPosition(spawnedSprite.WorldBottomLeft + new Vector2(-0.0625f, 0.25f), 0f, null, null, null, 3f);
		UnityEngine.Object.Destroy(spawnedVFX.gameObject);
		sourceChest.ForceUnlock();
		AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
		AkSoundEngine.PostEvent("Play_OBJ_item_spawn_01", GameManager.Instance.gameObject);
		bool goodToGo = false;
		while (!goodToGo)
		{
			goodToGo = true;
			for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
			{
				float num = Vector2.Distance(sourceChest.specRigidbody.UnitCenter, GameManager.Instance.AllPlayers[k].CenterPosition);
				if (num > 3f)
				{
					goodToGo = false;
				}
			}
			yield return null;
		}
		GameManager.Instance.MainCameraController.SetManualControl(true);
		GameManager.Instance.MainCameraController.OverridePosition = GameManager.Instance.BestActivePlayer.CenterPosition;
		for (int l = 0; l < GameManager.Instance.AllPlayers.Length; l++)
		{
			GameManager.Instance.AllPlayers[l].SetInputOverride("shrinkage");
		}
		yield return d.StartCoroutine(HandleCombatRoomShrinking(newRoom));
		for (int m = 0; m < GameManager.Instance.AllPlayers.Length; m++)
		{
			GameManager.Instance.AllPlayers[m].ClearInputOverride("shrinkage");
		}
		Pixelator.Instance.FadeToColor(0.25f, Color.white, true, 0.125f);
		AkSoundEngine.PostEvent("Play_OBJ_paydaydrill_end_01", GameManager.Instance.gameObject);
		GameManager.Instance.MainCameraController.SetManualControl(false, false);
		GameManager.Instance.BestActivePlayer.WarpToPoint(oldPlayerPosition);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(GameManager.Instance.BestActivePlayer).ReuniteWithOtherPlayer(GameManager.Instance.BestActivePlayer);
		}
		MoveObjectBetweenRooms(sourceChest.transform, newRoom, sourceRoom);
		for (int n = 0; n < movedObjects.Count; n++)
		{
			MoveObjectBetweenRooms(movedObjects[n], newRoom, sourceRoom);
		}
		sourceRoom.RegisterInteractable(sourceChest);
		m_inEffect = false;
	}

	private void MoveObjectBetweenRooms(Transform foundObject, RoomHandler fromRoom, RoomHandler toRoom)
	{
		Vector2 vector = foundObject.position.XY() - fromRoom.area.basePosition.ToVector2();
		Vector2 vector2 = toRoom.area.basePosition.ToVector2() + vector;
		foundObject.transform.position = vector2;
		if (foundObject.parent == fromRoom.hierarchyParent)
		{
			foundObject.parent = toRoom.hierarchyParent;
		}
		SpeculativeRigidbody component = foundObject.GetComponent<SpeculativeRigidbody>();
		if ((bool)component)
		{
			component.Reinitialize();
		}
		tk2dBaseSprite component2 = foundObject.GetComponent<tk2dBaseSprite>();
		if ((bool)component2)
		{
			component2.UpdateZDepth();
		}
	}

	private IEnumerator HandleCombatRoomShrinking(RoomHandler targetRoom)
	{
		float elapsed = 5.5f;
		int numExpansionsDone = 6;
		while (elapsed > 0f)
		{
			elapsed -= BraveTime.DeltaTime * 9f;
			while (elapsed < (float)numExpansionsDone && numExpansionsDone > 0)
			{
				numExpansionsDone--;
				ShrinkRoom(targetRoom);
			}
			yield return null;
		}
	}

	private IEnumerator HandleCombatRoomExpansion(RoomHandler sourceRoom, RoomHandler targetRoom, Chest sourceChest)
	{
		yield return new WaitForSeconds(DelayPreExpansion);
		float duration = 5.5f;
		float elapsed = 0f;
		int numExpansionsDone = 0;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime * 9f;
			while (elapsed > (float)numExpansionsDone)
			{
				numExpansionsDone++;
				ExpandRoom(targetRoom);
				AkSoundEngine.PostEvent("Play_OBJ_rock_break_01", GameManager.Instance.gameObject);
			}
			yield return null;
		}
		Dungeon d = GameManager.Instance.Dungeon;
		Pathfinder.Instance.InitializeRegion(d.data, targetRoom.area.basePosition + new IntVector2(-5, -5), targetRoom.area.dimensions + new IntVector2(10, 10));
		yield return new WaitForSeconds(DelayPostExpansionPreEnemies);
		yield return HandleCombatWaves(d, targetRoom, sourceChest);
	}

	private void ShrinkRoom(RoomHandler r)
	{
		Dungeon dungeon = GameManager.Instance.Dungeon;
		AkSoundEngine.PostEvent("Play_OBJ_stone_crumble_01", GameManager.Instance.gameObject);
		tk2dTileMap tk2dTileMap2 = null;
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		for (int i = -5; i < r.area.dimensions.x + 5; i++)
		{
			for (int j = -5; j < r.area.dimensions.y + 5; j++)
			{
				IntVector2 intVector = r.area.basePosition + new IntVector2(i, j);
				CellData cellData = ((!dungeon.data.CheckInBoundsAndValid(intVector)) ? null : dungeon.data[intVector]);
				if (cellData != null && cellData.type != CellType.WALL && cellData.HasTypeNeighbor(dungeon.data, CellType.WALL))
				{
					hashSet.Add(cellData.position);
				}
			}
		}
		foreach (IntVector2 item in hashSet)
		{
			CellData cellData2 = dungeon.data[item];
			cellData2.breakable = true;
			cellData2.occlusionData.overrideOcclusion = true;
			cellData2.occlusionData.cellOcclusionDirty = true;
			tk2dTileMap2 = dungeon.ConstructWallAtPosition(item.x, item.y);
			r.Cells.Remove(cellData2.position);
			r.CellsWithoutExits.Remove(cellData2.position);
			r.RawCells.Remove(cellData2.position);
		}
		Pixelator.Instance.MarkOcclusionDirty();
		Pixelator.Instance.ProcessOcclusionChange(r.Epicenter, 1f, r, false);
		if ((bool)tk2dTileMap2)
		{
			dungeon.RebuildTilemap(tk2dTileMap2);
		}
	}

	private void ExpandRoom(RoomHandler r)
	{
		Dungeon dungeon = GameManager.Instance.Dungeon;
		AkSoundEngine.PostEvent("Play_OBJ_stone_crumble_01", GameManager.Instance.gameObject);
		tk2dTileMap tk2dTileMap2 = null;
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		for (int i = -5; i < r.area.dimensions.x + 5; i++)
		{
			for (int j = -5; j < r.area.dimensions.y + 5; j++)
			{
				IntVector2 intVector = r.area.basePosition + new IntVector2(i, j);
				CellData cellData = ((!dungeon.data.CheckInBoundsAndValid(intVector)) ? null : dungeon.data[intVector]);
				if (cellData != null && cellData.type == CellType.WALL && cellData.HasTypeNeighbor(dungeon.data, CellType.FLOOR))
				{
					hashSet.Add(cellData.position);
				}
			}
		}
		foreach (IntVector2 item in hashSet)
		{
			CellData cellData2 = dungeon.data[item];
			cellData2.breakable = true;
			cellData2.occlusionData.overrideOcclusion = true;
			cellData2.occlusionData.cellOcclusionDirty = true;
			tk2dTileMap2 = dungeon.DestroyWallAtPosition(item.x, item.y);
			if (UnityEngine.Random.value < 0.25f)
			{
				VFXDustPoof.SpawnAtPosition(item.ToCenterVector3(item.y));
			}
			r.Cells.Add(cellData2.position);
			r.CellsWithoutExits.Add(cellData2.position);
			r.RawCells.Add(cellData2.position);
		}
		Pixelator.Instance.MarkOcclusionDirty();
		Pixelator.Instance.ProcessOcclusionChange(r.Epicenter, 1f, r, false);
		if ((bool)tk2dTileMap2)
		{
			dungeon.RebuildTilemap(tk2dTileMap2);
		}
	}
}
