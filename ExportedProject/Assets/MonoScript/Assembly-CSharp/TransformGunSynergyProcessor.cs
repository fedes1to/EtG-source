using System;
using Dungeonator;
using UnityEngine;

public class TransformGunSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	[PickupIdentifier(typeof(Gun))]
	public int NonSynergyGunId = -1;

	[PickupIdentifier(typeof(Gun))]
	public int SynergyGunId = -1;

	private Gun m_gun;

	private bool m_transformed;

	[NonSerialized]
	public bool ShouldResetAmmoAfterTransformation;

	[NonSerialized]
	public int ResetAmmoCount;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if (Dungeon.IsGenerating || Dungeon.ShouldAttemptToLoadFromMidgameSave)
		{
			return;
		}
		if ((bool)m_gun && m_gun.CurrentOwner is PlayerController)
		{
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			if (!m_gun.enabled)
			{
				return;
			}
			if (playerController.HasActiveBonusSynergy(SynergyToCheck) && !m_transformed)
			{
				m_transformed = true;
				m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(SynergyGunId) as Gun);
				if (ShouldResetAmmoAfterTransformation)
				{
					m_gun.ammo = ResetAmmoCount;
				}
			}
			else if (!playerController.HasActiveBonusSynergy(SynergyToCheck) && m_transformed)
			{
				m_transformed = false;
				m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(NonSynergyGunId) as Gun);
				if (ShouldResetAmmoAfterTransformation)
				{
					m_gun.ammo = ResetAmmoCount;
				}
			}
		}
		else if ((bool)m_gun && !m_gun.CurrentOwner && m_transformed)
		{
			m_transformed = false;
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(NonSynergyGunId) as Gun);
			if (ShouldResetAmmoAfterTransformation)
			{
				m_gun.ammo = ResetAmmoCount;
			}
		}
		ShouldResetAmmoAfterTransformation = false;
	}
}
