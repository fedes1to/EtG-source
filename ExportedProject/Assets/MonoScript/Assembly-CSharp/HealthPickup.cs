using System;
using Dungeonator;
using UnityEngine;

public class HealthPickup : PickupObject, IPlayerInteractable
{
	public string pickupName;

	public float healAmount = 1f;

	public int armorAmount;

	public GameObject healVFX;

	public GameObject armorVFX;

	public GameObject minimapIcon;

	private bool m_pickedUp;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	private bool m_placedInWorld;

	private void Awake()
	{
		if (Dungeon.IsGenerating)
		{
			m_placedInWorld = true;
		}
	}

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(TriggerWasEntered));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTrigger));
		if (minimapIcon != null && !m_pickedUp)
		{
			m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, minimapIcon);
		}
	}

	private void TriggerWasEntered(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody, CollisionData collisionData)
	{
		if (!m_pickedUp)
		{
			if (otherRigidbody.GetComponent<PlayerController>() != null)
			{
				PrePickupLogic(otherRigidbody, selfRigidbody);
			}
			else if (otherRigidbody.GetComponent<PickupObject>() != null && (bool)base.debris)
			{
				base.debris.ApplyVelocity((selfRigidbody.UnitCenter - otherRigidbody.UnitCenter).normalized);
				selfRigidbody.RegisterGhostCollisionException(otherRigidbody);
			}
		}
	}

	private void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_minimapIconRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (GameUIRoot.HasInstance)
		{
			ToggleLabel(false);
		}
		if (Minimap.HasInstance)
		{
			GetRidOfMinimapIcon();
		}
	}

	public void OnTrigger(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody, CollisionData collisionData)
	{
		if (!m_pickedUp && otherRigidbody.GetComponent<PlayerController>() != null)
		{
			PrePickupLogic(otherRigidbody, selfRigidbody);
		}
	}

	private void PrePickupLogic(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody)
	{
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if (component.IsGhost)
		{
			return;
		}
		HealthHaver healthHaver = otherRigidbody.healthHaver;
		if (component.HealthAndArmorSwapped)
		{
			if (healthHaver.GetCurrentHealth() == healthHaver.GetMaxHealth() && armorAmount > 0)
			{
				if ((bool)base.debris)
				{
					base.debris.ApplyVelocity(otherRigidbody.Velocity / 4f);
					selfRigidbody.RegisterTemporaryCollisionException(otherRigidbody, 0.25f);
				}
			}
			else
			{
				Pickup(component);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else if (healthHaver.GetCurrentHealth() == healthHaver.GetMaxHealth() && armorAmount == 0)
		{
			if (component.HasActiveBonusSynergy(CustomSynergyType.COIN_KING_OF_HEARTS))
			{
				m_pickedUp = true;
				AkSoundEngine.PostEvent("Play_OBJ_coin_medium_01", base.gameObject);
				int amountToDrop = ((!(healAmount < 1f)) ? UnityEngine.Random.Range(5, 12) : UnityEngine.Random.Range(3, 7));
				LootEngine.SpawnCurrency((!base.sprite) ? component.CenterPosition : base.sprite.WorldCenter, amountToDrop);
				GetRidOfMinimapIcon();
				ToggleLabel(false);
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if ((bool)base.debris)
			{
				base.debris.ApplyVelocity(otherRigidbody.Velocity / 4f);
				selfRigidbody.RegisterTemporaryCollisionException(otherRigidbody, 0.25f);
			}
		}
		else
		{
			Pickup(healthHaver.GetComponent<PlayerController>());
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public virtual void Update()
	{
		if (armorAmount > 0 && healAmount <= 0f && !m_pickedUp && !m_isBeingEyedByRat && Time.frameCount % 47 == 0 && !m_placedInWorld && ShouldBeTakenByRat(base.sprite.WorldCenter))
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleRatTheft());
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (player.IsGhost)
		{
			return;
		}
		HandleEncounterable(player);
		GetRidOfMinimapIcon();
		ToggleLabel(false);
		m_pickedUp = true;
		AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
		if (armorAmount > 0 && healAmount > 0f)
		{
			bool flag = player.healthHaver.GetCurrentHealth() != player.healthHaver.GetMaxHealth();
			if (player.HealthAndArmorSwapped)
			{
				player.healthHaver.Armor += Mathf.CeilToInt(healAmount);
				player.healthHaver.ApplyHealing(armorAmount);
			}
			else
			{
				player.healthHaver.ApplyHealing(healAmount);
				player.healthHaver.Armor += armorAmount;
			}
			if (flag && healVFX != null)
			{
				player.PlayEffectOnActor(healVFX, Vector3.zero);
			}
			else if (armorVFX != null)
			{
				player.PlayEffectOnActor(armorVFX, Vector3.zero);
			}
			else if (healVFX != null)
			{
				player.PlayEffectOnActor(healVFX, Vector3.zero);
			}
		}
		else if (armorAmount > 0)
		{
			if (armorVFX != null)
			{
				player.PlayEffectOnActor(armorVFX, Vector3.zero);
			}
			if (player.HealthAndArmorSwapped)
			{
				player.healthHaver.ApplyHealing(armorAmount);
			}
			else
			{
				player.healthHaver.Armor += armorAmount;
			}
		}
		else
		{
			if (healVFX != null)
			{
				player.PlayEffectOnActor(healVFX, Vector3.zero);
			}
			if (player.HealthAndArmorSwapped)
			{
				player.healthHaver.Armor += Mathf.CeilToInt(healAmount);
			}
			else
			{
				player.healthHaver.ApplyHealing(healAmount);
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (IsBeingSold || m_pickedUp)
		{
			return 1000f;
		}
		if (!base.sprite)
		{
			return 1000f;
		}
		if (armorAmount > 0)
		{
			return 1000f;
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public void ToggleLabel(bool enabledValue)
	{
		if (enabledValue)
		{
			GameObject gameObject = GameUIRoot.Instance.RegisterDefaultLabel(base.transform, new Vector3(1f, 0.1875f, 0f), StringTableManager.GetString("#SAVE_FOR_LATER"));
			dfLabel componentInChildren = gameObject.GetComponentInChildren<dfLabel>();
			componentInChildren.ColorizeSymbols = false;
			componentInChildren.ProcessMarkup = true;
		}
		else if (!GameManager.Instance.IsLoadingLevel && (bool)GameUIRoot.Instance)
		{
			GameUIRoot.Instance.DeregisterDefaultLabel(base.transform);
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && !m_pickedUp && armorAmount <= 0 && HeartDispenser.DispenserOnFloor && RoomHandler.unassignedInteractableObjects.Contains(this) && interactor.healthHaver.GetCurrentHealthPercentage() >= 1f)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
			ToggleLabel(true);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this && armorAmount <= 0 && HeartDispenser.DispenserOnFloor)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			if (!m_pickedUp)
			{
				base.sprite.UpdateZDepth();
				ToggleLabel(false);
			}
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_pickedUp && HeartDispenser.DispenserOnFloor && armorAmount <= 0 && interactor.healthHaver.GetCurrentHealthPercentage() >= 1f)
		{
			ToggleLabel(false);
			base.spriteAnimator.PlayAndDestroyObject((!(healAmount > 0.5f)) ? "heart_small_teleport" : "heart_big_teleport");
			if (healAmount > 0.5f)
			{
				HeartDispenser.CurrentHalfHeartsStored += 2;
			}
			else
			{
				HeartDispenser.CurrentHalfHeartsStored++;
			}
			m_pickedUp = true;
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	private RoomHandler FindShop()
	{
		RoomHandler roomHandler = null;
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			RoomHandler roomHandler2 = GameManager.Instance.Dungeon.data.rooms[i];
			if (!roomHandler2.IsShop)
			{
				continue;
			}
			BaseShopController[] componentsInChildren = roomHandler2.hierarchyParent.GetComponentsInChildren<BaseShopController>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if ((bool)componentsInChildren[j] && componentsInChildren[j].baseShopType == BaseShopController.AdditionalShopType.NONE)
				{
					roomHandler = roomHandler2;
					break;
				}
			}
			if (roomHandler != null)
			{
				break;
			}
		}
		return roomHandler;
	}
}
