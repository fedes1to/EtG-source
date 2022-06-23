using System;
using UnityEngine;

public class GunExtraSettingSynergyProcessor : MonoBehaviour
{
	public CustomSynergyType SynergyToCheck;

	public bool ChangesReflectedBulletDamage;

	public float ReflectedBulletDamageModifier = 1f;

	public bool ChangesReflectedBulletScale;

	public float ReflectedBulletScaleModifier = 1f;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		if (ChangesReflectedBulletDamage)
		{
			Gun gun = m_gun;
			gun.OnReflectedBulletDamageModifier = (Func<float, float>)Delegate.Combine(gun.OnReflectedBulletDamageModifier, new Func<float, float>(GetReflectedBulletDamageModifier));
		}
		if (ChangesReflectedBulletScale)
		{
			Gun gun2 = m_gun;
			gun2.OnReflectedBulletScaleModifier = (Func<float, float>)Delegate.Combine(gun2.OnReflectedBulletScaleModifier, new Func<float, float>(GetReflectedBulletScaleModifier));
		}
	}

	private float GetReflectedBulletScaleModifier(float inScale)
	{
		if (HasSynergy())
		{
			return inScale * ReflectedBulletScaleModifier;
		}
		return inScale;
	}

	private bool HasSynergy()
	{
		return (bool)m_gun && m_gun.CurrentOwner is PlayerController && (m_gun.CurrentOwner as PlayerController).HasActiveBonusSynergy(SynergyToCheck);
	}

	private float GetReflectedBulletDamageModifier(float inDamage)
	{
		if (HasSynergy())
		{
			return inDamage * ReflectedBulletDamageModifier;
		}
		return inDamage;
	}
}
