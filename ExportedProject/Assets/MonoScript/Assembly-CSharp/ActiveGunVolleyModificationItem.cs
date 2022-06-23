using System.Collections;
using UnityEngine;

public class ActiveGunVolleyModificationItem : PlayerItem
{
	public float duration = 5f;

	[Header("Gun Mod Settings")]
	public bool AddsModule;

	[ShowInInspectorIf("AddsModule", false)]
	public ProjectileModule ModuleToAdd;

	public int DuplicatesOfBaseModule;

	public float DuplicateAngleOffset = 10f;

	[LongNumericEnum]
	public CustomSynergyType[] SynergiesToIncrementDuplicates;

	private PlayerController m_cachedUser;

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_power_up_01", base.gameObject);
		m_cachedUser = user;
		StartCoroutine(HandleDuration(user));
	}

	private IEnumerator HandleDuration(PlayerController user)
	{
		bool wasFiring = false;
		if (user.CurrentGun != null && user.CurrentGun.IsFiring)
		{
			user.CurrentGun.CeaseAttack();
			wasFiring = true;
		}
		base.IsCurrentlyActive = true;
		user.stats.RecalculateStats(user);
		m_activeElapsed = 0f;
		m_activeDuration = duration;
		if (wasFiring)
		{
			user.CurrentGun.Attack();
			for (int i = 0; i < user.CurrentGun.ActiveBeams.Count; i++)
			{
				if (user.CurrentGun.ActiveBeams[i] != null && user.CurrentGun.ActiveBeams[i].beam is BasicBeamController)
				{
					(user.CurrentGun.ActiveBeams[i].beam as BasicBeamController).ForceChargeTimer(10f);
				}
			}
		}
		while (m_activeElapsed < m_activeDuration && base.IsCurrentlyActive)
		{
			yield return null;
		}
		bool wasEndFiring = user.CurrentGun != null && user.CurrentGun.IsFiring;
		if (wasEndFiring)
		{
			user.CurrentGun.CeaseAttack();
		}
		base.IsCurrentlyActive = false;
		user.stats.RecalculateStats(user);
		if (!wasEndFiring)
		{
			yield break;
		}
		user.CurrentGun.Attack();
		for (int j = 0; j < user.CurrentGun.ActiveBeams.Count; j++)
		{
			if (user.CurrentGun.ActiveBeams[j] != null && user.CurrentGun.ActiveBeams[j].beam is BasicBeamController)
			{
				(user.CurrentGun.ActiveBeams[j].beam as BasicBeamController).ForceChargeTimer(10f);
			}
		}
	}

	public void ModifyVolley(ProjectileVolleyData volleyToModify)
	{
		if (AddsModule)
		{
			ModuleToAdd.isExternalAddedModule = true;
			volleyToModify.projectiles.Add(ModuleToAdd);
		}
		int num = DuplicatesOfBaseModule;
		for (int i = 0; i < SynergiesToIncrementDuplicates.Length; i++)
		{
			if ((bool)LastOwner && LastOwner.HasActiveBonusSynergy(SynergiesToIncrementDuplicates[i]))
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return;
		}
		int count = volleyToModify.projectiles.Count;
		for (int j = 0; j < count; j++)
		{
			ProjectileModule projectileModule = volleyToModify.projectiles[j];
			float num2 = (float)num * DuplicateAngleOffset * -1f / 2f;
			for (int k = 0; k < num; k++)
			{
				int sourceIndex = j;
				if (projectileModule.CloneSourceIndex >= 0)
				{
					sourceIndex = projectileModule.CloneSourceIndex;
				}
				ProjectileModule projectileModule2 = ProjectileModule.CreateClone(projectileModule, false, sourceIndex);
				float num3 = (projectileModule2.angleFromAim = num2 + DuplicateAngleOffset * (float)k);
				projectileModule2.ignoredForReloadPurposes = true;
				projectileModule2.ammoCost = 0;
				volleyToModify.projectiles.Add(projectileModule2);
			}
		}
	}

	public override void OnItemSwitched(PlayerController user)
	{
		base.OnItemSwitched(user);
		base.IsCurrentlyActive = false;
		if (m_cachedUser != null)
		{
			m_cachedUser.stats.RecalculateStats(m_cachedUser);
		}
	}

	protected override void OnDestroy()
	{
		base.IsCurrentlyActive = false;
		if ((bool)m_cachedUser)
		{
			m_cachedUser.stats.RecalculateStats(m_cachedUser);
		}
		base.OnDestroy();
	}
}
