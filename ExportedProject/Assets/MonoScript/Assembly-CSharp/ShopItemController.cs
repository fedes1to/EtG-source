using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ShopItemController : BraveBehaviour, IPlayerInteractable
{
	public enum ShopCurrencyType
	{
		COINS,
		META_CURRENCY,
		KEYS,
		BLANKS
	}

	public PickupObject item;

	public bool UseOmnidirectionalItemFacing;

	public DungeonData.Direction itemFacing = DungeonData.Direction.SOUTH;

	[NonSerialized]
	public PlayerController LastInteractingPlayer;

	public ShopCurrencyType CurrencyType;

	public bool PrecludeAllDiscounts;

	public int CurrentPrice = -1;

	[NonSerialized]
	public int? OverridePrice;

	[NonSerialized]
	public bool SetsFlagOnSteal;

	[NonSerialized]
	public GungeonFlags FlagToSetOnSteal;

	[NonSerialized]
	public bool IsResourcefulRatKey;

	private bool pickedUp;

	private ShopController m_parentShop;

	private BaseShopController m_baseParentShop;

	private float THRESHOLD_CUTOFF_PRIMARY = 3f;

	private float THRESHOLD_CUTOFF_SECONDARY = 2f;

	[NonSerialized]
	private GameObject m_shadowObject;

	public bool Locked { get; set; }

	public int ModifiedPrice
	{
		get
		{
			if ((bool)m_baseParentShop && m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.RESRAT_SHORTCUT)
			{
				return 0;
			}
			if (IsResourcefulRatKey)
			{
				int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY));
				int num2 = 1000 - num;
				if (num2 <= 0)
				{
					return CurrentPrice;
				}
				return num2;
			}
			if (CurrencyType == ShopCurrencyType.META_CURRENCY)
			{
				return CurrentPrice;
			}
			if (CurrencyType == ShopCurrencyType.KEYS)
			{
				return CurrentPrice;
			}
			if (OverridePrice.HasValue)
			{
				return OverridePrice.Value;
			}
			if (PrecludeAllDiscounts)
			{
				return CurrentPrice;
			}
			float num3 = GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.GlobalPriceMultiplier);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.SecondaryPlayer)
			{
				num3 *= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.GlobalPriceMultiplier);
			}
			GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
			float num4 = ((lastLoadedLevelDefinition == null) ? 1f : lastLoadedLevelDefinition.priceMultiplier);
			float num5 = 1f;
			if (m_baseParentShop != null && m_baseParentShop.ShopCostModifier != 1f)
			{
				num5 *= m_baseParentShop.ShopCostModifier;
			}
			if (m_baseParentShop.GetAbsoluteParentRoom().area.PrototypeRoomName.Contains("Black Market"))
			{
				num5 *= 0.5f;
			}
			return Mathf.RoundToInt((float)CurrentPrice * num3 * num4 * num5);
		}
	}

	public bool Acquired
	{
		get
		{
			return pickedUp;
		}
	}

	public void Initialize(PickupObject i, BaseShopController parent)
	{
		m_baseParentShop = parent;
		InitializeInternal(i);
		if (parent.baseShopType != 0)
		{
			base.sprite.depthUsesTrimmedBounds = true;
			base.sprite.HeightOffGround = -1.25f;
			base.sprite.UpdateZDepth();
		}
	}

	public void Initialize(PickupObject i, ShopController parent)
	{
		m_parentShop = parent;
		InitializeInternal(i);
	}

	private void InitializeInternal(PickupObject i)
	{
		item = i;
		if (i is SpecialKeyItem && (i as SpecialKeyItem).keyType == SpecialKeyItem.SpecialKeyType.RESOURCEFUL_RAT_LAIR)
		{
			IsResourcefulRatKey = true;
		}
		if ((bool)item && (bool)item.encounterTrackable)
		{
			GameStatsManager.Instance.SingleIncrementDifferentiator(item.encounterTrackable);
		}
		CurrentPrice = item.PurchasePrice;
		if (m_baseParentShop != null && m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.KEY)
		{
			CurrentPrice = 1;
			if (item.quality == PickupObject.ItemQuality.A)
			{
				CurrentPrice = 2;
			}
			if (item.quality == PickupObject.ItemQuality.S)
			{
				CurrentPrice = 3;
			}
		}
		if (m_baseParentShop != null && m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.NONE && (item is BankMaskItem || item is BankBagItem || item is PaydayDrillItem))
		{
			EncounterTrackable encounterTrackable = item.encounterTrackable;
			if ((bool)encounterTrackable && !encounterTrackable.PrerequisitesMet())
			{
				if (item is BankMaskItem)
				{
					SetsFlagOnSteal = true;
					FlagToSetOnSteal = GungeonFlags.ITEMSPECIFIC_STOLE_BANKMASK;
				}
				else if (item is BankBagItem)
				{
					SetsFlagOnSteal = true;
					FlagToSetOnSteal = GungeonFlags.ITEMSPECIFIC_STOLE_BANKBAG;
				}
				else if (item is PaydayDrillItem)
				{
					SetsFlagOnSteal = true;
					FlagToSetOnSteal = GungeonFlags.ITEMSPECIFIC_STOLE_DRILL;
				}
				OverridePrice = 9999;
			}
		}
		base.gameObject.AddComponent<tk2dSprite>();
		tk2dSprite tk2dSprite2 = i.GetComponent<tk2dSprite>();
		if (tk2dSprite2 == null)
		{
			tk2dSprite2 = i.GetComponentInChildren<tk2dSprite>();
		}
		base.sprite.SetSprite(tk2dSprite2.Collection, tk2dSprite2.spriteId);
		base.sprite.IsPerpendicular = true;
		if (UseOmnidirectionalItemFacing)
		{
			base.sprite.IsPerpendicular = false;
		}
		base.sprite.HeightOffGround = 1f;
		if (m_parentShop != null)
		{
			if (m_parentShop is MetaShopController)
			{
				UseOmnidirectionalItemFacing = true;
				base.sprite.IsPerpendicular = false;
			}
			base.sprite.HeightOffGround += m_parentShop.ItemHeightOffGroundModifier;
		}
		else if (m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.BLACKSMITH)
		{
			UseOmnidirectionalItemFacing = true;
		}
		else if (m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.TRUCK || m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.GOOP || m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.CURSE || m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.BLANK || m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.KEY || m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.RESRAT_SHORTCUT)
		{
			UseOmnidirectionalItemFacing = true;
		}
		base.sprite.PlaceAtPositionByAnchor(base.transform.parent.position, tk2dBaseSprite.Anchor.MiddleCenter);
		base.sprite.transform.position = base.sprite.transform.position.Quantize(0.0625f);
		DepthLookupManager.ProcessRenderer(base.sprite.renderer);
		tk2dSprite componentInParent = base.transform.parent.gameObject.GetComponentInParent<tk2dSprite>();
		if (componentInParent != null)
		{
			componentInParent.AttachRenderer(base.sprite);
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f, 0.05f);
		GameObject gameObject = null;
		if (m_parentShop != null && m_parentShop.shopItemShadowPrefab != null)
		{
			gameObject = m_parentShop.shopItemShadowPrefab;
		}
		if (m_baseParentShop != null && m_baseParentShop.shopItemShadowPrefab != null)
		{
			gameObject = m_baseParentShop.shopItemShadowPrefab;
		}
		if (gameObject != null)
		{
			if (!m_shadowObject)
			{
				m_shadowObject = UnityEngine.Object.Instantiate(gameObject);
			}
			tk2dBaseSprite component = m_shadowObject.GetComponent<tk2dBaseSprite>();
			component.PlaceAtPositionByAnchor(base.sprite.WorldBottomCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			component.transform.position = component.transform.position.Quantize(0.0625f);
			base.sprite.AttachRenderer(component);
			component.transform.parent = base.sprite.transform;
			component.HeightOffGround = -0.5f;
			if (m_parentShop is MetaShopController)
			{
				component.HeightOffGround = -0.0625f;
			}
		}
		base.sprite.UpdateZDepth();
		SpeculativeRigidbody orAddComponent = base.gameObject.GetOrAddComponent<SpeculativeRigidbody>();
		orAddComponent.PixelColliders = new List<PixelCollider>();
		PixelCollider pixelCollider = new PixelCollider();
		pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Circle;
		pixelCollider.CollisionLayer = CollisionLayer.HighObstacle;
		pixelCollider.ManualDiameter = 14;
		PixelCollider pixelCollider2 = pixelCollider;
		Vector2 vector = base.sprite.WorldCenter - base.transform.position.XY();
		pixelCollider2.ManualOffsetX = PhysicsEngine.UnitToPixel(vector.x) - 7;
		pixelCollider2.ManualOffsetY = PhysicsEngine.UnitToPixel(vector.y) - 7;
		orAddComponent.PixelColliders.Add(pixelCollider2);
		orAddComponent.Initialize();
		orAddComponent.OnPreRigidbodyCollision = null;
		orAddComponent.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(orAddComponent.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(ItemOnPreRigidbodyCollision));
		RegenerateCache();
		if (!GameManager.Instance.IsFoyer && item is Gun && GameManager.Instance.PrimaryPlayer.CharacterUsesRandomGuns)
		{
			ForceOutOfStock();
		}
	}

	private void ItemOnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!otherRigidbody || otherRigidbody.PrimaryPixelCollider == null || otherRigidbody.PrimaryPixelCollider.CollisionLayer != CollisionLayer.Projectile)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void Update()
	{
		if ((bool)m_baseParentShop && m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.CURSE && !pickedUp && (bool)base.sprite)
		{
			PickupObject.HandlePickupCurseParticles(base.sprite, 1f);
		}
	}

	protected override void OnDestroy()
	{
		if (m_parentShop != null && m_parentShop is MetaShopController)
		{
			MetaShopController metaShopController = m_parentShop as MetaShopController;
			if ((bool)metaShopController.Hologramophone && item is ItemBlueprintItem)
			{
				metaShopController.Hologramophone.HideSprite(base.gameObject);
			}
		}
		base.OnDestroy();
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (!this)
		{
			return;
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		Vector3 offset = new Vector3(base.sprite.GetBounds().max.x + 0.1875f, base.sprite.GetBounds().min.y, 0f);
		EncounterTrackable component = item.GetComponent<EncounterTrackable>();
		string arg = ((!(component != null)) ? item.DisplayName : component.journalData.GetPrimaryDisplayName());
		string text = ModifiedPrice.ToString();
		if (m_baseParentShop != null)
		{
			text = ((m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.FOYER_META) ? (text + "[sprite \"hbux_text_icon\"]") : ((m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.CURSE) ? (text + "[sprite \"ui_coin\"]?") : ((m_baseParentShop.baseShopType == BaseShopController.AdditionalShopType.RESRAT_SHORTCUT) ? "0[sprite \"ui_coin\"]?" : ((m_baseParentShop.baseShopType != BaseShopController.AdditionalShopType.KEY) ? (text + "[sprite \"ui_coin\"]") : (text + "[sprite \"ui_key\"]")))));
		}
		if (m_parentShop != null && m_parentShop is MetaShopController)
		{
			text += "[sprite \"hbux_text_icon\"]";
			MetaShopController metaShopController = m_parentShop as MetaShopController;
			if ((bool)metaShopController.Hologramophone && item is ItemBlueprintItem)
			{
				ItemBlueprintItem itemBlueprintItem = item as ItemBlueprintItem;
				tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
				metaShopController.Hologramophone.ChangeToSprite(base.gameObject, encounterIconCollection, encounterIconCollection.GetSpriteIdByName(itemBlueprintItem.HologramIconSpriteName));
			}
		}
		string text2 = (((!m_baseParentShop || !m_baseParentShop.IsCapableOfBeingStolenFrom) && !interactor.IsCapableOfStealing) ? string.Format("{0}: {1}", arg, text) : string.Format("[color red]{0}: {1} {2}[/color]", arg, text, StringTableManager.GetString("#STEAL")));
		if (IsResourcefulRatKey)
		{
			int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY));
			if (num < 1000)
			{
				int num2 = Mathf.Min(interactor.carriedConsumables.Currency, 1000 - num);
				if (num2 > 0)
				{
					text2 = text2 + "[color green] (-" + num2 + ")[/color]";
				}
			}
		}
		GameObject gameObject = GameUIRoot.Instance.RegisterDefaultLabel(base.transform, offset, text2);
		dfLabel componentInChildren = gameObject.GetComponentInChildren<dfLabel>();
		componentInChildren.ColorizeSymbols = false;
		componentInChildren.ProcessMarkup = true;
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (!this)
		{
			return;
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f, 0.05f);
		GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
		if (m_parentShop != null && m_parentShop is MetaShopController)
		{
			MetaShopController metaShopController = m_parentShop as MetaShopController;
			if ((bool)metaShopController.Hologramophone && item is ItemBlueprintItem)
			{
				metaShopController.Hologramophone.HideSprite(base.gameObject);
			}
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!this)
		{
			return 1000f;
		}
		if (Locked)
		{
			return 1000f;
		}
		if (UseOmnidirectionalItemFacing)
		{
			Bounds bounds = base.sprite.GetBounds();
			return BraveMathCollege.DistToRectangle(point, bounds.min + base.transform.position, bounds.size);
		}
		if (itemFacing == DungeonData.Direction.EAST)
		{
			Bounds bounds2 = base.sprite.GetBounds();
			bounds2.SetMinMax(bounds2.min + base.transform.position, bounds2.max + base.transform.position);
			Vector2 vector = bounds2.center.XY();
			float num = vector.x - point.x;
			float num2 = Mathf.Abs(point.y - vector.y);
			if (num > 0f)
			{
				return 1000f;
			}
			if (num < 0f - THRESHOLD_CUTOFF_PRIMARY)
			{
				return 1000f;
			}
			if (num2 > THRESHOLD_CUTOFF_SECONDARY)
			{
				return 1000f;
			}
			return num2;
		}
		if (itemFacing == DungeonData.Direction.NORTH)
		{
			Bounds bounds3 = base.sprite.GetBounds();
			bounds3.SetMinMax(bounds3.min + base.transform.position, bounds3.max + base.transform.position);
			Vector2 vector2 = bounds3.center.XY();
			float num3 = Mathf.Abs(point.x - vector2.x);
			float num4 = vector2.y - point.y;
			if (num4 > bounds3.extents.y)
			{
				return 1000f;
			}
			if (num4 < 0f - THRESHOLD_CUTOFF_PRIMARY)
			{
				return 1000f;
			}
			if (num3 > THRESHOLD_CUTOFF_SECONDARY)
			{
				return 1000f;
			}
			return num3;
		}
		if (itemFacing == DungeonData.Direction.WEST)
		{
			Bounds bounds4 = base.sprite.GetBounds();
			bounds4.SetMinMax(bounds4.min + base.transform.position, bounds4.max + base.transform.position);
			Vector2 vector3 = bounds4.center.XY();
			float num5 = vector3.x - point.x;
			float num6 = Mathf.Abs(point.y - vector3.y);
			if (num5 < 0f)
			{
				return 1000f;
			}
			if (num5 > THRESHOLD_CUTOFF_PRIMARY)
			{
				return 1000f;
			}
			if (num6 > THRESHOLD_CUTOFF_SECONDARY)
			{
				return 1000f;
			}
			return num6;
		}
		Bounds bounds5 = base.sprite.GetBounds();
		bounds5.SetMinMax(bounds5.min + base.transform.position, bounds5.max + base.transform.position);
		Vector2 vector4 = bounds5.center.XY();
		float num7 = Mathf.Abs(point.x - vector4.x);
		float num8 = vector4.y - point.y;
		if (num8 < bounds5.extents.y)
		{
			return 1000f;
		}
		if (num8 > THRESHOLD_CUTOFF_PRIMARY)
		{
			return 1000f;
		}
		if (num7 > THRESHOLD_CUTOFF_SECONDARY)
		{
			return 1000f;
		}
		return num7;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	private bool ShouldSteal(PlayerController player)
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			return false;
		}
		return m_baseParentShop.IsCapableOfBeingStolenFrom || player.IsCapableOfStealing;
	}

	public void Interact(PlayerController player)
	{
		if ((bool)item && item is HealthPickup)
		{
			if ((item as HealthPickup).healAmount > 0f && (item as HealthPickup).armorAmount <= 0 && player.healthHaver.GetCurrentHealthPercentage() >= 1f)
			{
				return;
			}
		}
		else if ((bool)item && item is AmmoPickup && (player.CurrentGun == null || player.CurrentGun.ammo == player.CurrentGun.AdjustedMaxAmmo || !player.CurrentGun.CanGainAmmo || player.CurrentGun.InfiniteAmmo))
		{
			GameUIRoot.Instance.InformNeedsReload(player, new Vector3(player.specRigidbody.UnitCenter.x - player.transform.position.x, 1.25f, 0f), 1f, "#RELOAD_FULL");
			return;
		}
		LastInteractingPlayer = player;
		if (CurrencyType == ShopCurrencyType.COINS || CurrencyType == ShopCurrencyType.BLANKS || CurrencyType == ShopCurrencyType.KEYS)
		{
			bool flag = false;
			bool flag2 = true;
			if (ShouldSteal(player))
			{
				flag = m_baseParentShop.AttemptToSteal();
				flag2 = false;
				if (!flag)
				{
					player.DidUnstealthyAction();
					m_baseParentShop.NotifyStealFailed();
					return;
				}
			}
			if (flag2)
			{
				bool flag3 = false;
				if (CurrencyType == ShopCurrencyType.COINS || CurrencyType == ShopCurrencyType.BLANKS)
				{
					flag3 = player.carriedConsumables.Currency >= ModifiedPrice;
				}
				else if (CurrencyType == ShopCurrencyType.KEYS)
				{
					flag3 = player.carriedConsumables.KeyBullets >= ModifiedPrice;
				}
				if (IsResourcefulRatKey)
				{
					if (!flag3)
					{
						int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY));
						if (num >= 1000)
						{
							AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
							if (m_parentShop != null)
							{
								m_parentShop.NotifyFailedPurchase(this);
							}
							if (m_baseParentShop != null)
							{
								m_baseParentShop.NotifyFailedPurchase(this);
							}
						}
						else if (player.carriedConsumables.Currency > 0)
						{
							GameStatsManager.Instance.RegisterStatChange(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY, player.carriedConsumables.Currency);
							player.carriedConsumables.Currency = 0;
							OnExitRange(player);
							OnEnteredRange(player);
						}
						else
						{
							AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
							if (m_parentShop != null)
							{
								m_parentShop.NotifyFailedPurchase(this);
							}
							if (m_baseParentShop != null)
							{
								m_baseParentShop.NotifyFailedPurchase(this);
							}
						}
						return;
					}
					player.carriedConsumables.Currency -= ModifiedPrice;
					GameStatsManager.Instance.RegisterStatChange(TrackedStats.AMOUNT_PAID_FOR_RAT_KEY, ModifiedPrice);
					flag2 = false;
				}
				else if (!flag3)
				{
					AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
					if (m_parentShop != null)
					{
						m_parentShop.NotifyFailedPurchase(this);
					}
					if (m_baseParentShop != null)
					{
						m_baseParentShop.NotifyFailedPurchase(this);
					}
					return;
				}
			}
			if (pickedUp)
			{
				return;
			}
			pickedUp = !item.PersistsOnPurchase;
			LootEngine.GivePrefabToPlayer(item.gameObject, player);
			if (flag2)
			{
				if (CurrencyType == ShopCurrencyType.COINS || CurrencyType == ShopCurrencyType.BLANKS)
				{
					player.carriedConsumables.Currency -= ModifiedPrice;
				}
				else if (CurrencyType == ShopCurrencyType.KEYS)
				{
					player.carriedConsumables.KeyBullets -= ModifiedPrice;
				}
			}
			if (m_parentShop != null)
			{
				m_parentShop.PurchaseItem(this, !flag);
			}
			if (m_baseParentShop != null)
			{
				m_baseParentShop.PurchaseItem(this, !flag);
			}
			if (flag)
			{
				StatModifier statModifier = new StatModifier();
				statModifier.statToBoost = PlayerStats.StatType.Curse;
				statModifier.amount = 1f;
				statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				player.ownerlessStatModifiers.Add(statModifier);
				player.stats.RecalculateStats(player);
				player.HandleItemStolen(this);
				m_baseParentShop.NotifyStealSucceeded();
				player.IsThief = true;
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_ITEMS_STOLEN, 1f);
				if (SetsFlagOnSteal)
				{
					GameStatsManager.Instance.SetFlag(FlagToSetOnSteal, true);
				}
			}
			else
			{
				if (CurrencyType == ShopCurrencyType.BLANKS)
				{
					player.Blanks++;
				}
				player.HandleItemPurchased(this);
			}
			if (!item.PersistsOnPurchase)
			{
				GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
			}
			AkSoundEngine.PostEvent("Play_OBJ_item_purchase_01", base.gameObject);
		}
		else
		{
			if (CurrencyType != ShopCurrencyType.META_CURRENCY)
			{
				return;
			}
			int num2 = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY));
			if (num2 < ModifiedPrice)
			{
				AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
				if (m_parentShop != null)
				{
					m_parentShop.NotifyFailedPurchase(this);
				}
				if (m_baseParentShop != null)
				{
					m_baseParentShop.NotifyFailedPurchase(this);
				}
			}
			else if (!pickedUp)
			{
				pickedUp = !item.PersistsOnPurchase;
				GameStatsManager.Instance.ClearStatValueGlobal(TrackedStats.META_CURRENCY);
				GameStatsManager.Instance.SetStat(TrackedStats.META_CURRENCY, num2 - ModifiedPrice);
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY_SPENT_AT_META_SHOP, ModifiedPrice);
				LootEngine.GivePrefabToPlayer(item.gameObject, player);
				if (m_parentShop != null)
				{
					m_parentShop.PurchaseItem(this);
				}
				if (m_baseParentShop != null)
				{
					m_baseParentShop.PurchaseItem(this);
				}
				player.HandleItemPurchased(this);
				if (!item.PersistsOnPurchase)
				{
					GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
				}
				AkSoundEngine.PostEvent("Play_OBJ_item_purchase_01", base.gameObject);
			}
		}
	}

	public void ForceSteal(PlayerController player)
	{
		pickedUp = true;
		LootEngine.GivePrefabToPlayer(item.gameObject, player);
		if (m_parentShop != null)
		{
			m_parentShop.PurchaseItem(this, false, false);
		}
		if (m_baseParentShop != null)
		{
			m_baseParentShop.PurchaseItem(this, false, false);
		}
		StatModifier statModifier = new StatModifier();
		statModifier.statToBoost = PlayerStats.StatType.Curse;
		statModifier.amount = 1f;
		statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
		player.ownerlessStatModifiers.Add(statModifier);
		player.stats.RecalculateStats(player);
		player.HandleItemStolen(this);
		m_baseParentShop.NotifyStealSucceeded();
		player.IsThief = true;
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.MERCHANT_ITEMS_STOLEN, 1f);
		if (!m_baseParentShop.AttemptToSteal())
		{
			player.DidUnstealthyAction();
			m_baseParentShop.NotifyStealFailed();
		}
		GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
		AkSoundEngine.PostEvent("Play_OBJ_item_purchase_01", base.gameObject);
	}

	public void ForceOutOfStock()
	{
		pickedUp = true;
		if (m_parentShop != null)
		{
			m_parentShop.PurchaseItem(this, false);
		}
		if (m_baseParentShop != null)
		{
			m_baseParentShop.PurchaseItem(this, false);
		}
		GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}
}
