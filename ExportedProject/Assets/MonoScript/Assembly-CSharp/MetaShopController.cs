using System;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker;
using UnityEngine;

public class MetaShopController : ShopController, IPlaceConfigurable
{
	public TalkDoerLite Witchkeeper;

	public Transform WitchStandPoint;

	public Transform WitchSleepPoint;

	public Transform WitchChairPoint;

	public float ChanceToBeAsleep = 0.5f;

	public HologramDoer Hologramophone;

	public GameObject ExampleBlueprintPrefab;

	public GameObject ExampleBlueprintPrefabItem;

	[SerializeField]
	public List<MetaShopTier> metaShopTiers;

	protected override void Start()
	{
		base.Start();
	}

	public override void OnRoomEnter(PlayerController p)
	{
		if (firstTime)
		{
			firstTime = false;
			OnInitialRoomEnter();
		}
		else
		{
			OnSequentialRoomEnter();
		}
	}

	public override void OnRoomExit()
	{
	}

	protected override void OnInitialRoomEnter()
	{
		float num = UnityEngine.Random.value;
		if (!GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_RECEIVED_ROBOT_ARM_REWARD) && GameStatsManager.Instance.CurrentRobotArmFloor == 0)
		{
			num = ChanceToBeAsleep - 0.01f;
		}
		if (!GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_EVER_TALKED))
		{
			num = ChanceToBeAsleep - 0.01f;
		}
		string text = ((!GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_RECEIVED_ROBOT_ARM_REWARD)) ? "idle" : "idle_hand");
		if (num < ChanceToBeAsleep)
		{
			if (num < ChanceToBeAsleep / 2f)
			{
				Witchkeeper.transform.position = WitchChairPoint.position;
				Witchkeeper.specRigidbody.Reinitialize();
				Witchkeeper.SendPlaymakerEvent("SetChairMode");
				text += "_mask";
			}
			else
			{
				Witchkeeper.transform.position = WitchSleepPoint.position;
				Witchkeeper.specRigidbody.Reinitialize();
				Witchkeeper.SendPlaymakerEvent("SetShopMode");
			}
		}
		else
		{
			Witchkeeper.transform.position = WitchStandPoint.position;
			Witchkeeper.specRigidbody.Reinitialize();
			Witchkeeper.SendPlaymakerEvent("SetStandMode");
		}
		FsmString fsmString = speakerAnimator.playmakerFsm.FsmVariables.GetFsmString("idleAnim");
		fsmString.Value = text;
		speakerAnimator.Play(fsmString.Value);
	}

	protected override void OnSequentialRoomEnter()
	{
	}

	protected override void GunsmithTalk(string message)
	{
		TextBoxManager.ShowTextBox(speechPoint.position, speechPoint, 5f, message, "shopkeep", false);
		speakerAnimator.aiAnimator.PlayForDuration(defaultTalkAction, 2.5f);
	}

	public override void OnBetrayalWarning()
	{
	}

	public override void PullGun()
	{
	}

	public override void NotifyFailedPurchase(ShopItemController itemController)
	{
		if (UnityEngine.Random.value < 0.75f)
		{
			speakerAnimator.SendPlaymakerEvent("playerHasNoMoney");
		}
		else
		{
			Witchkeeper.SendPlaymakerEvent("playerHasNoMoney");
		}
	}

	public override void ReturnToIdle(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		animator.Play(defaultIdleAction);
		animator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(animator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(ReturnToIdle));
	}

	public override void PurchaseItem(ShopItemController item, bool actualPurchase = true, bool allowSign = true)
	{
		if (actualPurchase && !GameManager.Instance.IsSelectingCharacter)
		{
			if (UnityEngine.Random.value < 0.75f)
			{
				speakerAnimator.SendPlaymakerEvent("playerPaid");
			}
			else
			{
				Witchkeeper.SendPlaymakerEvent("playerPaid");
			}
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_PURCHASES_META, 1f);
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.MONEY_SPENT_AT_CURSE_SHOP, item.ModifiedPrice);
		}
		if (item.item.PersistsOnPurchase)
		{
			return;
		}
		m_room.DeregisterInteractable(item);
		if (allowSign)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/Sign_SoldOut"));
			gameObject.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(item.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
			GameObject gameObject2 = null;
			if (shopItemShadowPrefab != null)
			{
				gameObject2 = shopItemShadowPrefab;
			}
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			if (gameObject2 != null)
			{
				tk2dBaseSprite component2 = UnityEngine.Object.Instantiate(gameObject2).GetComponent<tk2dBaseSprite>();
				component.AttachRenderer(component2);
				component2.transform.parent = component.transform;
				component2.transform.localPosition = new Vector3(0f, 0.0625f, 0f);
				component2.HeightOffGround = -0.0625f;
			}
			gameObject.GetComponent<tk2dBaseSprite>().UpdateZDepth();
		}
		UnityEngine.Object.Destroy(item.gameObject);
	}

	public override void ConfigureOnPlacement(RoomHandler room)
	{
		base.ConfigureOnPlacement(room);
	}

	protected MetaShopTier GetCurrentTier()
	{
		for (int i = 0; i < metaShopTiers.Count; i++)
		{
			if (!GameStatsManager.Instance.GetFlag(GetFlagFromTargetItem(metaShopTiers[i].itemId1)) || !GameStatsManager.Instance.GetFlag(GetFlagFromTargetItem(metaShopTiers[i].itemId2)) || !GameStatsManager.Instance.GetFlag(GetFlagFromTargetItem(metaShopTiers[i].itemId3)))
			{
				return metaShopTiers[i];
			}
		}
		return metaShopTiers[metaShopTiers.Count - 1];
	}

	protected MetaShopTier GetProximateTier()
	{
		for (int i = 0; i < metaShopTiers.Count - 1; i++)
		{
			if (!GameStatsManager.Instance.GetFlag(GetFlagFromTargetItem(metaShopTiers[i].itemId1)) || !GameStatsManager.Instance.GetFlag(GetFlagFromTargetItem(metaShopTiers[i].itemId2)) || !GameStatsManager.Instance.GetFlag(GetFlagFromTargetItem(metaShopTiers[i].itemId3)))
			{
				return metaShopTiers[i + 1];
			}
		}
		return null;
	}

	protected GungeonFlags GetFlagFromTargetItem(int shopItemId)
	{
		GungeonFlags result = GungeonFlags.NONE;
		PickupObject byId = PickupObjectDatabase.GetById(shopItemId);
		for (int i = 0; i < byId.encounterTrackable.prerequisites.Length; i++)
		{
			if (byId.encounterTrackable.prerequisites[i].prerequisiteType == DungeonPrerequisite.PrerequisiteType.FLAG)
			{
				result = byId.encounterTrackable.prerequisites[i].saveFlagToCheck;
			}
		}
		return result;
	}

	protected override void DoSetup()
	{
		m_shopItems = new List<GameObject>();
		m_room.Entered += OnRoomEnter;
		m_room.Exited += OnRoomExit;
		MetaShopTier currentTier = GetCurrentTier();
		MetaShopTier proximateTier = GetProximateTier();
		m_itemControllers = new List<ShopItemController>();
		int num = 0;
		if (currentTier != null)
		{
			if (currentTier.itemId1 >= 0)
			{
				m_shopItems.Add(PickupObjectDatabase.GetById(currentTier.itemId1).gameObject);
			}
			if (currentTier.itemId2 >= 0)
			{
				m_shopItems.Add(PickupObjectDatabase.GetById(currentTier.itemId2).gameObject);
			}
			if (currentTier.itemId3 >= 0)
			{
				m_shopItems.Add(PickupObjectDatabase.GetById(currentTier.itemId3).gameObject);
			}
			for (int i = 0; i < spawnPositions.Length; i++)
			{
				Transform spawnTransform = spawnPositions[i];
				if (i < m_shopItems.Count && !(m_shopItems[i] == null))
				{
					PickupObject component = m_shopItems[i].GetComponent<PickupObject>();
					GameObject original = ExampleBlueprintPrefab;
					if (!(component is Gun))
					{
						original = ExampleBlueprintPrefabItem;
					}
					GameObject gameObject = UnityEngine.Object.Instantiate(original, new Vector3(150f, -50f, -100f), Quaternion.identity);
					ItemBlueprintItem component2 = gameObject.GetComponent<ItemBlueprintItem>();
					EncounterTrackable component3 = gameObject.GetComponent<EncounterTrackable>();
					component3.journalData.PrimaryDisplayName = component.encounterTrackable.journalData.PrimaryDisplayName;
					component3.journalData.NotificationPanelDescription = component.encounterTrackable.journalData.NotificationPanelDescription;
					component3.journalData.AmmonomiconFullEntry = component.encounterTrackable.journalData.AmmonomiconFullEntry;
					component3.journalData.AmmonomiconSprite = component.encounterTrackable.journalData.AmmonomiconSprite;
					component3.DoNotificationOnEncounter = false;
					component2.UsesCustomCost = true;
					int customCost = metaShopTiers.IndexOf(currentTier) + 1;
					if (currentTier.overrideTierCost > 0)
					{
						customCost = currentTier.overrideTierCost;
					}
					if (i == 0 && currentTier.overrideItem1Cost > 0)
					{
						customCost = currentTier.overrideItem1Cost;
					}
					if (i == 1 && currentTier.overrideItem2Cost > 0)
					{
						customCost = currentTier.overrideItem2Cost;
					}
					if (i == 2 && currentTier.overrideItem3Cost > 0)
					{
						customCost = currentTier.overrideItem3Cost;
					}
					component2.CustomCost = customCost;
					GungeonFlags gungeonFlags = (component2.SaveFlagToSetOnAcquisition = GetFlagFromTargetItem(component.PickupObjectId));
					component2.HologramIconSpriteName = component3.journalData.AmmonomiconSprite;
					HandleItemPlacement(spawnTransform, component2);
					gameObject.SetActive(false);
					if (GameStatsManager.Instance.GetFlag(component2.SaveFlagToSetOnAcquisition))
					{
						m_itemControllers[i].ForceOutOfStock();
					}
					else
					{
						num++;
					}
				}
			}
		}
		if (proximateTier != null)
		{
			m_shopItems.Clear();
			if (proximateTier.itemId1 >= 0)
			{
				m_shopItems.Add(PickupObjectDatabase.GetById(proximateTier.itemId1).gameObject);
			}
			if (proximateTier.itemId2 >= 0)
			{
				m_shopItems.Add(PickupObjectDatabase.GetById(proximateTier.itemId2).gameObject);
			}
			if (proximateTier.itemId3 >= 0)
			{
				m_shopItems.Add(PickupObjectDatabase.GetById(proximateTier.itemId3).gameObject);
			}
			for (int j = 0; j < spawnPositionsGroup2.Length; j++)
			{
				Transform spawnTransform2 = spawnPositionsGroup2[j];
				if (j < m_shopItems.Count && !(m_shopItems[j] == null))
				{
					PickupObject component4 = m_shopItems[j].GetComponent<PickupObject>();
					GameObject original2 = ExampleBlueprintPrefab;
					if (!(component4 is Gun))
					{
						original2 = ExampleBlueprintPrefabItem;
					}
					GameObject gameObject2 = UnityEngine.Object.Instantiate(original2, new Vector3(150f, -50f, -100f), Quaternion.identity);
					ItemBlueprintItem component5 = gameObject2.GetComponent<ItemBlueprintItem>();
					EncounterTrackable component6 = gameObject2.GetComponent<EncounterTrackable>();
					component6.journalData.PrimaryDisplayName = component4.encounterTrackable.journalData.PrimaryDisplayName;
					component6.journalData.NotificationPanelDescription = component4.encounterTrackable.journalData.NotificationPanelDescription;
					component6.journalData.AmmonomiconFullEntry = component4.encounterTrackable.journalData.AmmonomiconFullEntry;
					component6.journalData.AmmonomiconSprite = component4.encounterTrackable.journalData.AmmonomiconSprite;
					component6.DoNotificationOnEncounter = false;
					component5.UsesCustomCost = true;
					int customCost2 = metaShopTiers.IndexOf(proximateTier) + 1;
					if (proximateTier.overrideTierCost > 0)
					{
						customCost2 = proximateTier.overrideTierCost;
					}
					if (j == 0 && proximateTier.overrideItem1Cost > 0)
					{
						customCost2 = proximateTier.overrideItem1Cost;
					}
					if (j == 1 && proximateTier.overrideItem2Cost > 0)
					{
						customCost2 = proximateTier.overrideItem2Cost;
					}
					if (j == 2 && proximateTier.overrideItem3Cost > 0)
					{
						customCost2 = proximateTier.overrideItem3Cost;
					}
					component5.CustomCost = customCost2;
					GungeonFlags gungeonFlags2 = (component5.SaveFlagToSetOnAcquisition = GetFlagFromTargetItem(component4.PickupObjectId));
					component5.HologramIconSpriteName = component6.journalData.AmmonomiconSprite;
					HandleItemPlacement(spawnTransform2, component5);
					gameObject2.SetActive(false);
					if (GameStatsManager.Instance.GetFlag(component5.SaveFlagToSetOnAcquisition))
					{
						m_itemControllers[m_itemControllers.Count - 1].ForceOutOfStock();
					}
					else
					{
						num++;
					}
				}
			}
		}
		if (GameManager.Instance.platformInterface != null && num == 0 && proximateTier == null)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.SPEND_META_CURRENCY);
		}
		for (int k = 0; k < m_itemControllers.Count; k++)
		{
			m_itemControllers[k].sprite.IsPerpendicular = true;
			m_itemControllers[k].sprite.UpdateZDepth();
			m_itemControllers[k].CurrencyType = ShopItemController.ShopCurrencyType.META_CURRENCY;
		}
	}

	private void HandleItemPlacement(Transform spawnTransform, PickupObject shopItem)
	{
		GameObject gameObject = new GameObject("Shop item " + Array.IndexOf(spawnPositions, spawnTransform));
		Transform transform = gameObject.transform;
		transform.parent = spawnTransform;
		transform.localPosition = Vector3.zero;
		EncounterTrackable component = shopItem.GetComponent<EncounterTrackable>();
		if (component != null)
		{
			GameManager.Instance.ExtantShopTrackableGuids.Add(component.EncounterGuid);
		}
		ShopItemController shopItemController = gameObject.AddComponent<ShopItemController>();
		if (spawnTransform.name.Contains("SIDE") || spawnTransform.name.Contains("EAST"))
		{
			shopItemController.itemFacing = DungeonData.Direction.EAST;
		}
		else if (spawnTransform.name.Contains("WEST"))
		{
			shopItemController.itemFacing = DungeonData.Direction.WEST;
		}
		else if (spawnTransform.name.Contains("NORTH"))
		{
			shopItemController.itemFacing = DungeonData.Direction.NORTH;
		}
		if (!m_room.IsRegistered(shopItemController))
		{
			m_room.RegisterInteractable(shopItemController);
		}
		shopItemController.Initialize(shopItem, this);
		transform.localPosition += new Vector3(0.0625f, 0f, 0f);
		m_itemControllers.Add(shopItemController);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
