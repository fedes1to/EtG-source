using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PassiveItem : PickupObject, IPlayerInteractable
{
	public static Dictionary<PlayerController, Dictionary<Type, int>> ActiveFlagItems = new Dictionary<PlayerController, Dictionary<Type, int>>();

	private static GameObject m_defaultIcon;

	protected bool m_pickedUp;

	protected bool m_pickedUpThisRun;

	[NonSerialized]
	public bool suppressPickupVFX;

	[SerializeField]
	public StatModifier[] passiveStatModifiers;

	[SerializeField]
	public int ArmorToGainOnInitialPickup;

	public GameObject minimapIcon;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	protected PlayerController m_owner;

	public Action<PlayerController> OnPickedUp;

	public Action<PlayerController> OnDisabled;

	public bool PickedUp
	{
		get
		{
			return m_pickedUp;
		}
	}

	public PlayerController Owner
	{
		get
		{
			return m_owner;
		}
	}

	public static void IncrementFlag(PlayerController player, Type flagType)
	{
		if (!ActiveFlagItems.ContainsKey(player))
		{
			ActiveFlagItems.Add(player, new Dictionary<Type, int>());
		}
		if (!ActiveFlagItems[player].ContainsKey(flagType))
		{
			ActiveFlagItems[player].Add(flagType, 1);
		}
		else
		{
			ActiveFlagItems[player][flagType] = ActiveFlagItems[player][flagType] + 1;
		}
	}

	public static void DecrementFlag(PlayerController player, Type flagType)
	{
		if (ActiveFlagItems.ContainsKey(player) && ActiveFlagItems[player].ContainsKey(flagType))
		{
			ActiveFlagItems[player][flagType] = ActiveFlagItems[player][flagType] - 1;
			if (ActiveFlagItems[player][flagType] <= 0)
			{
				ActiveFlagItems[player].Remove(flagType);
			}
		}
	}

	public static bool IsFlagSetAtAll(Type flagType)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (IsFlagSetForCharacter(GameManager.Instance.AllPlayers[i], flagType))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsFlagSetForCharacter(PlayerController player, Type flagType)
	{
		if (ActiveFlagItems.ContainsKey(player) && ActiveFlagItems[player].ContainsKey(flagType))
		{
			return ActiveFlagItems[player][flagType] > 0;
		}
		return false;
	}

	private void Start()
	{
		if (!m_pickedUp)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
		}
		if (!m_pickedUp)
		{
			RegisterMinimapIcon();
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

	public virtual DebrisObject Drop(PlayerController player)
	{
		m_pickedUp = false;
		m_pickedUpThisRun = true;
		HasBeenStatProcessed = true;
		DisableEffect(player);
		m_owner = null;
		DebrisObject debrisObject = LootEngine.DropItemWithoutInstantiating(base.gameObject, player.LockedApproximateSpriteCenter, player.unadjustedAimPoint - player.LockedApproximateSpriteCenter, 4f);
		SpriteOutlineManager.AddOutlineToSprite(debrisObject.sprite, Color.black, 0.1f);
		RegisterMinimapIcon();
		return debrisObject;
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

	protected virtual void Update()
	{
		if (!m_pickedUp && m_owner == null)
		{
			HandlePickupCurseParticles();
			if (!m_isBeingEyedByRat && Time.frameCount % 51 == 0 && ShouldBeTakenByRat(base.sprite.WorldCenter))
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleRatTheft());
			}
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
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
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
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
		Pickup(interactor);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public override void Pickup(PlayerController player)
	{
		if (m_pickedUp)
		{
			return;
		}
		if (RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			RoomHandler.unassignedInteractableObjects.Remove(this);
		}
		if (!Dungeon.IsGenerating)
		{
			RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
			if (absoluteRoom.IsRegistered(this))
			{
				absoluteRoom.DeregisterInteractable(this);
			}
		}
		if (GameManager.Instance.InTutorial)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerAcquiredPassiveItem");
		}
		OnSharedPickup();
		GetRidOfMinimapIcon();
		m_isBeingEyedByRat = false;
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		m_pickedUp = true;
		m_owner = player;
		if (OnPickedUp != null)
		{
			OnPickedUp(m_owner);
		}
		if (ShouldBeDestroyedOnExistence())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!m_pickedUpThisRun)
		{
			HandleLootMods(player);
			HandleEncounterable(player);
			if (ArmorToGainOnInitialPickup > 0)
			{
				player.healthHaver.Armor += ArmorToGainOnInitialPickup;
			}
			if (ItemSpansBaseQualityTiers || ItemRespectsHeartMagnificence)
			{
				RewardManager.AdditionalHeartTierMagnificence += 1f;
			}
		}
		else if ((bool)base.encounterTrackable && base.encounterTrackable.m_doNotificationOnEncounter && !EncounterTrackable.SuppressNextNotification && !GameUIRoot.Instance.BossHealthBarVisible)
		{
			GameUIRoot.Instance.notificationController.DoNotification(base.encounterTrackable, true);
		}
		if (!m_pickedUpThisRun && player.characterIdentity == PlayableCharacters.Robot)
		{
			for (int i = 0; i < passiveStatModifiers.Length; i++)
			{
				if (passiveStatModifiers[i].statToBoost == PlayerStats.StatType.Health && passiveStatModifiers[i].amount > 0f)
				{
					int amountToDrop = Mathf.FloorToInt(passiveStatModifiers[i].amount * (float)UnityEngine.Random.Range(GameManager.Instance.RewardManager.RobotMinCurrencyPerHealthItem, GameManager.Instance.RewardManager.RobotMaxCurrencyPerHealthItem + 1));
					LootEngine.SpawnCurrency(player.CenterPosition, amountToDrop);
				}
			}
		}
		if (!suppressPickupVFX && !PileOfDarkSoulsPickup.IsPileOfDarkSoulsPickup)
		{
			GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Pickup");
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.PlaceAtPositionByAnchor(base.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			component.UpdateZDepth();
		}
		m_pickedUpThisRun = true;
		PlatformInterface.SetAlienFXColor(m_alienPickupColor, 1f);
		player.AcquirePassiveItem(this);
	}

	protected virtual void DisableEffect(PlayerController disablingPlayer)
	{
		if (OnDisabled != null)
		{
			OnDisabled(disablingPlayer);
		}
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		for (int i = 0; i < passiveStatModifiers.Length; i++)
		{
			if ((bool)m_owner && passiveStatModifiers[i].statToBoost == PlayerStats.StatType.AdditionalBlanksPerFloor)
			{
				m_owner.Blanks += Mathf.RoundToInt(passiveStatModifiers[i].amount);
			}
		}
	}

	protected override void OnDestroy()
	{
		if (Minimap.HasInstance)
		{
			GetRidOfMinimapIcon();
		}
		DisableEffect(m_owner);
		m_owner = null;
		base.OnDestroy();
	}
}
