using System;
using System.Collections.Generic;
using UnityEngine;

public class BankBagItem : PassiveItem, IPaydayItem
{
	public static float cachedCoinLifespan = 6f;

	public float CoinLifespan = 6f;

	public float MinPercentToDrop = 0.5f;

	public float MaxPercentToDrop = 1f;

	public int MaxCoinsToDrop = -1;

	public GameObject DropVFX;

	public GameObject AttachmentObject;

	private GameObject instanceAttachment;

	private tk2dSprite instanceAttachmentSprite;

	[NonSerialized]
	public bool HasSetOrder;

	[NonSerialized]
	public string ID01;

	[NonSerialized]
	public string ID02;

	[NonSerialized]
	public string ID03;

	public void StoreData(string id1, string id2, string id3)
	{
		ID01 = id1;
		ID02 = id2;
		ID03 = id3;
		HasSetOrder = true;
	}

	public bool HasCachedData()
	{
		return HasSetOrder;
	}

	public string GetID(int placement)
	{
		switch (placement)
		{
		case 0:
			return ID01;
		case 1:
			return ID02;
		default:
			return ID03;
		}
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add(HasSetOrder);
		data.Add(ID01);
		data.Add(ID02);
		data.Add(ID03);
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (data.Count == 4)
		{
			HasSetOrder = (bool)data[0];
			ID01 = (string)data[1];
			ID02 = (string)data[2];
			ID03 = (string)data[3];
		}
	}

	public void Awake()
	{
		cachedCoinLifespan = CoinLifespan;
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
			{
				PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
			}
			player.OnReceivedDamage += HandlePlayerDamaged;
			instanceAttachment = player.RegisterAttachedObject(AttachmentObject, "center");
			instanceAttachment.transform.parent = player.sprite.transform;
			instanceAttachmentSprite = instanceAttachment.GetComponentInChildren<tk2dSprite>();
			if (!PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[player].Add(GetType(), 1);
			}
			else
			{
				PassiveItem.ActiveFlagItems[player][GetType()] = PassiveItem.ActiveFlagItems[player][GetType()] + 1;
			}
		}
	}

	private void HandlePlayerDamaged(PlayerController p)
	{
		if (p.carriedConsumables.Currency > 0)
		{
			int num = UnityEngine.Random.Range(Mathf.FloorToInt((float)p.carriedConsumables.Currency * MinPercentToDrop), Mathf.CeilToInt((float)p.carriedConsumables.Currency * MaxPercentToDrop) + 1);
			if (MaxCoinsToDrop > 0)
			{
				num = Mathf.Clamp(num, 0, MaxCoinsToDrop);
			}
			num = Mathf.Min(num, p.carriedConsumables.Currency);
			if ((bool)DropVFX)
			{
				p.PlayEffectOnActor(DropVFX, Vector3.zero, false, true);
			}
			AkSoundEngine.PostEvent("Play_OBJ_coin_spill_01", base.gameObject);
			p.carriedConsumables.Currency = p.carriedConsumables.Currency - num;
			LootEngine.SpawnCurrencyManual(p.CenterPosition, num);
		}
	}

	private void LateUpdate()
	{
		if ((bool)instanceAttachment && m_pickedUp && (bool)m_owner)
		{
			instanceAttachment.transform.position = m_owner.sprite.WorldCenter + new Vector2(0f, -0.125f);
			instanceAttachmentSprite.FlipX = m_owner.sprite.FlipX;
			instanceAttachmentSprite.transform.localPosition = new Vector3((!instanceAttachmentSprite.FlipX) ? (-0.5f) : 0.5f, -0.5f, 0f);
			instanceAttachmentSprite.renderer.enabled = m_owner.IsVisible && m_owner.sprite.renderer.enabled && !m_owner.IsFalling && !m_owner.IsDodgeRolling;
			instanceAttachmentSprite.UpdateZDepth();
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if ((bool)player)
		{
			player.OnReceivedDamage -= HandlePlayerDamaged;
			player.DeregisterAttachedObject(instanceAttachment);
			instanceAttachment = null;
			instanceAttachmentSprite = null;
		}
		if (PassiveItem.ActiveFlagItems.ContainsKey(player) && PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		debrisObject.GetComponent<BankBagItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!m_pickedUp || !m_owner)
		{
			return;
		}
		m_owner.OnReceivedDamage -= HandlePlayerDamaged;
		m_owner.DeregisterAttachedObject(instanceAttachment);
		instanceAttachment = null;
		instanceAttachmentSprite = null;
		if (PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
	}
}
