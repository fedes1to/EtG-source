using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

public class BaseShopController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public enum AdditionalShopType
	{
		NONE,
		GOOP,
		BLANK,
		KEY,
		CURSE,
		TRUCK,
		FOYER_META,
		BLACKSMITH,
		RESRAT_SHORTCUT
	}

	protected enum ShopState
	{
		Default,
		GunDrawn,
		Hostile,
		PreTeleportAway,
		TeleportAway,
		Gone,
		RefuseService
	}

	private const bool c_allowShopkeepBossFloor = false;

	public AdditionalShopType baseShopType;

	public bool FoyerMetaShopForcedTiers;

	public bool IsBeetleMerchant;

	public GameObject ExampleBlueprintPrefab;

	[Header("Spawn Group 1")]
	public GenericLootTable shopItems;

	public Transform[] spawnPositions;

	[Header("Spawn Group 2")]
	public GenericLootTable shopItemsGroup2;

	public Transform[] spawnPositionsGroup2;

	public float spawnGroupTwoItem1Chance = 0.5f;

	public float spawnGroupTwoItem2Chance = 0.5f;

	public float spawnGroupTwoItem3Chance = 0.5f;

	[Header("Other Settings")]
	public PlayMakerFSM shopkeepFSM;

	public GameObject shopItemShadowPrefab;

	public GameObject cat;

	public GameObject OptionalMinimapIcon;

	public float ShopCostModifier = 1f;

	[LongEnum]
	public GungeonFlags FlagToSetOnEncounter;

	private OverridableBool m_capableOfBeingStolenFrom = new OverridableBool(false);

	private int m_numberThingsPurchased;

	private static bool m_hasLockedShopProcedurally;

	private bool m_hasBeenEntered;

	private int m_numberOfFirstTypeItems;

	protected bool PreventTeleportingPlayerAway;

	protected List<GameObject> m_shopItems;

	protected List<ShopItemController> m_itemControllers;

	protected RoomHandler m_room;

	protected TalkDoerLite m_shopkeep;

	private FakeGameActorEffectHandler m_fakeEffectHandler;

	[NonSerialized]
	public bool BeetleExhausted;

	private bool m_onLastStockBeetle;

	protected ShopState m_state;

	protected bool firstTime = true;

	protected int m_hitCount;

	protected float m_timeSinceLastHit = 10f;

	protected float m_preTeleportTimer;

	protected bool m_wasCaughtStealing;

	private float m_stealChance = 1f;

	private int m_itemsStolen;

	private static float s_mainShopkeepStealChance = 1f;

	private static int s_mainShopkeepItemsStolen;

	private static bool s_emptyFutureShops;

	protected bool IsMainShopkeep
	{
		get
		{
			return cat;
		}
	}

	public float StealChance
	{
		get
		{
			return (!IsMainShopkeep) ? m_stealChance : s_mainShopkeepStealChance;
		}
		protected set
		{
			if (IsMainShopkeep)
			{
				s_mainShopkeepStealChance = value;
			}
			else
			{
				m_stealChance = value;
			}
		}
	}

	public int ItemsStolen
	{
		get
		{
			return (!IsMainShopkeep) ? m_itemsStolen : s_mainShopkeepItemsStolen;
		}
		protected set
		{
			if (IsMainShopkeep)
			{
				s_mainShopkeepItemsStolen = value;
			}
			else
			{
				m_itemsStolen = value;
			}
		}
	}

	public Vector2 CenterPosition
	{
		get
		{
			if (!base.specRigidbody || base.specRigidbody.HitboxPixelCollider == null)
			{
				if ((bool)base.sprite)
				{
					return base.sprite.WorldCenter;
				}
				return base.transform.position.XY();
			}
			return base.specRigidbody.HitboxPixelCollider.UnitCenter;
		}
	}

	public bool IsCapableOfBeingStolenFrom
	{
		get
		{
			return m_capableOfBeingStolenFrom.Value;
		}
	}

	public bool WasCaughtStealing
	{
		get
		{
			return m_wasCaughtStealing;
		}
	}

	public static bool HasLockedShopProcedurally
	{
		get
		{
			return m_hasLockedShopProcedurally;
		}
		set
		{
			m_hasLockedShopProcedurally = value;
		}
	}

	private ShopState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	public void SetCapableOfBeingStolenFrom(bool value, string reason, float? duration = null)
	{
		m_capableOfBeingStolenFrom.SetOverride(reason, value, duration);
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].ForceRefreshInteractable = true;
		}
	}

	protected IEnumerator Start()
	{
		StaticReferenceManager.AllShops.Add(this);
		if (IsMainShopkeep)
		{
			StealChance = Mathf.Min(StealChance + 0.25f, 1f);
		}
		if (baseShopType == AdditionalShopType.FOYER_META)
		{
			StartCoroutine(HandleDelayedFoyerInitialization());
		}
		else
		{
			DoSetup();
		}
		m_shopkeep = shopkeepFSM.GetComponent<TalkDoerLite>();
		if ((bool)m_shopkeep)
		{
			m_fakeEffectHandler = m_shopkeep.gameObject.GetOrAddComponent<FakeGameActorEffectHandler>();
		}
		if (baseShopType != AdditionalShopType.FOYER_META)
		{
			yield return null;
			tk2dBaseSprite[] childSprites = GetComponentsInChildren<tk2dBaseSprite>(true);
			for (int i = 0; i < childSprites.Length; i++)
			{
				childSprites[i].UpdateZDepth();
			}
		}
		if (IsMainShopkeep && s_emptyFutureShops)
		{
			State = ShopState.Gone;
		}
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		Debug.LogWarning("s_empty?: " + s_emptyFutureShops);
		if (IsMainShopkeep && s_emptyFutureShops)
		{
			State = ShopState.Gone;
		}
		if (!IsBeetleMerchant)
		{
			yield break;
		}
		tk2dSpriteAnimator component = base.transform.Find("dung").GetComponent<tk2dSpriteAnimator>();
		if (UnityEngine.Random.value > 0.5f)
		{
			m_shopkeep.transform.position += new Vector3(0.125f, 1.9375f, 0f);
			m_shopkeep.sprite.HeightOffGround = 1f;
			m_shopkeep.sprite.UpdateZDepth();
			AIAnimator aIAnimator = m_shopkeep.aiAnimator;
			aIAnimator.IdleAnimation.Prefix = "idle_sit";
			aIAnimator.TalkAnimation.Type = DirectionalAnimation.DirectionType.Single;
			aIAnimator.TalkAnimation.Prefix = "talk_sit";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "not_sit_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "no_sit_left";
			aIAnimator.OtherAnimations[1].anim.Type = DirectionalAnimation.DirectionType.Single;
			aIAnimator.OtherAnimations[1].anim.Prefix = "yes_sit";
			if ((bool)component)
			{
				component.sprite.SetSprite("dung_pack_sit_001");
			}
		}
		else if ((bool)component)
		{
			component.Play("dungpack_idle");
		}
	}

	private IEnumerator HandleDelayedFoyerInitialization()
	{
		while (GameManager.Instance.IsSelectingCharacter || GameManager.Instance.PrimaryPlayer == null)
		{
			yield return null;
		}
		DoSetup();
	}

	private void Update()
	{
		if ((State == ShopState.Default || State == ShopState.GunDrawn) && SuperReaperController.Instance != null)
		{
			IntVector2 intVector = SuperReaperController.Instance.sprite.WorldCenter.ToIntVector2(VectorConversions.Floor);
			if (GameManager.Instance.Dungeon.data.CheckInBounds(intVector))
			{
				CellData cellData = GameManager.Instance.Dungeon.data[intVector];
				if (cellData != null && cellData.parentRoom == m_room)
				{
					PreventTeleportingPlayerAway = true;
					State = ShopState.TeleportAway;
				}
			}
		}
		if (baseShopType == AdditionalShopType.FOYER_META && m_itemControllers != null && IsBeetleMerchant && !BeetleExhausted)
		{
			bool flag = true;
			for (int i = 0; i < m_itemControllers.Count; i++)
			{
				if (!m_itemControllers[i].Acquired)
				{
					flag = false;
				}
			}
			if (flag)
			{
				m_shopkeep.ShopStockStatus = ((!m_onLastStockBeetle) ? Tribool.Ready : Tribool.Complete);
				if (m_onLastStockBeetle)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.BLUEPRINTBEETLE_GOLDIES, true);
				}
				BeetleExhausted = true;
				GameStatsManager.Instance.SetFlag(GungeonFlags.SHOP_BEETLE_ACTIVE, false);
				GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 0f;
				GameStatsManager.Instance.AccumulatedUsedBeetleMerchantChance = 0f;
			}
		}
		m_timeSinceLastHit += BraveTime.DeltaTime;
		if (State == ShopState.Default || State == ShopState.GunDrawn)
		{
			if (IsMainShopkeep)
			{
				for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
				{
					PlayerController playerController = GameManager.Instance.AllPlayers[j];
					if ((bool)playerController && playerController.healthHaver.IsAlive && playerController.CurrentRoom == m_room && playerController.IsFiring)
					{
						PlayerFired();
					}
				}
			}
		}
		else if (State == ShopState.PreTeleportAway)
		{
			if (m_shopkeep.IsTalking)
			{
				EndConversation.ForceEndConversation(m_shopkeep);
			}
			m_preTeleportTimer += BraveTime.DeltaTime;
			if (m_preTeleportTimer > 2f)
			{
				State = ShopState.TeleportAway;
			}
		}
		else if (State == ShopState.TeleportAway)
		{
			if (m_shopkeep.IsTalking)
			{
				EndConversation.ForceEndConversation(m_shopkeep);
			}
			if (!m_shopkeep.aiAnimator.IsPlaying("button"))
			{
				PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
				foreach (PlayerController playerController2 in allPlayers)
				{
					if ((bool)playerController2 && playerController2.CurrentRoom != null && playerController2.CurrentRoom != m_room && playerController2.CurrentRoom.IsSealed)
					{
						PreventTeleportingPlayerAway = true;
					}
				}
				if (!PreventTeleportingPlayerAway)
				{
					PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
					if (bestActivePlayer.CurrentRoom == m_room)
					{
						bestActivePlayer.EscapeRoom(PlayerController.EscapeSealedRoomStyle.TELEPORTER, false);
						AkSoundEngine.PostEvent("Play_OBJ_teleport_depart_01", bestActivePlayer.gameObject);
					}
					if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
					{
						PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(bestActivePlayer);
						if ((bool)otherPlayer && otherPlayer.healthHaver.IsAlive)
						{
							otherPlayer.ReuniteWithOtherPlayer(bestActivePlayer);
						}
					}
				}
				State = ShopState.Gone;
			}
		}
		else if (State == ShopState.Hostile)
		{
			bool flag2 = false;
			for (int l = 0; l < GameManager.Instance.AllPlayers.Length; l++)
			{
				PlayerController playerController3 = GameManager.Instance.AllPlayers[l];
				if ((bool)playerController3 && playerController3.healthHaver.IsAlive && playerController3.CurrentRoom == m_room)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				State = ShopState.Gone;
				return;
			}
			m_preTeleportTimer += BraveTime.DeltaTime;
			if (m_preTeleportTimer > 10f)
			{
				State = ShopState.TeleportAway;
			}
		}
		else if (State == ShopState.RefuseService)
		{
			bool flag3 = false;
			for (int m = 0; m < GameManager.Instance.AllPlayers.Length; m++)
			{
				PlayerController playerController4 = GameManager.Instance.AllPlayers[m];
				if ((bool)playerController4 && playerController4.healthHaver.IsAlive && playerController4.CurrentRoom == m_room)
				{
					flag3 = true;
				}
			}
			if (!flag3)
			{
				State = ShopState.Gone;
			}
		}
		if (m_capableOfBeingStolenFrom.UpdateTimers(BraveTime.DeltaTime))
		{
			for (int n = 0; n < GameManager.Instance.AllPlayers.Length; n++)
			{
				GameManager.Instance.AllPlayers[n].ForceRefreshInteractable = true;
			}
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)m_shopkeep && (bool)m_shopkeep.bulletBank)
		{
			AIBulletBank aIBulletBank = m_shopkeep.bulletBank;
			aIBulletBank.OnProjectileCreated = (Action<Projectile>)Delegate.Remove(aIBulletBank.OnProjectileCreated, new Action<Projectile>(OnProjectileCreated));
		}
		StaticReferenceManager.AllShops.Remove(this);
		base.OnDestroy();
	}

	public virtual void NotifyFailedPurchase(ShopItemController itemController)
	{
		if (shopkeepFSM != null)
		{
			FsmObject fsmObject = shopkeepFSM.FsmVariables.FindFsmObject("referencedItem");
			if (fsmObject != null)
			{
				fsmObject.Value = itemController;
			}
			shopkeepFSM.SendEvent("failedPurchase");
		}
	}

	public virtual void PurchaseItem(ShopItemController item, bool actualPurchase = true, bool allowSign = true)
	{
		float heightOffGround = -1f;
		if ((bool)item && (bool)item.sprite)
		{
			heightOffGround = item.sprite.HeightOffGround;
		}
		if (actualPurchase)
		{
			if (baseShopType == AdditionalShopType.TRUCK)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_PURCHASES_TRUCK, 1f);
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MONEY_SPENT_AT_TRUCK_SHOP, item.ModifiedPrice);
			}
			if (baseShopType == AdditionalShopType.GOOP)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_PURCHASES_GOOP, 1f);
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MONEY_SPENT_AT_GOOP_SHOP, item.ModifiedPrice);
			}
			if (baseShopType == AdditionalShopType.CURSE)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_PURCHASES_CURSE, 1f);
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MONEY_SPENT_AT_CURSE_SHOP, item.ModifiedPrice);
				StatModifier statModifier = new StatModifier();
				statModifier.amount = 2.5f;
				statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				statModifier.statToBoost = PlayerStats.StatType.Curse;
				item.LastInteractingPlayer.ownerlessStatModifiers.Add(statModifier);
				item.LastInteractingPlayer.stats.RecalculateStats(item.LastInteractingPlayer);
			}
			if (baseShopType == AdditionalShopType.BLANK)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_PURCHASES_BLANK, 1f);
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MONEY_SPENT_AT_BLANK_SHOP, item.ModifiedPrice);
			}
			if (baseShopType == AdditionalShopType.KEY)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_PURCHASES_KEY, 1f);
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MONEY_SPENT_AT_KEY_SHOP, item.ModifiedPrice);
			}
			if (shopkeepFSM != null && baseShopType != AdditionalShopType.RESRAT_SHORTCUT)
			{
				FsmObject fsmObject = shopkeepFSM.FsmVariables.FindFsmObject("referencedItem");
				if (fsmObject != null)
				{
					fsmObject.Value = item;
				}
				shopkeepFSM.SendEvent("succeedPurchase");
			}
		}
		if (!item.item.PersistsOnPurchase)
		{
			if (allowSign)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/Sign_SoldOut"));
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				component.PlaceAtPositionByAnchor(item.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
				gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
				component.HeightOffGround = heightOffGround;
				component.UpdateZDepth();
			}
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
			tk2dBaseSprite component2 = gameObject2.GetComponent<tk2dBaseSprite>();
			component2.PlaceAtPositionByAnchor(item.sprite.WorldCenter.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
			component2.transform.position = component2.transform.position.Quantize(0.0625f);
			component2.HeightOffGround = 5f;
			component2.UpdateZDepth();
			m_room.DeregisterInteractable(item);
			UnityEngine.Object.Destroy(item.gameObject);
		}
		if (baseShopType != AdditionalShopType.RESRAT_SHORTCUT)
		{
			return;
		}
		m_numberThingsPurchased++;
		int num = 1;
		switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
		{
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			num = 1;
			break;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			num = 1;
			break;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			num = 2;
			break;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			num = 3;
			break;
		default:
			num = 1;
			break;
		}
		if (m_numberThingsPurchased < num)
		{
			return;
		}
		for (int i = 0; i < m_itemControllers.Count; i++)
		{
			if (!m_itemControllers[i].Acquired)
			{
				m_itemControllers[i].ForceOutOfStock();
			}
		}
		if (shopkeepFSM != null)
		{
			FsmObject fsmObject2 = shopkeepFSM.FsmVariables.FindFsmObject("referencedItem");
			if (fsmObject2 != null)
			{
				fsmObject2.Value = item;
			}
			shopkeepFSM.SendEvent("succeedPurchase");
			m_shopkeep.IsInteractable = false;
		}
	}

	public void NotifyStealSucceeded()
	{
		ItemsStolen++;
		if (IsMainShopkeep)
		{
			StealChance = ((ItemsStolen > 1) ? 0.1f : 0.5f);
		}
		else
		{
			StealChance = 0.1f;
		}
	}

	public void NotifyStealFailed()
	{
		shopkeepFSM.SendEvent("caughtStealing");
		m_capableOfBeingStolenFrom.ClearOverrides();
		if ((bool)m_fakeEffectHandler)
		{
			m_fakeEffectHandler.RemoveAllEffects();
		}
		if (IsMainShopkeep)
		{
			State = ShopState.PreTeleportAway;
		}
		else
		{
			State = ShopState.RefuseService;
		}
		m_wasCaughtStealing = true;
	}

	public bool AttemptToSteal()
	{
		return UnityEngine.Random.value <= StealChance;
	}

	public virtual void ConfigureOnPlacement(RoomHandler room)
	{
		if (baseShopType != AdditionalShopType.RESRAT_SHORTCUT)
		{
			room.IsShop = true;
		}
		m_room = room;
		if (OptionalMinimapIcon != null)
		{
			Minimap.Instance.RegisterRoomIcon(m_room, OptionalMinimapIcon);
		}
		room.Entered += HandleEnter;
		bool isSeeded = GameManager.Instance.IsSeeded;
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON && (baseShopType == AdditionalShopType.BLANK || baseShopType == AdditionalShopType.CURSE || baseShopType == AdditionalShopType.GOOP || baseShopType == AdditionalShopType.TRUCK) && room.connectedRooms.Count != 1)
		{
		}
	}

	private PickupObject GetRandomLockedPaydayItem()
	{
		GenericLootTable itemsLootTable = GameManager.Instance.RewardManager.ItemsLootTable;
		List<PickupObject> list = new List<PickupObject>();
		for (int i = 0; i < itemsLootTable.defaultItemDrops.elements.Count; i++)
		{
			WeightedGameObject weightedGameObject = itemsLootTable.defaultItemDrops.elements[i];
			if (!weightedGameObject.gameObject)
			{
				continue;
			}
			PickupObject component = weightedGameObject.gameObject.GetComponent<PickupObject>();
			if ((bool)component && (component is BankMaskItem || component is BankBagItem || component is PaydayDrillItem))
			{
				EncounterTrackable encounterTrackable = component.encounterTrackable;
				if ((bool)encounterTrackable && !encounterTrackable.PrerequisitesMet())
				{
					list.Add(component);
				}
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private void HandleEnter(PlayerController p)
	{
		if (!m_hasBeenEntered && baseShopType == AdditionalShopType.NONE)
		{
			GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
			ReinitializeFirstItemToKey();
		}
		m_hasBeenEntered = true;
		if (FlagToSetOnEncounter != 0)
		{
			GameStatsManager.Instance.SetFlag(FlagToSetOnEncounter, true);
		}
	}

	private void OnProjectileCreated(Projectile projectile)
	{
		projectile.OwnerName = StringTableManager.GetEnemiesString("#JUSTICE_ENCNAME");
	}

	private void BeginState(ShopState state)
	{
		switch (state)
		{
		case ShopState.GunDrawn:
		{
			for (int j = 0; j < m_itemControllers.Count; j++)
			{
				if ((bool)m_itemControllers[j])
				{
					m_itemControllers[j].CurrentPrice *= 2;
				}
			}
			for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[k];
				if ((bool)playerController && playerController.healthHaver.IsAlive)
				{
					playerController.ForceRefreshInteractable = true;
				}
			}
			break;
		}
		case ShopState.Hostile:
		{
			shopkeepFSM.enabled = false;
			m_shopkeep.IsInteractable = false;
			LockItems();
			s_emptyFutureShops = true;
			m_shopkeep.behaviorSpeculator.enabled = true;
			AIBulletBank aIBulletBank = m_shopkeep.bulletBank;
			aIBulletBank.OnProjectileCreated = (Action<Projectile>)Delegate.Combine(aIBulletBank.OnProjectileCreated, new Action<Projectile>(OnProjectileCreated));
			break;
		}
		case ShopState.PreTeleportAway:
			m_preTeleportTimer = 0f;
			m_shopkeep.IsInteractable = false;
			LockItems();
			s_emptyFutureShops = true;
			break;
		case ShopState.TeleportAway:
			if (IsMainShopkeep)
			{
				shopkeepFSM.enabled = false;
				m_shopkeep.IsInteractable = false;
				m_shopkeep.behaviorSpeculator.InterruptAndDisable();
			}
			m_shopkeep.aiAnimator.PlayUntilCancelled("button");
			SpriteOutlineManager.RemoveOutlineFromSprite(m_shopkeep.sprite);
			m_shopkeep.sprite.HeightOffGround = 0f;
			m_shopkeep.sprite.UpdateZDepth();
			break;
		case ShopState.Gone:
		{
			if (IsMainShopkeep)
			{
				shopkeepFSM.enabled = false;
				m_shopkeep.IsInteractable = false;
				m_shopkeep.behaviorSpeculator.InterruptAndDisable();
				if (m_shopkeep.spriteAnimator.CurrentClip.name != "button_hit")
				{
					m_shopkeep.aiAnimator.PlayUntilCancelled("hide");
				}
				m_shopkeep.specRigidbody.enabled = false;
				UnityEngine.Object.Destroy(m_shopkeep.ultraFortunesFavor);
				m_shopkeep.RegenerateCache();
				if ((bool)cat)
				{
					tk2dBaseSprite component = cat.GetComponent<tk2dBaseSprite>();
					GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
					tk2dBaseSprite component2 = gameObject.GetComponent<tk2dBaseSprite>();
					component2.PlaceAtPositionByAnchor(component.WorldCenter.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
					component2.transform.position = component2.transform.position.Quantize(0.0625f);
					component2.HeightOffGround = 10f;
					component2.UpdateZDepth();
					cat.SetActive(false);
				}
			}
			for (int i = 0; i < m_itemControllers.Count; i++)
			{
				if (!m_itemControllers[i])
				{
					continue;
				}
				ShopItemController shopItemController = m_itemControllers[i];
				bool flag = false;
				if (shopItemController.item is BankMaskItem || shopItemController.item is BankBagItem || shopItemController.item is PaydayDrillItem)
				{
					EncounterTrackable encounterTrackable = shopItemController.item.encounterTrackable;
					if ((bool)encounterTrackable && !encounterTrackable.PrerequisitesMet())
					{
						flag = true;
						shopItemController.Locked = false;
						shopItemController.OverridePrice = 0;
						if (shopItemController.SetsFlagOnSteal)
						{
							GameStatsManager.Instance.SetFlag(shopItemController.FlagToSetOnSteal, true);
						}
					}
				}
				if (!flag)
				{
					m_itemControllers[i].ForceOutOfStock();
				}
			}
			break;
		}
		case ShopState.RefuseService:
			LockItems();
			m_shopkeep.SuppressRoomEnterExitEvents = true;
			break;
		}
	}

	private void EndState(ShopState state)
	{
	}

	private void PlayerFired()
	{
		if (!(m_timeSinceLastHit > 2f))
		{
			return;
		}
		m_hitCount++;
		m_timeSinceLastHit = 0f;
		if (m_state == ShopState.Default)
		{
			if (m_hitCount <= 1)
			{
				shopkeepFSM.SendEvent("betrayalWarning");
				return;
			}
			shopkeepFSM.SendEvent("betrayal");
			State = ShopState.GunDrawn;
		}
		else if (m_state == ShopState.GunDrawn)
		{
			State = ShopState.Hostile;
		}
	}

	public void ReinitializeFirstItemToKey()
	{
		if (baseShopType != 0)
		{
			return;
		}
		for (int i = 0; i < m_itemControllers.Count; i++)
		{
			if ((bool)m_itemControllers[i] && (bool)m_itemControllers[i].item && (bool)m_itemControllers[i].item.GetComponent<KeyBulletPickup>())
			{
				return;
			}
		}
		int num = UnityEngine.Random.Range(0, m_numberOfFirstTypeItems);
		if (num < 0)
		{
			num = 0;
		}
		if (num >= m_shopItems.Count || num >= m_itemControllers.Count || !m_shopItems[num] || !m_itemControllers[num])
		{
			num = 0;
		}
		if (!m_shopItems[num] || !m_itemControllers[num])
		{
			return;
		}
		GameObject gameObject = null;
		for (int j = 0; j < shopItems.defaultItemDrops.elements.Count; j++)
		{
			if ((bool)shopItems.defaultItemDrops.elements[j].gameObject && (bool)shopItems.defaultItemDrops.elements[j].gameObject.GetComponent<KeyBulletPickup>())
			{
				gameObject = shopItems.defaultItemDrops.elements[j].gameObject;
			}
		}
		m_shopItems[num] = gameObject;
		m_itemControllers[num].Initialize(gameObject.GetComponent<PickupObject>(), this);
	}

	protected virtual void DoSetup()
	{
		m_shopItems = new List<GameObject>();
		List<int> list = new List<int>();
		Func<GameObject, float, float> weightModifier = null;
		if (SecretHandshakeItem.NumActive > 0)
		{
			weightModifier = delegate(GameObject prefabObject, float sourceWeight)
			{
				PickupObject component10 = prefabObject.GetComponent<PickupObject>();
				float num7 = sourceWeight;
				if (component10 != null)
				{
					int quality = (int)component10.quality;
					num7 *= 1f + (float)quality / 10f;
				}
				return num7;
			};
		}
		System.Random safeRandom = null;
		if (baseShopType == AdditionalShopType.RESRAT_SHORTCUT)
		{
			if (GameStatsManager.Instance.CurrentResRatShopSeed < 0)
			{
				GameStatsManager.Instance.CurrentResRatShopSeed = UnityEngine.Random.Range(1, 1000000);
			}
			safeRandom = new System.Random(GameStatsManager.Instance.CurrentResRatShopSeed);
		}
		bool flag = GameStatsManager.Instance.IsRainbowRun && (baseShopType == AdditionalShopType.BLANK || baseShopType == AdditionalShopType.CURSE || baseShopType == AdditionalShopType.GOOP || baseShopType == AdditionalShopType.KEY || baseShopType == AdditionalShopType.TRUCK);
		for (int i = 0; i < spawnPositions.Length; i++)
		{
			if (flag)
			{
				m_shopItems.Add(null);
			}
			else if (baseShopType == AdditionalShopType.RESRAT_SHORTCUT)
			{
				GameObject shopItemResourcefulRatStyle = GameManager.Instance.RewardManager.GetShopItemResourcefulRatStyle(m_shopItems, safeRandom);
				m_shopItems.Add(shopItemResourcefulRatStyle);
			}
			else if (baseShopType == AdditionalShopType.FOYER_META && ExampleBlueprintPrefab != null)
			{
				if (FoyerMetaShopForcedTiers)
				{
					List<WeightedGameObject> compiledRawItems = shopItems.GetCompiledRawItems();
					int num = 0;
					bool flag2 = true;
					while (flag2)
					{
						for (int j = num; j < num + spawnPositions.Length; j++)
						{
							if (j >= compiledRawItems.Count)
							{
								flag2 = false;
								break;
							}
							GameObject gameObject = compiledRawItems[j].gameObject;
							PickupObject component = gameObject.GetComponent<PickupObject>();
							if (!component.encounterTrackable.PrerequisitesMet())
							{
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							num += spawnPositions.Length;
						}
					}
					if (num >= compiledRawItems.Count - spawnPositions.Length)
					{
						m_onLastStockBeetle = true;
					}
					for (int k = num; k < num + spawnPositions.Length; k++)
					{
						if (k >= compiledRawItems.Count)
						{
							m_shopItems.Add(null);
							list.Add(1);
							continue;
						}
						GameObject gameObject2 = compiledRawItems[k].gameObject;
						PickupObject component2 = gameObject2.GetComponent<PickupObject>();
						if (m_shopItems.Contains(gameObject2) || component2.encounterTrackable.PrerequisitesMet())
						{
							m_shopItems.Add(null);
							list.Add(1);
						}
						else
						{
							m_shopItems.Add(gameObject2);
							list.Add(Mathf.RoundToInt(compiledRawItems[k].weight));
						}
					}
					continue;
				}
				List<WeightedGameObject> compiledRawItems2 = shopItems.GetCompiledRawItems();
				GameObject gameObject3 = null;
				for (int l = 0; l < compiledRawItems2.Count; l++)
				{
					GameObject gameObject4 = compiledRawItems2[l].gameObject;
					PickupObject component3 = gameObject4.GetComponent<PickupObject>();
					if (!m_shopItems.Contains(gameObject4) && !component3.encounterTrackable.PrerequisitesMet())
					{
						gameObject3 = gameObject4;
						list.Add(Mathf.RoundToInt(compiledRawItems2[l].weight));
						break;
					}
				}
				m_shopItems.Add(gameObject3);
				if (gameObject3 == null)
				{
					list.Add(1);
				}
			}
			else
			{
				GameObject gameObject5 = shopItems.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded);
				m_shopItems.Add(gameObject5);
				if ((bool)gameObject5)
				{
					m_numberOfFirstTypeItems++;
				}
			}
		}
		m_itemControllers = new List<ShopItemController>();
		for (int m = 0; m < spawnPositions.Length; m++)
		{
			Transform transform = spawnPositions[m];
			if (flag || m_shopItems[m] == null)
			{
				continue;
			}
			PickupObject component4 = m_shopItems[m].GetComponent<PickupObject>();
			if (component4 == null)
			{
				continue;
			}
			GameObject gameObject6 = new GameObject("Shop item " + m);
			Transform transform2 = gameObject6.transform;
			transform2.parent = transform;
			transform2.localPosition = Vector3.zero;
			EncounterTrackable component5 = component4.GetComponent<EncounterTrackable>();
			if (component5 != null)
			{
				GameManager.Instance.ExtantShopTrackableGuids.Add(component5.EncounterGuid);
			}
			ShopItemController shopItemController = gameObject6.AddComponent<ShopItemController>();
			AssignItemFacing(transform, shopItemController);
			if (!m_room.IsRegistered(shopItemController))
			{
				m_room.RegisterInteractable(shopItemController);
			}
			if (baseShopType == AdditionalShopType.FOYER_META && ExampleBlueprintPrefab != null)
			{
				GameObject gameObject7 = UnityEngine.Object.Instantiate(ExampleBlueprintPrefab, new Vector3(150f, -50f, -100f), Quaternion.identity);
				ItemBlueprintItem component6 = gameObject7.GetComponent<ItemBlueprintItem>();
				EncounterTrackable component7 = gameObject7.GetComponent<EncounterTrackable>();
				component7.journalData.PrimaryDisplayName = component4.encounterTrackable.journalData.PrimaryDisplayName;
				component7.journalData.NotificationPanelDescription = component4.encounterTrackable.journalData.NotificationPanelDescription;
				component7.journalData.AmmonomiconFullEntry = component4.encounterTrackable.journalData.AmmonomiconFullEntry;
				component7.journalData.AmmonomiconSprite = component4.encounterTrackable.journalData.AmmonomiconSprite;
				component7.DoNotificationOnEncounter = false;
				component6.UsesCustomCost = true;
				component6.CustomCost = list[m];
				GungeonFlags saveFlagToSetOnAcquisition = GungeonFlags.NONE;
				for (int n = 0; n < component4.encounterTrackable.prerequisites.Length; n++)
				{
					if (component4.encounterTrackable.prerequisites[n].prerequisiteType == DungeonPrerequisite.PrerequisiteType.FLAG)
					{
						saveFlagToSetOnAcquisition = component4.encounterTrackable.prerequisites[n].saveFlagToCheck;
					}
				}
				component6.SaveFlagToSetOnAcquisition = saveFlagToSetOnAcquisition;
				component6.HologramIconSpriteName = component7.journalData.AmmonomiconSprite;
				shopItemController.Initialize(component6, this);
				gameObject7.SetActive(false);
			}
			else
			{
				shopItemController.Initialize(component4, this);
			}
			m_itemControllers.Add(shopItemController);
		}
		bool flag3 = false;
		if (shopItemsGroup2 != null && spawnPositionsGroup2.Length > 0)
		{
			int count = m_shopItems.Count;
			for (int num2 = 0; num2 < spawnPositionsGroup2.Length; num2++)
			{
				if (flag)
				{
					m_shopItems.Add(null);
					continue;
				}
				float num3 = spawnGroupTwoItem1Chance;
				switch (num2)
				{
				case 1:
					num3 = spawnGroupTwoItem2Chance;
					break;
				case 2:
					num3 = spawnGroupTwoItem3Chance;
					break;
				}
				bool isSeeded = GameManager.Instance.IsSeeded;
				if (((!isSeeded) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) < num3)
				{
					if (baseShopType == AdditionalShopType.BLACKSMITH)
					{
						if (!GameStatsManager.Instance.IsRainbowRun)
						{
							if (((!isSeeded) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) > 0.5f)
							{
								GameObject item = shopItemsGroup2.SelectByWeightWithoutDuplicatesFullPrereqs(m_shopItems, true, GameManager.Instance.IsSeeded);
								m_shopItems.Add(item);
							}
							else
							{
								GameObject rewardObjectShopStyle = GameManager.Instance.RewardManager.GetRewardObjectShopStyle(GameManager.Instance.PrimaryPlayer, true, false, m_shopItems);
								m_shopItems.Add(rewardObjectShopStyle);
							}
						}
						else
						{
							m_shopItems.Add(null);
						}
						continue;
					}
					float replaceFirstRewardWithPickup = GameManager.Instance.RewardManager.CurrentRewardData.ReplaceFirstRewardWithPickup;
					if (!flag3 && ((!isSeeded) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) < replaceFirstRewardWithPickup)
					{
						flag3 = true;
						GameObject item2 = shopItems.SelectByWeightWithoutDuplicatesFullPrereqs(m_shopItems, weightModifier, GameManager.Instance.IsSeeded);
						m_shopItems.Add(item2);
					}
					else if (!GameStatsManager.Instance.IsRainbowRun)
					{
						GameObject rewardObjectShopStyle2 = GameManager.Instance.RewardManager.GetRewardObjectShopStyle(GameManager.Instance.PrimaryPlayer, false, false, m_shopItems);
						m_shopItems.Add(rewardObjectShopStyle2);
					}
					else
					{
						m_shopItems.Add(null);
					}
				}
				else
				{
					m_shopItems.Add(null);
				}
			}
			bool flag4 = GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_BIGGEST_WALLET) || UnityEngine.Random.value < 0.05f;
			if (baseShopType == AdditionalShopType.NONE && flag4 && !flag)
			{
				PickupObject randomLockedPaydayItem = GetRandomLockedPaydayItem();
				if ((bool)randomLockedPaydayItem)
				{
					if (m_shopItems.Count - count < spawnPositionsGroup2.Length)
					{
						m_shopItems.Add(randomLockedPaydayItem.gameObject);
					}
					else
					{
						m_shopItems[UnityEngine.Random.Range(count, m_shopItems.Count)] = randomLockedPaydayItem.gameObject;
					}
				}
			}
			for (int num4 = 0; num4 < spawnPositionsGroup2.Length; num4++)
			{
				Transform transform3 = spawnPositionsGroup2[num4];
				if (flag || m_shopItems[count + num4] == null)
				{
					continue;
				}
				PickupObject component8 = m_shopItems[count + num4].GetComponent<PickupObject>();
				if (!(component8 == null))
				{
					GameObject gameObject8 = new GameObject("Shop 2 item " + num4);
					Transform transform4 = gameObject8.transform;
					transform4.parent = transform3;
					transform4.localPosition = Vector3.zero;
					EncounterTrackable component9 = component8.GetComponent<EncounterTrackable>();
					if (component9 != null)
					{
						GameManager.Instance.ExtantShopTrackableGuids.Add(component9.EncounterGuid);
					}
					ShopItemController shopItemController2 = gameObject8.AddComponent<ShopItemController>();
					AssignItemFacing(transform3, shopItemController2);
					if (!m_room.IsRegistered(shopItemController2))
					{
						m_room.RegisterInteractable(shopItemController2);
					}
					shopItemController2.Initialize(component8, this);
					m_itemControllers.Add(shopItemController2);
				}
			}
		}
		if (baseShopType == AdditionalShopType.NONE || baseShopType == AdditionalShopType.BLACKSMITH || baseShopType == AdditionalShopType.FOYER_META)
		{
			List<ShopSubsidiaryZone> componentsInRoom = m_room.GetComponentsInRoom<ShopSubsidiaryZone>();
			for (int num5 = 0; num5 < componentsInRoom.Count; num5++)
			{
				componentsInRoom[num5].HandleSetup(this, m_room, m_shopItems, m_itemControllers);
			}
		}
		for (int num6 = 0; num6 < m_itemControllers.Count; num6++)
		{
			if (baseShopType == AdditionalShopType.KEY)
			{
				m_itemControllers[num6].CurrencyType = ShopItemController.ShopCurrencyType.KEYS;
			}
			if (baseShopType == AdditionalShopType.FOYER_META)
			{
				m_itemControllers[num6].CurrencyType = ShopItemController.ShopCurrencyType.META_CURRENCY;
			}
		}
	}

	private void AssignItemFacing(Transform spawnTransform, ShopItemController itemController)
	{
		if (baseShopType == AdditionalShopType.FOYER_META)
		{
			itemController.UseOmnidirectionalItemFacing = true;
		}
		else if (spawnTransform.name.Contains("SIDE") || spawnTransform.name.Contains("EAST"))
		{
			itemController.itemFacing = DungeonData.Direction.EAST;
		}
		else if (spawnTransform.name.Contains("WEST"))
		{
			itemController.itemFacing = DungeonData.Direction.WEST;
		}
		else if (spawnTransform.name.Contains("NORTH"))
		{
			itemController.itemFacing = DungeonData.Direction.NORTH;
		}
	}

	private void LockItems()
	{
		for (int i = 0; i < m_itemControllers.Count; i++)
		{
			if ((bool)m_itemControllers[i])
			{
				m_itemControllers[i].Locked = true;
			}
		}
		for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[j];
			if ((bool)playerController && playerController.healthHaver.IsAlive)
			{
				playerController.ForceRefreshInteractable = true;
			}
		}
	}

	public static MidGameStaticShopData GetStaticShopDataForMidGameSave()
	{
		MidGameStaticShopData midGameStaticShopData = new MidGameStaticShopData();
		midGameStaticShopData.MainShopkeepStealChance = s_mainShopkeepStealChance;
		midGameStaticShopData.MainShopkeepItemsStolen = s_mainShopkeepItemsStolen;
		midGameStaticShopData.EmptyFutureShops = s_emptyFutureShops;
		midGameStaticShopData.HasDroppedSerJunkan = Chest.HasDroppedSerJunkanThisSession;
		return midGameStaticShopData;
	}

	public static void LoadFromMidGameSave(MidGameStaticShopData ssd)
	{
		s_mainShopkeepStealChance = ssd.MainShopkeepStealChance;
		s_mainShopkeepItemsStolen = ssd.MainShopkeepItemsStolen;
		s_emptyFutureShops = ssd.EmptyFutureShops;
		Chest.HasDroppedSerJunkanThisSession = ssd.HasDroppedSerJunkan;
	}

	public static void ClearStaticMemory()
	{
		s_mainShopkeepItemsStolen = 0;
		s_mainShopkeepStealChance = 1f;
		s_emptyFutureShops = false;
	}
}
