using System;
using System.Collections.Generic;
using UnityEngine;

public class DuctTapeItem : PlayerItem
{
	private Gun m_validSourceGun;

	private Gun m_validTargetGun;

	public override bool CanBeUsed(PlayerController user)
	{
		if (!user || user.inventory == null || user.inventory.AllGuns.Count < 2)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < user.inventory.AllGuns.Count; i++)
		{
			if ((bool)user.inventory.AllGuns[i] && !user.inventory.AllGuns[i].InfiniteAmmo && user.inventory.AllGuns[i].CanActuallyBeDropped(user))
			{
				num++;
			}
		}
		if (num < 2)
		{
			return false;
		}
		if (!IsGunValid(user.CurrentGun, m_validSourceGun))
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	public static ProjectileVolleyData TransferDuctTapeModules(ProjectileVolleyData source, ProjectileVolleyData target, Gun targetGun)
	{
		ProjectileVolleyData projectileVolleyData = ScriptableObject.CreateInstance<ProjectileVolleyData>();
		if (target != null)
		{
			projectileVolleyData.InitializeFrom(target);
		}
		else
		{
			projectileVolleyData.projectiles = new List<ProjectileModule>();
			projectileVolleyData.projectiles.Add(ProjectileModule.CreateClone(targetGun.singleModule));
			projectileVolleyData.BeamRotationDegreesPerSecond = float.MaxValue;
		}
		for (int i = 0; i < source.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = source.projectiles[i];
			if (projectileModule.IsDuctTapeModule)
			{
				ProjectileModule item = ProjectileModule.CreateClone(projectileModule);
				projectileVolleyData.projectiles.Add(item);
			}
		}
		ReconfigureVolley(projectileVolleyData);
		return projectileVolleyData;
	}

	protected static ProjectileVolleyData CombineVolleys(Gun sourceGun, Gun mergeGun)
	{
		ProjectileVolleyData projectileVolleyData = ScriptableObject.CreateInstance<ProjectileVolleyData>();
		if (sourceGun.RawSourceVolley != null)
		{
			projectileVolleyData.InitializeFrom(sourceGun.RawSourceVolley);
		}
		else
		{
			projectileVolleyData.projectiles = new List<ProjectileModule>();
			projectileVolleyData.projectiles.Add(ProjectileModule.CreateClone(sourceGun.singleModule));
			projectileVolleyData.BeamRotationDegreesPerSecond = float.MaxValue;
		}
		if (mergeGun.RawSourceVolley != null)
		{
			for (int i = 0; i < mergeGun.RawSourceVolley.projectiles.Count; i++)
			{
				ProjectileModule projectileModule = ProjectileModule.CreateClone(mergeGun.RawSourceVolley.projectiles[i]);
				projectileModule.IsDuctTapeModule = true;
				projectileModule.ignoredForReloadPurposes = projectileModule.ammoCost <= 0 || projectileModule.numberOfShotsInClip <= 0;
				projectileVolleyData.projectiles.Add(projectileModule);
				if (!string.IsNullOrEmpty(mergeGun.gunSwitchGroup) && i == 0)
				{
					projectileModule.runtimeGuid = ((projectileModule.runtimeGuid == null) ? Guid.NewGuid().ToString() : projectileModule.runtimeGuid);
					sourceGun.AdditionalShootSoundsByModule.Add(projectileModule.runtimeGuid, mergeGun.gunSwitchGroup);
				}
				if (mergeGun.RawSourceVolley.projectiles[i].runtimeGuid != null && mergeGun.AdditionalShootSoundsByModule.ContainsKey(mergeGun.RawSourceVolley.projectiles[i].runtimeGuid))
				{
					sourceGun.AdditionalShootSoundsByModule.Add(mergeGun.RawSourceVolley.projectiles[i].runtimeGuid, mergeGun.AdditionalShootSoundsByModule[mergeGun.RawSourceVolley.projectiles[i].runtimeGuid]);
				}
			}
		}
		else
		{
			ProjectileModule projectileModule2 = ProjectileModule.CreateClone(mergeGun.singleModule);
			projectileModule2.IsDuctTapeModule = true;
			projectileModule2.ignoredForReloadPurposes = projectileModule2.ammoCost <= 0 || projectileModule2.numberOfShotsInClip <= 0;
			projectileVolleyData.projectiles.Add(projectileModule2);
			if (!string.IsNullOrEmpty(mergeGun.gunSwitchGroup))
			{
				projectileModule2.runtimeGuid = ((projectileModule2.runtimeGuid == null) ? Guid.NewGuid().ToString() : projectileModule2.runtimeGuid);
				sourceGun.AdditionalShootSoundsByModule.Add(projectileModule2.runtimeGuid, mergeGun.gunSwitchGroup);
			}
		}
		return projectileVolleyData;
	}

	protected static void ReconfigureVolley(ProjectileVolleyData newVolley)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num = 0;
		for (int i = 0; i < newVolley.projectiles.Count; i++)
		{
			if (newVolley.projectiles[i].shootStyle == ProjectileModule.ShootStyle.Automatic)
			{
				flag = true;
			}
			if (newVolley.projectiles[i].shootStyle == ProjectileModule.ShootStyle.Beam)
			{
				flag = true;
			}
			if (newVolley.projectiles[i].shootStyle == ProjectileModule.ShootStyle.Burst)
			{
				flag4 = true;
			}
			if (newVolley.projectiles[i].shootStyle == ProjectileModule.ShootStyle.Charged)
			{
				flag3 = true;
				num++;
			}
		}
		if ((!flag && !flag2 && !flag3 && !flag4) || (!flag && !flag2 && !flag3 && flag4))
		{
			return;
		}
		int num2 = 0;
		for (int j = 0; j < newVolley.projectiles.Count; j++)
		{
			if (newVolley.projectiles[j].shootStyle == ProjectileModule.ShootStyle.SemiAutomatic)
			{
				newVolley.projectiles[j].shootStyle = ProjectileModule.ShootStyle.Automatic;
			}
			if (newVolley.projectiles[j].shootStyle == ProjectileModule.ShootStyle.Charged && num > 1)
			{
				num2++;
				if (num <= 1)
				{
				}
			}
		}
	}

	protected Gun GetValidGun(PlayerController user, Gun excluded = null)
	{
		int num = user.inventory.AllGuns.IndexOf(user.CurrentGun);
		if (num < 0)
		{
			num = 0;
		}
		for (int i = num; i < num + user.inventory.AllGuns.Count; i++)
		{
			int index = i % user.inventory.AllGuns.Count;
			Gun gun = user.inventory.AllGuns[index];
			if (!gun.InfiniteAmmo && gun.CanActuallyBeDropped(user) && !(gun == excluded))
			{
				return gun;
			}
		}
		return null;
	}

	protected bool IsGunValid(Gun g, Gun excluded)
	{
		if (g.InfiniteAmmo || !g.CanActuallyBeDropped(g.CurrentOwner as PlayerController))
		{
			return false;
		}
		if (g == excluded)
		{
			return false;
		}
		return true;
	}

	public static void DuctTapeGuns(Gun merged, Gun target)
	{
		ProjectileVolleyData projectileVolleyData = CombineVolleys(target, merged);
		ReconfigureVolley(projectileVolleyData);
		target.RawSourceVolley = projectileVolleyData;
		target.SetBaseMaxAmmo(target.GetBaseMaxAmmo() + merged.GetBaseMaxAmmo());
		target.GainAmmo(merged.CurrentAmmo);
		if (target.DuctTapeMergedGunIDs == null)
		{
			target.DuctTapeMergedGunIDs = new List<int>();
		}
		if (merged.DuctTapeMergedGunIDs != null)
		{
			target.DuctTapeMergedGunIDs.AddRange(merged.DuctTapeMergedGunIDs);
		}
		target.DuctTapeMergedGunIDs.Add(merged.PickupObjectId);
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		if ((bool)user && (bool)user.CurrentGun && IsGunValid(user.CurrentGun, m_validSourceGun))
		{
			m_validTargetGun = user.CurrentGun;
			if (!m_validSourceGun || !m_validTargetGun)
			{
				return;
			}
			DuctTapeGuns(m_validSourceGun, m_validTargetGun);
			user.inventory.RemoveGunFromInventory(m_validSourceGun);
			UnityEngine.Object.Destroy(m_validSourceGun.gameObject);
			user.stats.RecalculateStats(user);
		}
		m_isCurrentlyActive = false;
	}

	protected override void DoEffect(PlayerController user)
	{
		if (user.inventory.AllGuns.Count >= 2)
		{
			m_validSourceGun = null;
			m_validTargetGun = null;
			if ((bool)user && (bool)user.CurrentGun && IsGunValid(user.CurrentGun, m_validSourceGun))
			{
				m_validSourceGun = user.CurrentGun;
				m_isCurrentlyActive = true;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
