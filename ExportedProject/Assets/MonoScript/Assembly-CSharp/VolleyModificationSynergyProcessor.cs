using System;
using UnityEngine;

public class VolleyModificationSynergyProcessor : MonoBehaviour
{
	public VolleyModificationSynergyData[] synergies;

	private Gun m_gun;

	private PassiveItem m_item;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		if ((bool)m_gun)
		{
			Gun gun = m_gun;
			gun.PostProcessVolley = (Action<ProjectileVolleyData>)Delegate.Combine(gun.PostProcessVolley, new Action<ProjectileVolleyData>(HandleVolleyRebuild));
			bool flag = false;
			if (synergies != null)
			{
				for (int i = 0; i < synergies.Length; i++)
				{
					if (synergies[i].ReplacesSourceProjectile)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				Gun gun2 = m_gun;
				gun2.OnPreFireProjectileModifier = (Func<Gun, Projectile, ProjectileModule, Projectile>)Delegate.Combine(gun2.OnPreFireProjectileModifier, new Func<Gun, Projectile, ProjectileModule, Projectile>(HandlePreFireProjectileReplacement));
			}
		}
		else
		{
			m_item = GetComponent<PassiveItem>();
			if ((bool)m_item)
			{
				PassiveItem item = m_item;
				item.OnPickedUp = (Action<PlayerController>)Delegate.Combine(item.OnPickedUp, new Action<PlayerController>(LinkPassiveItem));
				PassiveItem item2 = m_item;
				item2.OnDisabled = (Action<PlayerController>)Delegate.Combine(item2.OnDisabled, new Action<PlayerController>(DelinkPassiveItem));
			}
		}
	}

	private Projectile HandlePreFireProjectileReplacementPlayer(Gun sourceGun, Projectile sourceProjectile)
	{
		Projectile result = sourceProjectile;
		PlayerController playerController = sourceGun.CurrentOwner as PlayerController;
		if (synergies != null)
		{
			for (int i = 0; i < synergies.Length; i++)
			{
				VolleyModificationSynergyData volleyModificationSynergyData = synergies[i];
				if (!volleyModificationSynergyData.ReplacesSourceProjectile || !playerController || !playerController.HasActiveBonusSynergy(volleyModificationSynergyData.RequiredSynergy) || !(UnityEngine.Random.value < volleyModificationSynergyData.ReplacementChance))
				{
					continue;
				}
				if (volleyModificationSynergyData.UsesMultipleReplacementProjectiles)
				{
					if (volleyModificationSynergyData.MultipleReplacementsSequential)
					{
						result = volleyModificationSynergyData.MultipleReplacementProjectiles[volleyModificationSynergyData.multipleSequentialReplacementIndex];
						volleyModificationSynergyData.multipleSequentialReplacementIndex = (volleyModificationSynergyData.multipleSequentialReplacementIndex + 1) % volleyModificationSynergyData.MultipleReplacementProjectiles.Length;
					}
					else
					{
						result = volleyModificationSynergyData.MultipleReplacementProjectiles[UnityEngine.Random.Range(0, volleyModificationSynergyData.MultipleReplacementProjectiles.Length)];
					}
				}
				else
				{
					result = volleyModificationSynergyData.ReplacementProjectile;
				}
			}
		}
		return result;
	}

	private Projectile HandlePreFireProjectileReplacement(Gun sourceGun, Projectile sourceProjectile, ProjectileModule sourceModule)
	{
		Projectile result = sourceProjectile;
		PlayerController playerController = sourceGun.CurrentOwner as PlayerController;
		if (synergies != null)
		{
			for (int i = 0; i < synergies.Length; i++)
			{
				VolleyModificationSynergyData volleyModificationSynergyData = synergies[i];
				if (!volleyModificationSynergyData.ReplacesSourceProjectile || !playerController || !playerController.HasActiveBonusSynergy(volleyModificationSynergyData.RequiredSynergy) || (volleyModificationSynergyData.OnlyReplacesAdditionalProjectiles && !sourceModule.ignoredForReloadPurposes) || ((bool)sourceGun && sourceGun.IsCharging && volleyModificationSynergyData.RequiredSynergy != CustomSynergyType.ANTIMATTER_BODY))
				{
					continue;
				}
				if (volleyModificationSynergyData.ReplacementSkipsChargedShots && sourceModule.shootStyle == ProjectileModule.ShootStyle.Charged)
				{
					bool flag = false;
					for (int j = 0; j < sourceModule.chargeProjectiles.Count; j++)
					{
						if (sourceModule.chargeProjectiles[j].Projectile == sourceProjectile && sourceModule.chargeProjectiles[j].ChargeTime > 0f)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				if (!(UnityEngine.Random.value < volleyModificationSynergyData.ReplacementChance))
				{
					continue;
				}
				if (volleyModificationSynergyData.UsesMultipleReplacementProjectiles)
				{
					if (volleyModificationSynergyData.MultipleReplacementsSequential)
					{
						result = volleyModificationSynergyData.MultipleReplacementProjectiles[volleyModificationSynergyData.multipleSequentialReplacementIndex];
						volleyModificationSynergyData.multipleSequentialReplacementIndex = (volleyModificationSynergyData.multipleSequentialReplacementIndex + 1) % volleyModificationSynergyData.MultipleReplacementProjectiles.Length;
					}
					else
					{
						result = volleyModificationSynergyData.MultipleReplacementProjectiles[UnityEngine.Random.Range(0, volleyModificationSynergyData.MultipleReplacementProjectiles.Length)];
					}
				}
				else
				{
					result = volleyModificationSynergyData.ReplacementProjectile;
				}
			}
		}
		return result;
	}

	private void LinkPassiveItem(PlayerController p)
	{
		p.stats.AdditionalVolleyModifiers -= HandleVolleyRebuild;
		p.stats.AdditionalVolleyModifiers += HandleVolleyRebuild;
		bool flag = false;
		if (synergies != null)
		{
			for (int i = 0; i < synergies.Length; i++)
			{
				if (synergies[i].ReplacesSourceProjectile)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			p.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Combine(p.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileReplacementPlayer));
		}
	}

	private void DelinkPassiveItem(PlayerController p)
	{
		if ((bool)p && p.stats != null)
		{
			p.stats.AdditionalVolleyModifiers -= HandleVolleyRebuild;
			p.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Remove(p.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileReplacementPlayer));
		}
	}

	private void HandleVolleyRebuild(ProjectileVolleyData targetVolley)
	{
		PlayerController playerController = null;
		if ((bool)m_gun)
		{
			playerController = m_gun.CurrentOwner as PlayerController;
		}
		else if ((bool)m_item)
		{
			playerController = m_item.Owner;
		}
		if (!playerController || synergies == null)
		{
			return;
		}
		for (int i = 0; i < synergies.Length; i++)
		{
			if (playerController.HasActiveBonusSynergy(synergies[i].RequiredSynergy))
			{
				ApplySynergy(targetVolley, synergies[i], playerController);
			}
		}
	}

	private void ApplySynergy(ProjectileVolleyData volley, VolleyModificationSynergyData synergy, PlayerController owner)
	{
		if (synergy.AddsChargeProjectile)
		{
			volley.projectiles[0].chargeProjectiles.Add(synergy.ChargeProjectileToAdd);
		}
		if (synergy.AddsModules)
		{
			bool flag = true;
			if (volley != null && volley.projectiles.Count > 0 && volley.projectiles[0].projectiles != null && volley.projectiles[0].projectiles.Count > 0)
			{
				Projectile projectile = volley.projectiles[0].projectiles[0];
				if ((bool)projectile && (bool)projectile.GetComponent<ArtfulDodgerProjectileController>())
				{
					flag = false;
				}
			}
			if (flag)
			{
				for (int i = 0; i < synergy.ModulesToAdd.Length; i++)
				{
					synergy.ModulesToAdd[i].isExternalAddedModule = true;
					volley.projectiles.Add(synergy.ModulesToAdd[i]);
				}
			}
		}
		if (synergy.AddsDuplicatesOfBaseModule)
		{
			GunVolleyModificationItem.AddDuplicateOfBaseModule(volley, m_gun.CurrentOwner as PlayerController, synergy.DuplicatesOfBaseModule, synergy.BaseModuleDuplicateAngle, 0f);
		}
		if (synergy.SetsNumberFinalProjectiles)
		{
			bool flag2 = false;
			for (int j = 0; j < volley.projectiles.Count; j++)
			{
				if (!flag2 && synergy.AddsNewFinalProjectile && !volley.projectiles[j].usesOptionalFinalProjectile)
				{
					flag2 = true;
					m_gun.OverrideFinaleAudio = true;
					volley.projectiles[j].usesOptionalFinalProjectile = true;
					volley.projectiles[j].numberOfFinalProjectiles = 1;
					volley.projectiles[j].finalProjectile = synergy.NewFinalProjectile;
					volley.projectiles[j].finalAmmoType = GameUIAmmoType.AmmoType.CUSTOM;
					volley.projectiles[j].finalCustomAmmoType = synergy.NewFinalProjectileAmmoType;
					if (string.IsNullOrEmpty(m_gun.finalShootAnimation))
					{
						m_gun.finalShootAnimation = m_gun.shootAnimation;
					}
				}
				if (volley.projectiles[j].usesOptionalFinalProjectile)
				{
					volley.projectiles[j].numberOfFinalProjectiles = synergy.NumberFinalProjectiles;
				}
			}
		}
		if (synergy.SetsBurstCount)
		{
			if (synergy.MakesDefaultModuleBurst && volley.projectiles.Count > 0 && volley.projectiles[0].shootStyle != ProjectileModule.ShootStyle.Burst)
			{
				volley.projectiles[0].shootStyle = ProjectileModule.ShootStyle.Burst;
			}
			for (int k = 0; k < volley.projectiles.Count; k++)
			{
				if (volley.projectiles[k].shootStyle == ProjectileModule.ShootStyle.Burst)
				{
					int burstShotCount = volley.projectiles[k].burstShotCount;
					int num = volley.projectiles[k].GetModNumberOfShotsInClip(owner);
					if (num < 0)
					{
						num = int.MaxValue;
					}
					int burstShotCount2 = Mathf.Clamp(Mathf.RoundToInt((float)burstShotCount * synergy.BurstMultiplier) + synergy.BurstShift, 1, num);
					volley.projectiles[k].burstShotCount = burstShotCount2;
				}
			}
		}
		if (synergy.AddsPossibleProjectileToPrimaryModule)
		{
			volley.projectiles[0].projectiles.Add(synergy.AdditionalModuleProjectile);
		}
	}
}
