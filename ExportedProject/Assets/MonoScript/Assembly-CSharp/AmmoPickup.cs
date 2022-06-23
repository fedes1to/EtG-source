using Dungeonator;
using UnityEngine;

public class AmmoPickup : PickupObject, IPlayerInteractable
{
	public enum AmmoPickupMode
	{
		ONE_CLIP,
		FULL_AMMO,
		SPREAD_AMMO
	}

	public AmmoPickupMode mode = AmmoPickupMode.FULL_AMMO;

	public GameObject pickupVFX;

	public GameObject minimapIcon;

	public float SpreadAmmoCurrentGunPercent = 0.5f;

	public float SpreadAmmoOtherGunsPercent = 0.2f;

	[Header("Custom Ammo")]
	public bool AppliesCustomAmmunition;

	[ShowInInspectorIf("AppliesCustomAmmunition", false)]
	public float CustomAmmunitionDamageModifier = 1f;

	[ShowInInspectorIf("AppliesCustomAmmunition", false)]
	public float CustomAmmunitionSpeedModifier = 1f;

	[ShowInInspectorIf("AppliesCustomAmmunition", false)]
	public float CustomAmmunitionRangeModifier = 1f;

	private bool m_pickedUp;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	public bool pickedUp
	{
		get
		{
			return m_pickedUp;
		}
	}

	private void Start()
	{
		if (minimapIcon != null && !m_pickedUp)
		{
			m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, minimapIcon);
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
		if (AppliesCustomAmmunition)
		{
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/RainbowChestShader");
		}
	}

	private void Update()
	{
		if (!m_pickedUp && !m_isBeingEyedByRat && ShouldBeTakenByRat(base.sprite.WorldCenter))
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleRatTheft());
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
		if (Minimap.HasInstance)
		{
			GetRidOfMinimapIcon();
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (m_pickedUp)
		{
			return;
		}
		player.ResetTarnisherClipCapacity();
		if (player.CurrentGun == null || player.CurrentGun.ammo == player.CurrentGun.AdjustedMaxAmmo || !player.CurrentGun.CanGainAmmo)
		{
			return;
		}
		switch (mode)
		{
		case AmmoPickupMode.ONE_CLIP:
			player.CurrentGun.GainAmmo(player.CurrentGun.ClipCapacity);
			break;
		case AmmoPickupMode.FULL_AMMO:
			if (player.CurrentGun.AdjustedMaxAmmo > 0)
			{
				player.CurrentGun.GainAmmo(player.CurrentGun.AdjustedMaxAmmo);
				player.CurrentGun.ForceImmediateReload();
				string string3 = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
				string description = player.CurrentGun.GetComponent<EncounterTrackable>().journalData.GetPrimaryDisplayName() + " " + StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_BODY");
				tk2dBaseSprite tk2dBaseSprite3 = player.CurrentGun.GetSprite();
				if (!GameUIRoot.Instance.BossHealthBarVisible)
				{
					GameUIRoot.Instance.notificationController.DoCustomNotification(string3, description, tk2dBaseSprite3.Collection, tk2dBaseSprite3.spriteId);
				}
			}
			break;
		case AmmoPickupMode.SPREAD_AMMO:
		{
			player.CurrentGun.GainAmmo(Mathf.CeilToInt((float)player.CurrentGun.AdjustedMaxAmmo * SpreadAmmoCurrentGunPercent));
			for (int i = 0; i < player.inventory.AllGuns.Count; i++)
			{
				if ((bool)player.inventory.AllGuns[i] && player.CurrentGun != player.inventory.AllGuns[i])
				{
					player.inventory.AllGuns[i].GainAmmo(Mathf.FloorToInt((float)player.inventory.AllGuns[i].AdjustedMaxAmmo * SpreadAmmoOtherGunsPercent));
				}
			}
			player.CurrentGun.ForceImmediateReload();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
				if (!otherPlayer.IsGhost)
				{
					for (int j = 0; j < otherPlayer.inventory.AllGuns.Count; j++)
					{
						if ((bool)otherPlayer.inventory.AllGuns[j])
						{
							otherPlayer.inventory.AllGuns[j].GainAmmo(Mathf.FloorToInt((float)otherPlayer.inventory.AllGuns[j].AdjustedMaxAmmo * SpreadAmmoOtherGunsPercent));
						}
					}
					otherPlayer.CurrentGun.ForceImmediateReload();
				}
			}
			string @string = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
			string string2 = StringTableManager.GetString("#AMMO_SPREAD_REFILLED_BODY");
			tk2dBaseSprite tk2dBaseSprite2 = base.sprite;
			if (!GameUIRoot.Instance.BossHealthBarVisible)
			{
				GameUIRoot.Instance.notificationController.DoCustomNotification(@string, string2, tk2dBaseSprite2.Collection, tk2dBaseSprite2.spriteId);
			}
			break;
		}
		}
		m_pickedUp = true;
		m_isBeingEyedByRat = false;
		GetRidOfMinimapIcon();
		if (pickupVFX != null)
		{
			player.PlayEffectOnActor(pickupVFX, Vector3.zero);
		}
		Object.Destroy(base.gameObject);
		AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", base.gameObject);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!base.sprite)
		{
			return 1000f;
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2)) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && (interactor.CurrentRoom.IsRegistered(this) || RoomHandler.unassignedInteractableObjects.Contains(this)))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!this)
		{
			return;
		}
		if (interactor.CurrentGun == null || interactor.CurrentGun.ammo == interactor.CurrentGun.AdjustedMaxAmmo || interactor.CurrentGun.InfiniteAmmo || interactor.CurrentGun.RequiresFundsToShoot)
		{
			if (interactor.CurrentGun != null)
			{
				GameUIRoot.Instance.InformNeedsReload(interactor, new Vector3(interactor.specRigidbody.UnitCenter.x - interactor.transform.position.x, 1.25f, 0f), 1f, "#RELOAD_FULL");
			}
			return;
		}
		if (RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			RoomHandler.unassignedInteractableObjects.Remove(this);
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		Pickup(interactor);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}
}
