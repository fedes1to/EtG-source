using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public static class LootEngine
{
	public enum AmmoDropType
	{
		DEFAULT_AMMO,
		SPREAD_AMMO
	}

	private const float HIGH_AMMO_THRESHOLD = 0.9f;

	private const float LOW_AMMO_THRESHOLD = 0.1f;

	private const float AMMO_DROP_CHANCE_REDUCTION_FACTOR = 0.05f;

	public static void ClearPerLevelData()
	{
		StaticReferenceManager.WeaponChestsSpawnedOnFloor = 0;
		StaticReferenceManager.ItemChestsSpawnedOnFloor = 0;
		StaticReferenceManager.DChestsSpawnedOnFloor = 0;
	}

	public static void SpawnHealth(Vector2 centerPoint, int halfHearts, Vector2? direction, float startingZForce = 4f, float startingHeight = 0.05f)
	{
		int num;
		for (num = halfHearts; num >= 2; num -= 2)
		{
			SpawnItem(GameManager.Instance.RewardManager.FullHeartPrefab.gameObject, centerPoint, (!direction.HasValue) ? Vector2.up : direction.Value, startingZForce);
		}
		while (num >= 1)
		{
			SpawnItem(GameManager.Instance.RewardManager.HalfHeartPrefab.gameObject, centerPoint, (!direction.HasValue) ? Vector2.up : direction.Value, startingZForce);
			num--;
		}
	}

	public static GameObject SpawnBowlerNote(GameObject note, Vector2 position, RoomHandler parentRoom, bool doPoof = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(note, position.ToVector3ZisY(), Quaternion.identity);
		if ((bool)gameObject)
		{
			IPlayerInteractable[] interfacesInChildren = gameObject.GetInterfacesInChildren<IPlayerInteractable>();
			for (int i = 0; i < interfacesInChildren.Length; i++)
			{
				parentRoom.RegisterInteractable(interfacesInChildren[i]);
			}
		}
		if (doPoof)
		{
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
			tk2dBaseSprite component = gameObject2.GetComponent<tk2dBaseSprite>();
			component.PlaceAtPositionByAnchor(position.ToVector3ZUp() + new Vector3(0.5f, 0.75f, 0f), tk2dBaseSprite.Anchor.MiddleCenter);
			component.HeightOffGround = 5f;
			component.UpdateZDepth();
		}
		return gameObject;
	}

	public static void SpawnCurrency(Vector2 centerPoint, int amountToDrop, bool isMetaCurrency, Vector2? direction, float? angleVariance, float startingZForce = 4f, float startingHeight = 0.05f)
	{
		if (!isMetaCurrency && PassiveItem.IsFlagSetAtAll(typeof(BankBagItem)))
		{
			amountToDrop *= 2;
		}
		List<GameObject> currencyToDrop = GameManager.Instance.Dungeon.sharedSettingsPrefab.GetCurrencyToDrop(amountToDrop, isMetaCurrency);
		float num = 360f / (float)currencyToDrop.Count;
		if (angleVariance.HasValue)
		{
			num = angleVariance.Value * 2f / (float)currencyToDrop.Count;
		}
		Vector3 vector = Vector3.up;
		if (direction.HasValue && angleVariance.HasValue)
		{
			vector = Quaternion.Euler(0f, 0f, 0f - angleVariance.Value) * direction.Value;
		}
		else if (direction.HasValue)
		{
			vector = direction.Value.ToVector3ZUp();
		}
		for (int i = 0; i < currencyToDrop.Count; i++)
		{
			Vector3 vector2 = Quaternion.Euler(0f, 0f, num * (float)i) * vector;
			vector2 *= 2f;
			GameObject gameObject = SpawnManager.SpawnDebris(currencyToDrop[i], centerPoint.ToVector3ZUp(centerPoint.y), Quaternion.identity);
			DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
			orAddComponent.shouldUseSRBMotion = true;
			orAddComponent.angularVelocity = 0f;
			orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
			orAddComponent.Trigger(vector2.WithZ(startingZForce), startingHeight);
			orAddComponent.canRotate = false;
		}
	}

	public static void SpawnCurrencyManual(Vector2 centerPoint, int amountToDrop)
	{
		List<GameObject> currencyToDrop = GameManager.Instance.Dungeon.sharedSettingsPrefab.GetCurrencyToDrop(amountToDrop, false, true);
		float num = 360f / (float)currencyToDrop.Count;
		Vector3 up = Vector3.up;
		List<CurrencyPickup> list = new List<CurrencyPickup>();
		for (int i = 0; i < currencyToDrop.Count; i++)
		{
			Vector3 vector = Quaternion.Euler(0f, 0f, num * (float)i) * up;
			vector *= 2f;
			GameObject gameObject = SpawnManager.SpawnDebris(currencyToDrop[i], centerPoint.ToVector3ZUp(centerPoint.y), Quaternion.identity);
			CurrencyPickup component = gameObject.GetComponent<CurrencyPickup>();
			component.PreventPickup = true;
			list.Add(component);
			PickupMover component2 = gameObject.GetComponent<PickupMover>();
			if ((bool)component2)
			{
				component2.enabled = false;
			}
			DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
			orAddComponent.OnGrounded = (Action<DebrisObject>)Delegate.Combine(orAddComponent.OnGrounded, (Action<DebrisObject>)delegate(DebrisObject sourceDebris)
			{
				sourceDebris.GetComponent<CurrencyPickup>().PreventPickup = false;
				sourceDebris.OnGrounded = null;
			});
			orAddComponent.shouldUseSRBMotion = true;
			orAddComponent.angularVelocity = 0f;
			orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
			orAddComponent.Trigger(vector.WithZ(2f) * UnityEngine.Random.Range(1.5f, 2.125f), 0.05f);
			orAddComponent.canRotate = false;
		}
		GameManager.Instance.Dungeon.StartCoroutine(HandleManualCoinSpawnLifespan(list));
	}

	private static IEnumerator HandleManualCoinSpawnLifespan(List<CurrencyPickup> coins)
	{
		float elapsed = 0f;
		while (elapsed < BankBagItem.cachedCoinLifespan * 0.75f)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		float flickerTimer = 0f;
		while (elapsed < BankBagItem.cachedCoinLifespan)
		{
			elapsed += BraveTime.DeltaTime;
			flickerTimer += BraveTime.DeltaTime;
			for (int i = 0; i < coins.Count; i++)
			{
				if ((bool)coins[i] && (bool)coins[i].renderer)
				{
					bool enabled = flickerTimer % 0.2f > 0.15f;
					coins[i].renderer.enabled = enabled;
				}
			}
			yield return null;
		}
		for (int j = 0; j < coins.Count; j++)
		{
			if ((bool)coins[j])
			{
				UnityEngine.Object.Destroy(coins[j].gameObject);
			}
		}
	}

	public static void SpawnCurrency(Vector2 centerPoint, int amountToDrop, bool isMetaCurrency = false)
	{
		SpawnCurrency(centerPoint, amountToDrop, isMetaCurrency, null, null);
	}

	public static bool DoAmmoClipCheck(FloorRewardData currentRewardData, out AmmoDropType AmmoToDrop)
	{
		bool flag = DoAmmoClipCheck(currentRewardData.FloorChanceToDropAmmo);
		AmmoToDrop = AmmoDropType.DEFAULT_AMMO;
		if (flag)
		{
			AmmoToDrop = ((UnityEngine.Random.value < currentRewardData.FloorChanceForSpreadAmmo) ? AmmoDropType.SPREAD_AMMO : AmmoDropType.DEFAULT_AMMO);
			return true;
		}
		return false;
	}

	public static bool DoAmmoClipCheck(float baseAmmoDropChance)
	{
		float num = baseAmmoDropChance;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			num *= GameManager.Instance.RewardManager.CoopAmmoChanceModifier;
		}
		float num2 = 1f;
		float num3 = PlayerStats.GetTotalCurse();
		num2 += Mathf.Clamp01(num3 / 10f) / 2f;
		num *= num2;
		if (GameManager.Instance.AllPlayers != null)
		{
			float num4 = 0f;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (!GameManager.Instance.AllPlayers[i] || GameManager.Instance.AllPlayers[i].inventory == null || GameManager.Instance.AllPlayers[i].inventory.AllGuns == null)
				{
					continue;
				}
				for (int j = 0; j < GameManager.Instance.AllPlayers[i].inventory.AllGuns.Count; j++)
				{
					Gun gun = GameManager.Instance.AllPlayers[i].inventory.AllGuns[j];
					if ((bool)gun && !gun.InfiniteAmmo)
					{
						num4 = 1f;
					}
				}
			}
			num *= num4;
		}
		return UnityEngine.Random.value < num;
	}

	private static void PostprocessGunSpawn(Gun spawnedGun)
	{
		spawnedGun.gameObject.SetActive(true);
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_HAS_BEEN_PEDESTAL_MIMICKED) && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE && UnityEngine.Random.value < GameManager.Instance.RewardManager.GunMimicMimicGunChance)
		{
			spawnedGun.gameObject.AddComponent<MimicGunMimicModifier>();
		}
	}

	private static void PostprocessItemSpawn(DebrisObject item)
	{
		tk2dSpriteAnimator component = item.gameObject.GetComponent<tk2dSpriteAnimator>();
		CurrencyPickup component2 = item.GetComponent<CurrencyPickup>();
		if (component2 == null && !item.GetComponent<BulletThatCanKillThePast>() && (component == null || !component.playAutomatically))
		{
			item.gameObject.GetOrAddComponent<SquishyBounceWiggler>();
		}
		PlayerItem component3 = item.GetComponent<PlayerItem>();
		if (component3 != null && !RoomHandler.unassignedInteractableObjects.Contains(component3))
		{
			RoomHandler.unassignedInteractableObjects.Add(component3);
		}
		PassiveItem component4 = item.GetComponent<PassiveItem>();
		if (component4 != null && !RoomHandler.unassignedInteractableObjects.Contains(component4))
		{
			RoomHandler.unassignedInteractableObjects.Add(component4);
		}
		AmmoPickup component5 = item.GetComponent<AmmoPickup>();
		if (component5 != null && !RoomHandler.unassignedInteractableObjects.Contains(component5))
		{
			RoomHandler.unassignedInteractableObjects.Add(component5);
		}
		HealthPickup component6 = item.GetComponent<HealthPickup>();
		if (component6 != null && !RoomHandler.unassignedInteractableObjects.Contains(component6))
		{
			RoomHandler.unassignedInteractableObjects.Add(component6);
		}
		item.OnGrounded = (Action<DebrisObject>)Delegate.Remove(item.OnGrounded, new Action<DebrisObject>(PostprocessItemSpawn));
	}

	private static DebrisObject SpawnInternal(GameObject spawnedItem, Vector3 spawnPosition, Vector2 spawnDirection, float force, bool invalidUntilGrounded = true, bool doDefaultItemPoof = false, bool disablePostprocessing = false, bool disableHeightBoost = false)
	{
		Vector3 normalized = spawnDirection.ToVector3ZUp().normalized;
		normalized *= force;
		Gun component = spawnedItem.GetComponent<Gun>();
		if (component != null)
		{
			PostprocessGunSpawn(component);
			DebrisObject debrisObject = component.DropGun(2f);
			if (doDefaultItemPoof)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
				tk2dBaseSprite component2 = gameObject.GetComponent<tk2dBaseSprite>();
				component2.PlaceAtPositionByAnchor(debrisObject.sprite.WorldCenter.ToVector3ZUp() + new Vector3(0f, 0.5f, 0f), tk2dBaseSprite.Anchor.MiddleCenter);
				component2.HeightOffGround = 5f;
				component2.UpdateZDepth();
			}
			return debrisObject;
		}
		DebrisObject orAddComponent = spawnedItem.GetOrAddComponent<DebrisObject>();
		if (!disablePostprocessing)
		{
			orAddComponent.OnGrounded = (Action<DebrisObject>)Delegate.Combine(orAddComponent.OnGrounded, new Action<DebrisObject>(PostprocessItemSpawn));
		}
		orAddComponent.additionalHeightBoost = ((!disableHeightBoost) ? 1.5f : 0f);
		orAddComponent.shouldUseSRBMotion = true;
		orAddComponent.angularVelocity = 0f;
		orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
		orAddComponent.sprite.UpdateZDepth();
		orAddComponent.Trigger(normalized.WithZ(2f), (!disableHeightBoost) ? 0.5f : 0f);
		orAddComponent.canRotate = false;
		if (invalidUntilGrounded && orAddComponent.specRigidbody != null)
		{
			orAddComponent.specRigidbody.CollideWithOthers = false;
			orAddComponent.OnTouchedGround = (Action<DebrisObject>)Delegate.Combine(orAddComponent.OnTouchedGround, new Action<DebrisObject>(BecomeViableItem));
		}
		orAddComponent.AssignFinalWorldDepth(-0.5f);
		if (doDefaultItemPoof)
		{
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
			tk2dBaseSprite component3 = gameObject2.GetComponent<tk2dBaseSprite>();
			component3.PlaceAtPositionByAnchor(orAddComponent.sprite.WorldCenter.ToVector3ZUp() + new Vector3(0f, 0.5f, 0f), tk2dBaseSprite.Anchor.MiddleCenter);
			component3.HeightOffGround = 5f;
			component3.UpdateZDepth();
		}
		return orAddComponent;
	}

	public static void DoDefaultSynergyPoof(Vector2 worldPosition, bool ignoreTimeScale = false)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Synergy_Poof_001"));
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		component.PlaceAtPositionByAnchor(worldPosition.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
		component.HeightOffGround = 5f;
		component.UpdateZDepth();
		if (ignoreTimeScale)
		{
			tk2dSpriteAnimator component2 = component.GetComponent<tk2dSpriteAnimator>();
			if (component2 != null)
			{
				component2.ignoreTimeScale = true;
				component2.alwaysUpdateOffscreen = true;
			}
		}
	}

	public static void DoDefaultPurplePoof(Vector2 worldPosition, bool ignoreTimeScale = false)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Purple_Smoke_001"));
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		component.PlaceAtPositionByAnchor(worldPosition.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
		component.HeightOffGround = 5f;
		component.UpdateZDepth();
		if (ignoreTimeScale)
		{
			tk2dSpriteAnimator component2 = component.GetComponent<tk2dSpriteAnimator>();
			if (component2 != null)
			{
				component2.ignoreTimeScale = true;
				component2.alwaysUpdateOffscreen = true;
			}
		}
	}

	public static void DoDefaultItemPoof(Vector2 worldPosition, bool ignoreTimeScale = false, bool muteAudio = false)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		component.PlaceAtPositionByAnchor(worldPosition.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
		component.HeightOffGround = 5f;
		component.UpdateZDepth();
		if (!ignoreTimeScale && !muteAudio)
		{
			return;
		}
		tk2dSpriteAnimator component2 = component.GetComponent<tk2dSpriteAnimator>();
		if (component2 != null)
		{
			if (ignoreTimeScale)
			{
				component2.ignoreTimeScale = true;
				component2.alwaysUpdateOffscreen = true;
			}
			if (muteAudio)
			{
				component2.MuteAudio = true;
			}
		}
	}

	public static DebrisObject DropItemWithoutInstantiating(GameObject item, Vector3 spawnPosition, Vector2 spawnDirection, float force, bool invalidUntilGrounded = true, bool doDefaultItemPoof = false, bool disablePostprocessing = false, bool disableHeightBoost = false)
	{
		if (item.GetComponent<DebrisObject>() != null)
		{
			UnityEngine.Object.DestroyImmediate(item.GetComponent<DebrisObject>());
		}
		item.GetComponent<Renderer>().enabled = true;
		item.transform.parent = null;
		item.transform.position = spawnPosition;
		item.transform.rotation = Quaternion.identity;
		return SpawnInternal(item, spawnPosition, spawnDirection, force, invalidUntilGrounded, doDefaultItemPoof, disablePostprocessing, disableHeightBoost);
	}

	public static DebrisObject SpawnItem(GameObject item, Vector3 spawnPosition, Vector2 spawnDirection, float force, bool invalidUntilGrounded = true, bool doDefaultItemPoof = false, bool disableHeightBoost = false)
	{
		if (GameStatsManager.HasInstance && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE))
		{
			PickupObject component = item.GetComponent<PickupObject>();
			if ((bool)component && component.PickupObjectId == GlobalItemIds.UnfinishedGun)
			{
				item = PickupObjectDatabase.GetById(GlobalItemIds.FinishedGun).gameObject;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(item, spawnPosition, Quaternion.identity);
		GameObject spawnedItem = gameObject;
		bool invalidUntilGrounded2 = invalidUntilGrounded;
		bool doDefaultItemPoof2 = doDefaultItemPoof;
		bool disableHeightBoost2 = disableHeightBoost;
		return SpawnInternal(spawnedItem, spawnPosition, spawnDirection, force, invalidUntilGrounded2, doDefaultItemPoof2, false, disableHeightBoost2);
	}

	public static void DelayedSpawnItem(float delay, GameObject item, Vector3 spawnPosition, Vector2 spawnDirection, float force, bool invalidUntilGrounded = true, bool doDefaultItemPoof = false, bool disableHeightBoost = false)
	{
		GameManager.Instance.StartCoroutine(DelayedSpawnItem_CR(delay, item, spawnPosition, spawnDirection, force, invalidUntilGrounded, doDefaultItemPoof, disableHeightBoost));
	}

	private static IEnumerator DelayedSpawnItem_CR(float delay, GameObject item, Vector3 spawnPosition, Vector2 spawnDirection, float force, bool invalidUntilGrounded = true, bool doDefaultItemPoof = false, bool disableHeightBoost = false)
	{
		float elapsed = 0f;
		while (elapsed < delay)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		SpawnItem(item, spawnPosition, spawnDirection, force, invalidUntilGrounded, doDefaultItemPoof, disableHeightBoost);
	}

	public static void GivePrefabToPlayer(GameObject item, PlayerController player)
	{
		Gun component = item.GetComponent<Gun>();
		if (component != null)
		{
			EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
			if (component2 != null)
			{
				component2.HandleEncounter();
			}
			if (player.CharacterUsesRandomGuns)
			{
				player.ChangeToRandomGun();
			}
			else
			{
				player.inventory.AddGunToInventory(component, true);
			}
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(item, Vector3.zero, Quaternion.identity);
		PickupObject component3 = gameObject.GetComponent<PickupObject>();
		if (component3 != null)
		{
			if (component3 is PlayerItem)
			{
				(component3 as PlayerItem).ForceAsExtant = true;
			}
			component3.Pickup(player);
		}
		else
		{
			Debug.LogError("Failed in giving item to player; item " + item.name + " is not a pickupObject.");
		}
	}

	public static Gun TryGiveGunToPlayer(GameObject item, PlayerController player, bool attemptForce = false)
	{
		Gun g3 = item.GetComponent<Gun>();
		if (g3 != null)
		{
			if (player.inventory.AllGuns.Count >= player.inventory.maxGuns && !attemptForce)
			{
				Gun gun = player.inventory.AllGuns.Find((Gun g2) => g2.PickupObjectId == g3.PickupObjectId);
				if (gun == null || gun.CurrentAmmo >= gun.AdjustedMaxAmmo)
				{
					SpewLoot(item, player.specRigidbody.UnitCenter);
					return null;
				}
			}
			EncounterTrackable component = g3.GetComponent<EncounterTrackable>();
			if (component != null)
			{
				component.HandleEncounter();
			}
			return player.inventory.AddGunToInventory(g3, true);
		}
		return null;
	}

	public static bool TryGivePrefabToPlayer(GameObject item, PlayerController player, bool attemptForce = false)
	{
		Gun g3 = item.GetComponent<Gun>();
		if (g3 != null)
		{
			if (player.inventory.AllGuns.Count >= player.inventory.maxGuns && !attemptForce)
			{
				Gun gun = player.inventory.AllGuns.Find((Gun g2) => g2.PickupObjectId == g3.PickupObjectId);
				if (gun == null || gun.CurrentAmmo >= gun.AdjustedMaxAmmo)
				{
					SpewLoot(item, player.specRigidbody.UnitCenter);
					return false;
				}
			}
			EncounterTrackable component = g3.GetComponent<EncounterTrackable>();
			if (component != null)
			{
				component.HandleEncounter();
			}
			player.inventory.AddGunToInventory(g3, true);
			return true;
		}
		PlayerItem component2 = item.GetComponent<PlayerItem>();
		if ((bool)component2 && player.activeItems.Count >= player.maxActiveItemsHeld && !attemptForce)
		{
			SpewLoot(item, player.specRigidbody.UnitCenter);
			return false;
		}
		PickupObject component3 = UnityEngine.Object.Instantiate(item, Vector3.zero, Quaternion.identity).GetComponent<PickupObject>();
		if (component3 == null)
		{
			Debug.LogError("Failed in giving item to player; item " + item.name + " is not a pickupObject.");
			return false;
		}
		if (component3 is PlayerItem)
		{
			(component3 as PlayerItem).ForceAsExtant = true;
		}
		component3.Pickup(player);
		return true;
	}

	private static void BecomeViableItem(DebrisObject debris)
	{
		debris.specRigidbody.CollideWithOthers = true;
	}

	public static DebrisObject SpewLoot(GameObject itemToSpawn, Vector3 spawnPosition)
	{
		DebrisObject debrisObject = null;
		Vector3 vector = Quaternion.Euler(0f, 0f, 0f) * Vector3.down;
		vector *= 2f;
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE))
		{
			PickupObject component = itemToSpawn.GetComponent<PickupObject>();
			if ((bool)component && component.PickupObjectId == GlobalItemIds.UnfinishedGun)
			{
				itemToSpawn = PickupObjectDatabase.GetById(GlobalItemIds.FinishedGun).gameObject;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
		Gun component2 = gameObject.GetComponent<Gun>();
		if (component2 != null)
		{
			PostprocessGunSpawn(component2);
			return component2.DropGun(2f);
		}
		DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
		if (component2 == null)
		{
			orAddComponent.OnGrounded = (Action<DebrisObject>)Delegate.Combine(orAddComponent.OnGrounded, new Action<DebrisObject>(PostprocessItemSpawn));
		}
		orAddComponent.FlagAsPickup();
		orAddComponent.additionalHeightBoost = 1.5f;
		orAddComponent.shouldUseSRBMotion = true;
		orAddComponent.angularVelocity = 0f;
		orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
		orAddComponent.sprite.UpdateZDepth();
		orAddComponent.AssignFinalWorldDepth(-0.5f);
		orAddComponent.Trigger(Vector3.zero, 0.5f);
		orAddComponent.canRotate = false;
		return orAddComponent;
	}

	public static List<DebrisObject> SpewLoot(List<GameObject> itemsToSpawn, Vector3 spawnPosition)
	{
		List<DebrisObject> list = new List<DebrisObject>();
		float num = ((itemsToSpawn.Count != 8) ? 0f : 22.5f);
		float num2 = 360f / (float)itemsToSpawn.Count;
		for (int i = 0; i < itemsToSpawn.Count; i++)
		{
			Vector3 vector = Quaternion.Euler(0f, 0f, num + num2 * (float)i) * Vector3.down;
			vector *= 2f;
			GameObject gameObject = UnityEngine.Object.Instantiate(itemsToSpawn[i], spawnPosition, Quaternion.identity);
			Gun component = gameObject.GetComponent<Gun>();
			if (component != null)
			{
				PostprocessGunSpawn(component);
				list.Add(component.DropGun(2f));
				continue;
			}
			DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
			if (component == null)
			{
				orAddComponent.OnGrounded = (Action<DebrisObject>)Delegate.Combine(orAddComponent.OnGrounded, new Action<DebrisObject>(PostprocessItemSpawn));
			}
			orAddComponent.additionalHeightBoost = 1.5f;
			orAddComponent.shouldUseSRBMotion = true;
			orAddComponent.angularVelocity = 0f;
			orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
			orAddComponent.sprite.UpdateZDepth();
			orAddComponent.AssignFinalWorldDepth(-0.5f);
			orAddComponent.Trigger(vector.WithZ(2f), 0.5f);
			orAddComponent.canRotate = false;
			list.Add(orAddComponent);
		}
		for (int j = 0; j < list.Count; j++)
		{
			if ((bool)list[j].sprite)
			{
				list[j].sprite.UpdateZDepth();
			}
		}
		return list;
	}

	private static PickupObject.ItemQuality GetRandomItemTier()
	{
		return (PickupObject.ItemQuality)UnityEngine.Random.Range(0, 6);
	}

	public static List<T> GetItemsOfQualityFromList<T>(List<T> validObjects, PickupObject.ItemQuality quality) where T : PickupObject
	{
		List<T> list = new List<T>();
		for (int i = 0; i < validObjects.Count; i++)
		{
			if (validObjects[i].quality == quality)
			{
				list.Add(validObjects[i]);
			}
		}
		return list;
	}

	public static T GetItemOfTypeAndQuality<T>(PickupObject.ItemQuality itemQuality, GenericLootTable lootTable, bool anyQuality = false) where T : PickupObject
	{
		List<T> list = new List<T>();
		if (lootTable != null)
		{
			List<WeightedGameObject> compiledRawItems = lootTable.GetCompiledRawItems();
			for (int i = 0; i < compiledRawItems.Count; i++)
			{
				if (compiledRawItems[i].gameObject == null)
				{
					continue;
				}
				T component = compiledRawItems[i].gameObject.GetComponent<T>();
				if (!((UnityEngine.Object)component != (UnityEngine.Object)null) || !component.PrerequisitesMet())
				{
					continue;
				}
				EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
				if (component2 != null)
				{
					int num = GameStatsManager.Instance.QueryEncounterableDifferentiator(component2);
					if (num > 0)
					{
						continue;
					}
				}
				list.Add(component);
			}
		}
		else
		{
			for (int j = 0; j < PickupObjectDatabase.Instance.Objects.Count; j++)
			{
				T val = PickupObjectDatabase.Instance.Objects[j] as T;
				if (val is ContentTeaserGun || val is ContentTeaserItem || !((UnityEngine.Object)val != (UnityEngine.Object)null) || !val.PrerequisitesMet())
				{
					continue;
				}
				EncounterTrackable component3 = val.GetComponent<EncounterTrackable>();
				if (component3 != null)
				{
					int num2 = GameStatsManager.Instance.QueryEncounterableDifferentiator(component3);
					if (num2 > 0)
					{
						continue;
					}
				}
				list.Add(val);
			}
		}
		if (list.Count == 0)
		{
			return (T)null;
		}
		if (anyQuality)
		{
			if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
		}
		else
		{
			while (itemQuality >= PickupObject.ItemQuality.COMMON)
			{
				List<T> itemsOfQualityFromList = GetItemsOfQualityFromList(list, itemQuality);
				if (itemsOfQualityFromList.Count > 0)
				{
					return itemsOfQualityFromList[UnityEngine.Random.Range(0, itemsOfQualityFromList.Count)];
				}
				itemQuality--;
			}
		}
		return (T)null;
	}
}
