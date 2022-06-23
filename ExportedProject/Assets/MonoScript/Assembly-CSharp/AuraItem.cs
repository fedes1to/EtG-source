using System;
using UnityEngine;

public class AuraItem : PassiveItem
{
	public float AuraRadius = 5f;

	public CoreDamageTypes damageTypes;

	public float DamagePerSecond = 5f;

	public bool DamageFallsOffInRadius;

	public GameObject AuraVFX;

	public NumericSynergyMultiplier[] damageMultiplierSynergies;

	public NumericSynergyMultiplier[] rangeMultiplierSynergies;

	private GameObject m_extantAuraVFX;

	private Action<AIActor, float> AuraAction;

	private bool didDamageEnemies;

	private float ModifiedAuraRadius
	{
		get
		{
			return AuraRadius * GetRangeMultiplier();
		}
	}

	private float ModifiedDamagePerSecond
	{
		get
		{
			return DamagePerSecond * GetDamageMultiplier();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_pickedUp && (bool)m_owner && !m_owner.IsStealthed && !GameManager.Instance.IsLoadingLevel)
		{
			DoAura();
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		if (m_extantAuraVFX != null)
		{
			UnityEngine.Object.Destroy(m_extantAuraVFX);
			m_extantAuraVFX = null;
		}
		return base.Drop(player);
	}

	protected float GetDamageMultiplier()
	{
		float num = 1f;
		if (m_owner != null)
		{
			for (int i = 0; i < damageMultiplierSynergies.Length; i++)
			{
				if (m_owner.HasActiveBonusSynergy(damageMultiplierSynergies[i].RequiredSynergy))
				{
					num *= damageMultiplierSynergies[i].SynergyMultiplier;
				}
			}
		}
		return num;
	}

	protected float GetRangeMultiplier()
	{
		float num = 1f;
		if (m_owner != null)
		{
			for (int i = 0; i < rangeMultiplierSynergies.Length; i++)
			{
				if (m_owner.HasActiveBonusSynergy(rangeMultiplierSynergies[i].RequiredSynergy))
				{
					num *= rangeMultiplierSynergies[i].SynergyMultiplier;
				}
			}
		}
		return num;
	}

	protected virtual void DoAura()
	{
		if (m_extantAuraVFX == null)
		{
		}
		didDamageEnemies = false;
		if (AuraAction == null)
		{
			AuraAction = delegate(AIActor actor, float dist)
			{
				float num = ModifiedDamagePerSecond * BraveTime.DeltaTime;
				if (DamageFallsOffInRadius)
				{
					float t = dist / ModifiedAuraRadius;
					num = Mathf.Lerp(num, 0f, t);
				}
				if (num > 0f)
				{
					didDamageEnemies = true;
				}
				actor.healthHaver.ApplyDamage(num, Vector2.zero, "Aura", damageTypes);
			};
		}
		if (m_owner != null && m_owner.CurrentRoom != null)
		{
			m_owner.CurrentRoom.ApplyActionToNearbyEnemies(m_owner.CenterPosition, ModifiedAuraRadius, AuraAction);
		}
		if (didDamageEnemies)
		{
			m_owner.DidUnstealthyAction();
		}
	}

	protected override void OnDestroy()
	{
		if (m_extantAuraVFX != null)
		{
			UnityEngine.Object.Destroy(m_extantAuraVFX);
			m_extantAuraVFX = null;
		}
		base.OnDestroy();
	}
}
