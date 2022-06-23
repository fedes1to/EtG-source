using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PileOfDarkSoulsPickup : PickupObject, IPlayerInteractable
{
	[NonSerialized]
	public List<PlayerItem> activeItems = new List<PlayerItem>();

	[NonSerialized]
	public List<PassiveItem> passiveItems = new List<PassiveItem>();

	[NonSerialized]
	public List<Gun> guns = new List<Gun>();

	[NonSerialized]
	public List<PickupObject> additionalItems = new List<PickupObject>();

	[NonSerialized]
	public int TargetPlayerID = -1;

	public int containedCurrency;

	public GameObject pickupVFX;

	public GameObject minimapIcon;

	private bool m_pickedUp;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	public static bool IsPileOfDarkSoulsPickup;

	private void Start()
	{
		if (minimapIcon != null && !m_pickedUp)
		{
			m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, minimapIcon);
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
	}

	public void ToggleItems(bool val)
	{
		for (int i = 0; i < guns.Count; i++)
		{
			guns[i].gameObject.SetActive(val);
			guns[i].GetRidOfMinimapIcon();
		}
		for (int j = 0; j < activeItems.Count; j++)
		{
			activeItems[j].gameObject.SetActive(val);
			activeItems[j].GetRidOfMinimapIcon();
		}
		for (int k = 0; k < passiveItems.Count; k++)
		{
			passiveItems[k].gameObject.SetActive(val);
			passiveItems[k].GetRidOfMinimapIcon();
		}
		for (int l = 0; l < additionalItems.Count; l++)
		{
			additionalItems[l].gameObject.SetActive(val);
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
		ToggleItems(true);
		player.HandleDarkSoulsHollowTransition(false);
		m_pickedUp = true;
		player.healthHaver.CursedMaximum = float.MaxValue;
		float currentHealth = player.healthHaver.GetCurrentHealth();
		player.carriedConsumables.Currency += containedCurrency;
		EncounterTrackable.SuppressNextNotification = true;
		IsPileOfDarkSoulsPickup = true;
		bool flag = false;
		for (int i = 0; i < passiveItems.Count; i++)
		{
			EncounterTrackable.SuppressNextNotification = true;
			passiveItems[i].Pickup(player);
			if (passiveItems[i] is ExtraLifeItem && !flag)
			{
				ExtraLifeItem extraLifeItem = passiveItems[i] as ExtraLifeItem;
				if (extraLifeItem.extraLifeMode == ExtraLifeItem.ExtraLifeMode.DARK_SOULS && (bool)extraLifeItem.encounterTrackable)
				{
					flag = true;
					EncounterTrackable.SuppressNextNotification = false;
					GameUIRoot.Instance.notificationController.DoNotification(extraLifeItem.encounterTrackable);
					EncounterTrackable.SuppressNextNotification = true;
				}
			}
		}
		for (int j = 0; j < activeItems.Count; j++)
		{
			EncounterTrackable.SuppressNextNotification = true;
			activeItems[j].Pickup(player);
		}
		for (int k = 0; k < guns.Count; k++)
		{
			EncounterTrackable.SuppressNextNotification = true;
			guns[k].Pickup(player);
		}
		for (int l = 0; l < additionalItems.Count; l++)
		{
			EncounterTrackable.SuppressNextNotification = true;
			additionalItems[l].Pickup(player);
		}
		player.ChangeGun(1);
		EncounterTrackable.SuppressNextNotification = false;
		IsPileOfDarkSoulsPickup = false;
		player.healthHaver.ForceSetCurrentHealth(currentHealth);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!base.sprite)
		{
			return 1000f;
		}
		return Vector2.Distance(point, base.sprite.WorldCenter) / 2f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && interactor.PlayerIDX == TargetPlayerID && RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this && interactor.PlayerIDX == TargetPlayerID)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void Interact(PlayerController interactor)
	{
		if ((bool)this && interactor.PlayerIDX == TargetPlayerID)
		{
			if (RoomHandler.unassignedInteractableObjects.Contains(this))
			{
				RoomHandler.unassignedInteractableObjects.Remove(this);
			}
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			Pickup(interactor);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}
}
