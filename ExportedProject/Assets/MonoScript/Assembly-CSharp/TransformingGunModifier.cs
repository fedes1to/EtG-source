using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformingGunModifier : MonoBehaviour
{
	[PickupIdentifier]
	public int BaseGunID;

	public bool TransformsOnAmmoThresholds;

	public List<AmmoThresholdTransformation> AmmoThresholdTransformations;

	public bool TransformsOnDamageDealt;

	public bool TransformationsAreTimeLimited;

	[ShowInInspectorIf("TransformationsAreTimeLimited", false)]
	public float TransformationDuration = 10f;

	public bool TransformationsAreAmmoLimited;

	[ShowInInspectorIf("TransformationsAreAmmoLimited", false)]
	public int TransformationAmmoCount = 10;

	private Gun m_gun;

	private float m_previousAmmoPercentage = 1f;

	private IEnumerator Start()
	{
		m_gun = GetComponent<Gun>();
		if (TransformsOnAmmoThresholds)
		{
			Gun gun = m_gun;
			gun.OnPostFired = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnPostFired, new Action<PlayerController, Gun>(HandlePostFired));
			Gun gun2 = m_gun;
			gun2.OnAmmoChanged = (Action<PlayerController, Gun>)Delegate.Combine(gun2.OnAmmoChanged, new Action<PlayerController, Gun>(HandlePostFired));
		}
		yield return null;
		if (m_gun.CurrentOwner != null && m_gun.CurrentOwner is PlayerController)
		{
			HandlePostFired(m_gun.CurrentOwner as PlayerController, m_gun);
		}
	}

	private float GetMaxAmmoSansInfinity(Gun g)
	{
		if (g.CurrentOwner == null)
		{
			return g.GetBaseMaxAmmo();
		}
		if (g.CurrentOwner is PlayerController)
		{
			if (g.RequiresFundsToShoot)
			{
				return g.ClipShotsRemaining;
			}
			if ((g.CurrentOwner as PlayerController).stats != null)
			{
				float statValue = (g.CurrentOwner as PlayerController).stats.GetStatValue(PlayerStats.StatType.AmmoCapacityMultiplier);
				return Mathf.RoundToInt(statValue * (float)g.GetBaseMaxAmmo());
			}
			return g.GetBaseMaxAmmo();
		}
		return g.GetBaseMaxAmmo();
	}

	private void HandlePostFired(PlayerController arg1, Gun arg2)
	{
		if (!arg2.enabled)
		{
			return;
		}
		float num = (float)m_gun.CurrentAmmo / (1f * GetMaxAmmoSansInfinity(m_gun));
		AmmoThresholdTransformation? ammoThresholdTransformation = null;
		for (int i = 0; i < AmmoThresholdTransformations.Count; i++)
		{
			AmmoThresholdTransformation value = AmmoThresholdTransformations[i];
			if (num <= value.GetAmmoPercentage())
			{
				if (!ammoThresholdTransformation.HasValue)
				{
					ammoThresholdTransformation = value;
				}
				else if (value.GetAmmoPercentage() < ammoThresholdTransformation.Value.GetAmmoPercentage())
				{
					ammoThresholdTransformation = value;
				}
			}
		}
		if (ammoThresholdTransformation.HasValue)
		{
			Gun gun = PickupObjectDatabase.GetById(ammoThresholdTransformation.Value.TargetGunID) as Gun;
			if ((bool)gun && gun.shootAnimation != m_gun.shootAnimation)
			{
				m_gun.TransformToTargetGun(gun);
			}
		}
		m_previousAmmoPercentage = num;
	}
}
