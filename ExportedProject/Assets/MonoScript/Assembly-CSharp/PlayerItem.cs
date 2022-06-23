using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class PlayerItem : PickupObject, IPlayerInteractable
{
	public static bool AllowDamageCooldownOnActive;

	private static GameObject m_defaultIcon;

	public bool consumable = true;

	[ShowInInspectorIf("consumable", false)]
	public bool consumableOnCooldownUse;

	[ShowInInspectorIf("consumable", false)]
	public bool consumableOnActiveUse;

	[ShowInInspectorIf("consumable", false)]
	public bool consumableHandlesOwnDuration;

	[ShowInInspectorIf("consumableHandlesOwnDuration", false)]
	public float customDestroyTime = -1f;

	public int numberOfUses = 1;

	public bool UsesNumberOfUsesBeforeCooldown;

	public bool canStack = true;

	public int roomCooldown = 1;

	public float timeCooldown;

	public float damageCooldown;

	public bool usableDuringDodgeRoll;

	public string useAnimation;

	[NonSerialized]
	public bool ForceAsExtant;

	[NonSerialized]
	public bool PreventCooldownBar;

	public GameObject minimapIcon;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	public Action<PlayerItem> OnActivationStatusChanged;

	[NonSerialized]
	protected bool m_isCurrentlyActive;

	private bool m_isDestroyed;

	[NonSerialized]
	protected float m_activeElapsed;

	[NonSerialized]
	protected float m_activeDuration;

	[NonSerialized]
	protected int m_cachedNumberOfUses;

	[NonSerialized]
	protected bool m_pickedUp;

	[NonSerialized]
	protected bool m_pickedUpThisRun;

	private int remainingRoomCooldown;

	private float remainingTimeCooldown;

	private float remainingDamageCooldown;

	public string OnActivatedSprite = string.Empty;

	public string OnCooldownSprite = string.Empty;

	[SerializeField]
	public StatModifier[] passiveStatModifiers;

	private int m_baseSpriteID = -1;

	[NonSerialized]
	public PlayerController LastOwner;

	[NonSerialized]
	protected float m_adjustedTimeScale = 1f;

	public Action<PlayerController> OnPickedUp;

	public Action OnPreDropEvent;

	public bool IsCurrentlyActive
	{
		get
		{
			return m_isCurrentlyActive;
		}
		protected set
		{
			if (value != m_isCurrentlyActive)
			{
				m_isCurrentlyActive = value;
				if (OnActivationStatusChanged != null)
				{
					OnActivationStatusChanged(this);
				}
			}
		}
	}

	public bool PickedUp
	{
		get
		{
			return m_pickedUp;
		}
	}

	public int CurrentRoomCooldown
	{
		get
		{
			return remainingRoomCooldown;
		}
		set
		{
			remainingRoomCooldown = value;
		}
	}

	public float CurrentTimeCooldown
	{
		get
		{
			return remainingTimeCooldown;
		}
		set
		{
			remainingTimeCooldown = value;
		}
	}

	public float CurrentDamageCooldown
	{
		get
		{
			return remainingDamageCooldown;
		}
		set
		{
			remainingDamageCooldown = value;
		}
	}

	public bool IsActive
	{
		get
		{
			return IsCurrentlyActive;
		}
	}

	public bool IsOnCooldown
	{
		get
		{
			return remainingRoomCooldown > 0 || remainingTimeCooldown > 0f || remainingDamageCooldown > 0f;
		}
	}

	public float ActivePercentage
	{
		get
		{
			return Mathf.Clamp01(m_activeElapsed / m_activeDuration);
		}
	}

	public float CooldownPercentage
	{
		get
		{
			if (IsCurrentlyActive)
			{
				return ActivePercentage;
			}
			if (!IsOnCooldown)
			{
				return 0f;
			}
			if (remainingRoomCooldown > 0)
			{
				return (float)remainingRoomCooldown / (float)roomCooldown;
			}
			if (remainingDamageCooldown > 0f)
			{
				return remainingDamageCooldown / damageCooldown;
			}
			if (remainingTimeCooldown > 0f)
			{
				return remainingTimeCooldown / timeCooldown;
			}
			return 0f;
		}
	}

	protected virtual void Start()
	{
		m_baseSpriteID = base.sprite.spriteId;
		m_cachedNumberOfUses = numberOfUses;
		if (!m_pickedUp)
		{
			base.renderer.enabled = true;
			if (!(this is SilencerItem))
			{
				SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
			}
			RegisterMinimapIcon();
		}
	}

	public virtual void Update()
	{
		if (m_pickedUp)
		{
			if (LastOwner == null)
			{
				LastOwner = GetComponentInParent<PlayerController>();
			}
			if (remainingTimeCooldown > 0f && (AllowDamageCooldownOnActive || !IsCurrentlyActive))
			{
				remainingTimeCooldown = Mathf.Max(0f, remainingTimeCooldown - BraveTime.DeltaTime);
			}
			if (IsCurrentlyActive)
			{
				m_activeElapsed += BraveTime.DeltaTime * m_adjustedTimeScale;
				if (!string.IsNullOrEmpty(OnActivatedSprite))
				{
					base.sprite.SetSprite(OnActivatedSprite);
				}
			}
		}
		else
		{
			HandlePickupCurseParticles();
			if (!m_isBeingEyedByRat && Time.frameCount % 47 == 0 && ShouldBeTakenByRat(base.sprite.WorldCenter))
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleRatTheft());
			}
		}
	}

	public void RegisterMinimapIcon()
	{
		if (base.transform.position.y < -300f)
		{
			return;
		}
		if (minimapIcon == null)
		{
			if (m_defaultIcon == null)
			{
				m_defaultIcon = (GameObject)BraveResources.Load("Global Prefabs/Minimap_Item_Icon");
			}
			minimapIcon = m_defaultIcon;
		}
		if (minimapIcon != null && !m_pickedUp)
		{
			m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, minimapIcon);
		}
	}

	public void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_minimapIconRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	protected bool UseConsumableStack()
	{
		numberOfUses--;
		if (numberOfUses <= 0)
		{
			m_isDestroyed = true;
			return true;
		}
		return false;
	}

	public virtual bool CanBeUsed(PlayerController user)
	{
		return true;
	}

	public void ResetSprite()
	{
		if ((!string.IsNullOrEmpty(OnCooldownSprite) || !string.IsNullOrEmpty(OnActivatedSprite)) && base.sprite.spriteId != m_baseSpriteID)
		{
			base.sprite.SetSprite(m_baseSpriteID);
		}
	}

	public bool Use(PlayerController user, out float destroyTime)
	{
		destroyTime = -1f;
		if (m_isDestroyed)
		{
			return false;
		}
		if (!CanBeUsed(user))
		{
			return false;
		}
		if (IsCurrentlyActive)
		{
			DoActiveEffect(user);
			if (consumable && consumableOnActiveUse && UseConsumableStack())
			{
				return true;
			}
			if (!string.IsNullOrEmpty(OnActivatedSprite) && base.sprite.spriteId != m_baseSpriteID)
			{
				base.sprite.SetSprite(m_baseSpriteID);
			}
			return false;
		}
		if (IsOnCooldown)
		{
			DoOnCooldownEffect(user);
			if (consumable && consumableOnCooldownUse && UseConsumableStack())
			{
				return true;
			}
			if (!string.IsNullOrEmpty(OnCooldownSprite) && base.sprite.spriteId != m_baseSpriteID)
			{
				base.sprite.SetSprite(m_baseSpriteID);
			}
			return false;
		}
		DoEffect(user);
		if (!string.IsNullOrEmpty(useAnimation))
		{
			tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(useAnimation);
			base.spriteAnimator.Play(clipByName);
			destroyTime = (float)clipByName.frames.Length / clipByName.fps;
		}
		if (consumable && !consumableOnCooldownUse && !consumableOnActiveUse)
		{
			bool flag = UseConsumableStack();
			if (consumableHandlesOwnDuration)
			{
				destroyTime = customDestroyTime;
			}
			if (flag)
			{
				return true;
			}
		}
		else if (UsesNumberOfUsesBeforeCooldown)
		{
			numberOfUses--;
		}
		if (destroyTime >= 0f)
		{
			StartCoroutine(HandleAnimationReset(destroyTime));
		}
		if (!UsesNumberOfUsesBeforeCooldown || numberOfUses <= 0)
		{
			if (UsesNumberOfUsesBeforeCooldown)
			{
				numberOfUses = m_cachedNumberOfUses;
			}
			ApplyCooldown(user);
			AfterCooldownApplied(user);
		}
		return false;
	}

	public void ForceApplyCooldown(PlayerController user)
	{
		ApplyCooldown(user);
		AfterCooldownApplied(user);
	}

	protected void ApplyCooldown(PlayerController user)
	{
		float num = 1f;
		if (user != null)
		{
			float num2 = user.stats.GetStatValue(PlayerStats.StatType.Coolness) * 0.05f;
			if (PassiveItem.IsFlagSetForCharacter(user, typeof(ChamberOfEvilItem)))
			{
				float num3 = user.stats.GetStatValue(PlayerStats.StatType.Curse) * 0.05f;
				num2 += num3;
			}
			num2 = Mathf.Clamp(num2, 0f, 0.5f);
			num = Mathf.Max(0f, num - num2);
		}
		remainingRoomCooldown += roomCooldown;
		remainingTimeCooldown += timeCooldown * num;
		remainingDamageCooldown += damageCooldown * num;
		if (!string.IsNullOrEmpty(OnCooldownSprite))
		{
			base.sprite.SetSprite(OnCooldownSprite);
		}
	}

	protected void ApplyAdditionalTimeCooldown(float addTimeCooldown)
	{
		remainingTimeCooldown += addTimeCooldown;
	}

	protected void ApplyAdditionalDamageCooldown(float addDamageCooldown)
	{
		remainingDamageCooldown += addDamageCooldown;
	}

	private IEnumerator HandleAnimationReset(float delay)
	{
		yield return new WaitForSeconds(delay);
		base.spriteAnimator.StopAndResetFrame();
	}

	public void ClearCooldowns()
	{
		remainingRoomCooldown = 0;
		remainingDamageCooldown = 0f;
		remainingTimeCooldown = 0f;
	}

	public void DidDamage(PlayerController Owner, float damageDone)
	{
		if (!IsActive || AllowDamageCooldownOnActive)
		{
			float num = 1f;
			GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
			if (lastLoadedLevelDefinition != null)
			{
				num /= lastLoadedLevelDefinition.enemyHealthMultiplier;
			}
			damageDone *= num;
			remainingDamageCooldown = Mathf.Max(0f, remainingDamageCooldown - damageDone);
		}
	}

	public void ClearedRoom()
	{
		if (remainingRoomCooldown > 0)
		{
			remainingRoomCooldown--;
		}
	}

	public virtual void OnItemSwitched(PlayerController user)
	{
	}

	protected virtual void DoEffect(PlayerController user)
	{
	}

	protected virtual void AfterCooldownApplied(PlayerController user)
	{
	}

	protected virtual void DoActiveEffect(PlayerController user)
	{
	}

	protected virtual void DoOnCooldownEffect(PlayerController user)
	{
	}

	protected virtual void OnPreDrop(PlayerController user)
	{
	}

	public DebrisObject Drop(PlayerController player, float overrideForce = 4f)
	{
		OnPreDrop(player);
		if (OnPreDropEvent != null)
		{
			OnPreDropEvent();
		}
		Vector2 spawnDirection = player.unadjustedAimPoint - player.sprite.WorldCenter.ToVector3ZUp();
		if (player.CurrentRoom != null && player.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SPECIAL)
		{
			overrideForce = 2f;
			spawnDirection = Vector2.down;
		}
		Vector3 spawnPosition = player.sprite.WorldCenter.ToVector3ZUp();
		if (this is RobotUnlockTelevisionItem)
		{
			spawnPosition += new Vector3(0f, -0.875f, 0f);
		}
		DebrisObject debrisObject = LootEngine.SpawnItem(base.gameObject, spawnPosition, spawnDirection, overrideForce);
		PlayerItem component = debrisObject.GetComponent<PlayerItem>();
		component.m_baseSpriteID = m_baseSpriteID;
		component.m_pickedUp = false;
		component.m_pickedUpThisRun = true;
		component.HasBeenStatProcessed = true;
		component.HasProcessedStatMods = HasProcessedStatMods;
		component.remainingDamageCooldown = remainingDamageCooldown;
		component.remainingRoomCooldown = remainingRoomCooldown;
		component.remainingTimeCooldown = remainingTimeCooldown;
		component.ResetSprite();
		component.CopyStateFrom(this);
		player.stats.RecalculateStats(player);
		return debrisObject;
	}

	protected virtual void CopyStateFrom(PlayerItem other)
	{
	}

	public override void Pickup(PlayerController player)
	{
		if (m_pickedUp)
		{
			return;
		}
		if (GameManager.Instance.InTutorial)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerAcquiredPlayerItem");
		}
		if (RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			RoomHandler.unassignedInteractableObjects.Remove(this);
		}
		OnSharedPickup();
		GetRidOfMinimapIcon();
		if (ShouldBeDestroyedOnExistence())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!PileOfDarkSoulsPickup.IsPileOfDarkSoulsPickup)
		{
			AkSoundEngine.PostEvent("Play_OBJ_item_pickup_01", base.gameObject);
			GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Pickup");
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.PlaceAtPositionByAnchor(base.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			component.UpdateZDepth();
		}
		if (!m_pickedUpThisRun)
		{
			HandleLootMods(player);
			HandleEncounterable(player);
		}
		else if ((bool)base.encounterTrackable && base.encounterTrackable.m_doNotificationOnEncounter && !EncounterTrackable.SuppressNextNotification && !GameUIRoot.Instance.BossHealthBarVisible)
		{
			GameUIRoot.Instance.notificationController.DoNotification(base.encounterTrackable, true);
		}
		LastOwner = player;
		m_isBeingEyedByRat = false;
		DebrisObject component2 = GetComponent<DebrisObject>();
		if (component2 != null || ForceAsExtant)
		{
			if ((bool)component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			m_pickedUp = true;
			m_pickedUpThisRun = true;
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			base.renderer.enabled = false;
			SquishyBounceWiggler component3 = GetComponent<SquishyBounceWiggler>();
			if (component3 != null)
			{
				UnityEngine.Object.Destroy(component3);
				base.sprite.ForceBuild();
			}
			player.GetEquippedWith(this);
		}
		else
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(base.gameObject);
			PlayerItem component4 = gameObject2.GetComponent<PlayerItem>();
			gameObject2.GetComponent<Renderer>().enabled = false;
			gameObject2.transform.position = player.transform.position;
			component4.m_pickedUp = true;
			component4.m_pickedUpThisRun = true;
			player.GetEquippedWith(component4);
		}
		if (OnPickedUp != null)
		{
			OnPickedUp(player);
		}
		PlatformInterface.SetAlienFXColor(m_alienPickupColor, 1f);
		player.DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (IsBeingSold)
		{
			return 1000f;
		}
		if (!base.sprite)
		{
			return 1000f;
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			if (!(this is SilencerItem))
			{
				SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			}
			base.sprite.UpdateZDepth();
			SquishyBounceWiggler component = GetComponent<SquishyBounceWiggler>();
			if (component != null)
			{
				component.WiggleHold = true;
			}
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (!this)
		{
			return;
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		if (!m_pickedUp)
		{
			if (!(this is SilencerItem))
			{
				SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
			}
			base.sprite.UpdateZDepth();
			SquishyBounceWiggler component = GetComponent<SquishyBounceWiggler>();
			if (component != null)
			{
				component.WiggleHold = false;
			}
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (GameStatsManager.HasInstance && GameStatsManager.Instance.IsRainbowRun)
		{
			if ((bool)interactor && interactor.CurrentRoom != null && interactor.CurrentRoom == GameManager.Instance.Dungeon.data.Entrance && Time.frameCount == PickupObject.s_lastRainbowPickupFrame)
			{
				return;
			}
			PickupObject.s_lastRainbowPickupFrame = Time.frameCount;
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		if (GameManager.Instance.InTutorial)
		{
			EncounterTrackable.SuppressNextNotification = true;
		}
		Pickup(interactor);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Minimap.HasInstance)
		{
			GetRidOfMinimapIcon();
		}
	}
}
