using System;
using UnityEngine;

public class ExoticSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool SnapsToAngleMultiple;

	public float AngleMultiple = 90f;

	public bool HasChanceToGainAmmo;

	public float ChanceToGainAmmo;

	public bool SetsFlying;

	public bool SetsGoopReloadFree;

	private Gun m_gun;

	private PlayerController m_cachedPlayer;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		if (HasChanceToGainAmmo)
		{
			Gun gun = m_gun;
			gun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun.PostProcessProjectile, new Action<Projectile>(HandleGainAmmo));
		}
	}

	private void HandleGainAmmo(Projectile obj)
	{
		if ((bool)m_gun && m_gun.OwnerHasSynergy(RequiredSynergy) && UnityEngine.Random.value < ChanceToGainAmmo)
		{
			m_gun.GainAmmo(1);
		}
	}

	public void Update()
	{
		if (SnapsToAngleMultiple && (bool)m_gun)
		{
			if (m_gun.OwnerHasSynergy(RequiredSynergy))
			{
				m_gun.preventRotation = true;
				m_gun.OverrideAngleSnap = AngleMultiple;
			}
			else
			{
				m_gun.preventRotation = false;
				m_gun.OverrideAngleSnap = null;
			}
		}
		if (SetsGoopReloadFree && (bool)m_gun)
		{
			if (m_gun.OwnerHasSynergy(RequiredSynergy))
			{
				m_gun.GoopReloadsFree = true;
			}
			else
			{
				m_gun.GoopReloadsFree = false;
			}
		}
		if (!SetsFlying)
		{
			return;
		}
		if ((bool)m_gun && m_gun.OwnerHasSynergy(RequiredSynergy))
		{
			if (!m_cachedPlayer)
			{
				m_cachedPlayer = m_gun.CurrentOwner as PlayerController;
				m_cachedPlayer.SetIsFlying(true, "synergy flight");
				m_cachedPlayer.AdditionalCanDodgeRollWhileFlying.AddOverride("synergy flight");
			}
		}
		else if ((bool)m_cachedPlayer)
		{
			m_cachedPlayer.AdditionalCanDodgeRollWhileFlying.RemoveOverride("synergy flight");
			m_cachedPlayer.SetIsFlying(false, "synergy flight");
			m_cachedPlayer = null;
		}
	}

	private void OnDisable()
	{
		if ((bool)m_cachedPlayer)
		{
			m_cachedPlayer.AdditionalCanDodgeRollWhileFlying.RemoveOverride("synergy flight");
			m_cachedPlayer.SetIsFlying(false, "synergy flight");
			m_cachedPlayer = null;
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_cachedPlayer)
		{
			m_cachedPlayer.AdditionalCanDodgeRollWhileFlying.RemoveOverride("synergy flight");
			m_cachedPlayer.SetIsFlying(false, "synergy flight");
			m_cachedPlayer = null;
		}
	}
}
