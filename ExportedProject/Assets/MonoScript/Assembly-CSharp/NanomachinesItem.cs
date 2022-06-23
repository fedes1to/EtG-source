using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NanomachinesItem : PassiveItem
{
	public int initialArmorBoost = 2;

	public float DamagePerArmor = 2f;

	protected float m_receivedDamageCounter;

	[Header("Nanomachines, Son")]
	public float RageSynergyDuration = 10f;

	public Color RageFlatColor = Color.red;

	public float RageDamageMultiplier = 2f;

	public GameObject RageOverheadVFX;

	private float m_rageElapsed;

	private GameObject rageInstanceVFX;

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add(m_receivedDamageCounter);
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (data.Count == 1)
		{
			m_receivedDamageCounter = (float)data[0];
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			if (!m_pickedUpThisRun)
			{
				player.healthHaver.Armor += initialArmorBoost;
			}
			player.OnReceivedDamage += PlayerReceivedDamage;
			base.Pickup(player);
		}
	}

	private void PlayerReceivedDamage(PlayerController obj)
	{
		m_receivedDamageCounter += 0.5f;
		float num = 0f;
		if (base.Owner.HasActiveBonusSynergy(CustomSynergyType.NANOMACHINES_SON))
		{
			num = 0.5f;
		}
		if (m_receivedDamageCounter >= DamagePerArmor - num)
		{
			m_receivedDamageCounter = 0f;
			m_owner.healthHaver.Armor += 1f;
			HandleRageEffect();
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<NanomachinesItem>().m_pickedUpThisRun = true;
		player.OnReceivedDamage -= PlayerReceivedDamage;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_owner)
		{
			m_owner.OnReceivedDamage -= PlayerReceivedDamage;
		}
	}

	private void HandleRageEffect()
	{
		if (!base.Owner.HasActiveBonusSynergy(CustomSynergyType.NANOMACHINES_SON))
		{
			return;
		}
		if (m_rageElapsed > 0f)
		{
			m_rageElapsed = RageSynergyDuration;
			if ((bool)RageOverheadVFX && rageInstanceVFX == null)
			{
				rageInstanceVFX = base.Owner.PlayEffectOnActor(RageOverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
			}
		}
		else
		{
			base.Owner.StartCoroutine(HandleRageCooldown());
		}
	}

	private IEnumerator HandleRageCooldown()
	{
		rageInstanceVFX = null;
		if ((bool)RageOverheadVFX)
		{
			rageInstanceVFX = base.Owner.PlayEffectOnActor(RageOverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
		}
		m_rageElapsed = RageSynergyDuration;
		StatModifier damageStat = new StatModifier
		{
			amount = RageDamageMultiplier,
			modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
			statToBoost = PlayerStats.StatType.Damage
		};
		PlayerController cachedOwner = base.Owner;
		cachedOwner.ownerlessStatModifiers.Add(damageStat);
		cachedOwner.stats.RecalculateStats(cachedOwner);
		Color rageColor = RageFlatColor;
		while (m_rageElapsed > 0f)
		{
			cachedOwner.baseFlatColorOverride = rageColor.WithAlpha(Mathf.Lerp(rageColor.a, 0f, 1f - Mathf.Clamp01(m_rageElapsed)));
			if ((bool)rageInstanceVFX && m_rageElapsed < RageSynergyDuration - 1f)
			{
				rageInstanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
				rageInstanceVFX = null;
			}
			yield return null;
			m_rageElapsed -= BraveTime.DeltaTime;
		}
		if ((bool)rageInstanceVFX)
		{
			rageInstanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
		}
		cachedOwner.ownerlessStatModifiers.Remove(damageStat);
		cachedOwner.stats.RecalculateStats(cachedOwner);
	}
}
