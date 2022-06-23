using System;
using UnityEngine;

public class ScalingProjectileModifier : MonoBehaviour
{
	public bool IsSynergyContingent;

	[LongNumericEnum]
	public CustomSynergyType SynergyToTest;

	public float PercentGainPerUnit = 2f;

	[NonSerialized]
	public float ScaleMultiplier = 1f;

	[NonSerialized]
	public float DamageMultiplier = 1f;

	public float MaximumDamageMultiplier = -1f;

	[NonSerialized]
	public float ScaleToDamageRatio = 1f;

	private Projectile m_projectile;

	private float m_lastElapsedDistance;

	private float m_totalElapsedDistance;

	private float m_elapsedSizeGain = 1f;

	private float m_elapsedDamageGain = 1f;

	public void Start()
	{
		m_projectile = GetComponent<Projectile>();
		if (!IsSynergyContingent || ((bool)m_projectile.PossibleSourceGun && m_projectile.PossibleSourceGun.CurrentOwner is PlayerController && (m_projectile.PossibleSourceGun.CurrentOwner as PlayerController).HasActiveBonusSynergy(SynergyToTest)))
		{
			m_projectile.specRigidbody.UpdateCollidersOnScale = true;
			m_projectile.OnPostUpdate += HandlePostUpdate;
		}
	}

	public virtual void OnDespawned()
	{
		if ((bool)m_projectile)
		{
			m_projectile.RuntimeUpdateScale(1f / m_projectile.AdditionalScaleMultiplier);
			m_projectile.baseData.damage = m_projectile.baseData.damage / m_elapsedDamageGain;
		}
		UnityEngine.Object.Destroy(this);
	}

	private void HandlePostUpdate(Projectile proj)
	{
		if ((bool)proj)
		{
			float elapsedDistance = proj.GetElapsedDistance();
			if (elapsedDistance < m_lastElapsedDistance)
			{
				m_lastElapsedDistance = 0f;
				m_totalElapsedDistance = 0f;
			}
			m_totalElapsedDistance += elapsedDistance - m_lastElapsedDistance;
			m_lastElapsedDistance = elapsedDistance;
			m_totalElapsedDistance = Mathf.Clamp(m_totalElapsedDistance, 0f, 160f);
			float num = 1f + m_totalElapsedDistance / 100f * PercentGainPerUnit;
			float num2 = (num - 1f) * ScaleToDamageRatio + 1f;
			float num3 = ((!(MaximumDamageMultiplier > 0f)) ? (num2 * DamageMultiplier) : Mathf.Min(MaximumDamageMultiplier, num2 * DamageMultiplier));
			float num4 = num * ScaleMultiplier / m_elapsedSizeGain;
			if (num4 > 1.25f)
			{
				m_projectile.RuntimeUpdateScale(num * ScaleMultiplier / m_elapsedSizeGain);
				m_elapsedSizeGain = num * ScaleMultiplier;
			}
			m_projectile.baseData.damage *= num3 / m_elapsedDamageGain;
			m_elapsedDamageGain = num3;
		}
	}
}
