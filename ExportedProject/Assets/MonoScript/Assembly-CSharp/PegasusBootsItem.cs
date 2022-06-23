using System;
using System.Collections.Generic;
using UnityEngine;

public class PegasusBootsItem : PassiveItem
{
	public bool ModifiesDodgeRoll;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public float DodgeRollTimeMultiplier = 0.9f;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public float DodgeRollDistanceMultiplier = 1.25f;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public int AdditionalInvulnerabilityFrames;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			if (ModifiesDodgeRoll)
			{
				player.rollStats.rollDistanceMultiplier *= DodgeRollDistanceMultiplier;
				player.rollStats.rollTimeMultiplier *= DodgeRollTimeMultiplier;
				player.rollStats.additionalInvulnerabilityFrames += AdditionalInvulnerabilityFrames;
			}
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
			player.OnRollStarted += OnRollStarted;
			base.Pickup(player);
		}
	}

	private void OnRollStarted(PlayerController obj, Vector2 dirVec)
	{
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if (ModifiesDodgeRoll)
		{
			player.rollStats.rollDistanceMultiplier /= DodgeRollDistanceMultiplier;
			player.rollStats.rollTimeMultiplier /= DodgeRollTimeMultiplier;
			player.rollStats.additionalInvulnerabilityFrames -= AdditionalInvulnerabilityFrames;
			player.rollStats.additionalInvulnerabilityFrames = Mathf.Max(player.rollStats.additionalInvulnerabilityFrames, 0);
		}
		if (PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		player.OnRollStarted -= OnRollStarted;
		debrisObject.GetComponent<PegasusBootsItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp && (bool)m_owner && PassiveItem.ActiveFlagItems != null && PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
		if (m_owner != null)
		{
			m_owner.OnRollStarted -= OnRollStarted;
		}
		base.OnDestroy();
	}
}
