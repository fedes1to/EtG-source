using System;
using UnityEngine;

public class GunRechargeSynergyProcessor : MonoBehaviour
{
	public CustomSynergyType SynergyToCheck;

	public float CDR_Multiplier = 1f;

	protected Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		if ((bool)m_gun)
		{
			Gun gun = m_gun;
			gun.ModifyActiveCooldownDamage = (Func<float, float>)Delegate.Combine(gun.ModifyActiveCooldownDamage, new Func<float, float>(HandleActiveCooldownModification));
		}
	}

	private float HandleActiveCooldownModification(float inDamage)
	{
		if ((bool)m_gun && m_gun.CurrentOwner is PlayerController && (m_gun.CurrentOwner as PlayerController).HasActiveBonusSynergy(SynergyToCheck))
		{
			return inDamage * CDR_Multiplier;
		}
		return inDamage;
	}
}
