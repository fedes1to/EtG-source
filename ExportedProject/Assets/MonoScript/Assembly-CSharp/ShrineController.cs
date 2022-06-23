using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ShrineController : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public string displayTextKey;

	public string acceptOptionKey;

	public string declineOptionKey;

	public Transform talkPoint;

	public int healthToGive;

	public int armorToGive;

	public int moneyToGive;

	public int ammoPercentageToReplenish;

	public bool takesCurrentGun;

	public bool appliesStatChanges;

	public List<StatModifier> statModifiers;

	public bool cleansesCurse;

	public GameObject onPlayerVFX;

	public Vector3 playerVFXOffset = Vector3.zero;

	private int m_useCount;

	private RoomHandler m_parentRoom;

	private GameObject m_instanceMinimapIcon;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_parentRoom = room;
		room.OptionalDoorTopDecorable = ResourceCache.Acquire("Global Prefabs/Shrine_Lantern") as GameObject;
		RegisterMinimapIcon();
	}

	public void RegisterMinimapIcon()
	{
		m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_parentRoom, (GameObject)BraveResources.Load("Global Prefabs/Minimap_Shrine_Icon"));
	}

	public void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_parentRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	private void DoShrineEffect(PlayerController player)
	{
		if (takesCurrentGun && (player.CurrentGun == null || !player.CurrentGun.CanActuallyBeDropped(player)))
		{
			m_useCount--;
			m_parentRoom.RegisterInteractable(this);
			return;
		}
		AkSoundEngine.PostEvent("Play_OBJ_shrine_accept_01", base.gameObject);
		if (healthToGive > 0)
		{
			AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", base.gameObject);
			player.healthHaver.ApplyHealing(healthToGive);
		}
		else if (healthToGive < 0)
		{
			player.healthHaver.ApplyDamage(healthToGive * -1, Vector2.zero, StringTableManager.GetEnemiesString("#SHRINE"), CoreDamageTypes.None, DamageCategory.Environment, true);
		}
		if (armorToGive > 0)
		{
			player.healthHaver.Armor += armorToGive;
		}
		if (moneyToGive > 0)
		{
			AkSoundEngine.PostEvent("Play_OBJ_item_purchase_01", base.gameObject);
			player.carriedConsumables.Currency += moneyToGive;
		}
		if (ammoPercentageToReplenish > 0)
		{
			for (int i = 0; i < player.inventory.AllGuns.Count; i++)
			{
				int num = player.inventory.AllGuns[i].AdjustedMaxAmmo * ammoPercentageToReplenish;
				if (num <= 0)
				{
					AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", base.gameObject);
					num = player.inventory.AllGuns[i].ammo * ammoPercentageToReplenish;
				}
				if (num <= 0)
				{
					Debug.LogError("Shrine is attempting to give negative ammo!");
					num = 1;
				}
				player.inventory.AllGuns[i].GainAmmo(num);
			}
		}
		if (takesCurrentGun && player.CurrentGun != null && player.CurrentGun.CanActuallyBeDropped(player))
		{
			player.inventory.DestroyCurrentGun();
		}
		if (appliesStatChanges)
		{
			for (int j = 0; j < statModifiers.Count; j++)
			{
				if (player.ownerlessStatModifiers == null)
				{
					player.ownerlessStatModifiers = new List<StatModifier>();
				}
				player.ownerlessStatModifiers.Add(statModifiers[j]);
			}
			player.stats.RecalculateStats(player);
		}
		if (cleansesCurse)
		{
			StatModifier statModifier = new StatModifier();
			statModifier.amount = player.stats.GetStatValue(PlayerStats.StatType.Curse) * -1f;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			statModifier.statToBoost = PlayerStats.StatType.Curse;
			player.ownerlessStatModifiers.Add(statModifier);
			player.stats.RecalculateStats(player);
		}
		if (onPlayerVFX != null)
		{
			player.PlayEffectOnActor(onPlayerVFX, playerVFXOffset);
		}
		if (base.transform.parent != null)
		{
			EncounterTrackable component = base.transform.parent.gameObject.GetComponent<EncounterTrackable>();
			if (component != null)
			{
				component.ForceDoNotification(m_instanceMinimapIcon.GetComponent<tk2dBaseSprite>());
			}
		}
		GetRidOfMinimapIcon();
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (base.sprite == null)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, base.specRigidbody.UnitBottomLeft, base.specRigidbody.UnitDimensions);
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
	}

	private IEnumerator HandleShrineConversation(PlayerController interactor)
	{
		TextBoxManager.ShowStoneTablet(talkPoint.position, talkPoint, -1f, StringTableManager.GetString(displayTextKey));
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		yield return null;
		GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(acceptOptionKey), StringTableManager.GetString(declineOptionKey));
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
		if (selectedResponse == 0)
		{
			DoShrineEffect(interactor);
			yield break;
		}
		m_useCount--;
		m_parentRoom.RegisterInteractable(this);
	}

	public void Interact(PlayerController interactor)
	{
		if (m_useCount <= 0)
		{
			m_useCount++;
			m_parentRoom.DeregisterInteractable(this);
			StartCoroutine(HandleShrineConversation(interactor));
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
