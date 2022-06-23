using System;
using System.Collections.Generic;
using UnityEngine;

public class BankMaskItem : PassiveItem, IPaydayItem
{
	public tk2dSpriteAnimation OverrideAnimLib;

	public tk2dSprite OverrideHandSprite;

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

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OverrideAnimationLibrary = OverrideAnimLib;
			player.OverridePlayerSwitchState = PlayableCharacters.Pilot.ToString();
			player.SetOverrideShader(ShaderCache.Acquire(player.LocalShaderName));
			if (player.characterIdentity == PlayableCharacters.Eevee)
			{
				player.GetComponent<CharacterAnimationRandomizer>().AddOverrideAnimLibrary(OverrideAnimLib);
			}
			player.ChangeHandsToCustomType(OverrideHandSprite.Collection, OverrideHandSprite.spriteId);
			if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
			{
				PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
			}
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

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.OverrideAnimationLibrary = null;
		player.OverridePlayerSwitchState = null;
		player.ClearOverrideShader();
		if (player.characterIdentity == PlayableCharacters.Eevee)
		{
			player.GetComponent<CharacterAnimationRandomizer>().RemoveOverrideAnimLibrary(OverrideAnimLib);
		}
		player.RevertHandsToBaseType();
		if (PassiveItem.ActiveFlagItems.ContainsKey(player) && PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		debrisObject.GetComponent<BankMaskItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!m_pickedUp || !m_owner)
		{
			return;
		}
		m_owner.RevertHandsToBaseType();
		m_owner.OverrideAnimationLibrary = null;
		m_owner.OverridePlayerSwitchState = null;
		m_owner.ClearOverrideShader();
		if (m_owner.characterIdentity == PlayableCharacters.Eevee)
		{
			m_owner.GetComponent<CharacterAnimationRandomizer>().RemoveOverrideAnimLibrary(OverrideAnimLib);
		}
		if (PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
	}
}
