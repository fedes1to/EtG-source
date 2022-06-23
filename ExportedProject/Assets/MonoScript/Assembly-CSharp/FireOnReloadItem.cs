using System;
using System.Collections.Generic;
using UnityEngine;

public class FireOnReloadItem : PassiveItem
{
	public float ActivationChance = 1f;

	public float InternalCooldown = 1f;

	public bool TriggersRadialBulletBurst;

	[ShowInInspectorIf("TriggersRadialBulletBurst", false)]
	public RadialBurstInterface RadialBurstSettings;

	private float m_lastUsedTime;

	public bool IsHipHolster;

	private void Awake()
	{
		if (IsHipHolster)
		{
			RadialBurstInterface radialBurstSettings = RadialBurstSettings;
			radialBurstSettings.CustomPostProcessProjectile = (Action<Projectile>)Delegate.Combine(radialBurstSettings.CustomPostProcessProjectile, new Action<Projectile>(HandleHipHolsterProcessing));
		}
	}

	private void HandleHipHolsterProcessing(Projectile proj)
	{
		if (!base.Owner)
		{
			return;
		}
		if (base.Owner.HasActiveBonusSynergy(CustomSynergyType.DOUBLE_HOLSTER))
		{
			HomingModifier homingModifier = proj.gameObject.GetComponent<HomingModifier>();
			if (homingModifier == null)
			{
				homingModifier = proj.gameObject.AddComponent<HomingModifier>();
				homingModifier.HomingRadius = 0f;
				homingModifier.AngularVelocity = 0f;
			}
			homingModifier.HomingRadius += 20f;
			homingModifier.AngularVelocity += 1080f;
		}
		if (base.Owner.HasActiveBonusSynergy(CustomSynergyType.EXPLOSIVE_HOLSTER))
		{
			ExplosiveModifier component = proj.gameObject.GetComponent<ExplosiveModifier>();
			if (component == null)
			{
				component = proj.gameObject.AddComponent<ExplosiveModifier>();
				component.explosionData = new ExplosionData();
				component.explosionData.ignoreList = new List<SpeculativeRigidbody>();
				component.explosionData.CopyFrom(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData);
				component.explosionData.damageToPlayer = 0f;
				component.explosionData.useDefaultExplosion = false;
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Combine(player.OnReloadedGun, new Action<PlayerController, Gun>(DoEffect));
		}
	}

	private void DoEffect(PlayerController usingPlayer, Gun usedGun)
	{
		if (!(Time.realtimeSinceStartup - m_lastUsedTime < InternalCooldown) && (!usedGun || !usedGun.HasFiredHolsterShot))
		{
			usedGun.HasFiredHolsterShot = true;
			m_lastUsedTime = Time.realtimeSinceStartup;
			if (UnityEngine.Random.value < ActivationChance && TriggersRadialBulletBurst)
			{
				RadialBurstSettings.DoBurst(usingPlayer);
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		FireOnReloadItem component = debrisObject.GetComponent<FireOnReloadItem>();
		player.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Remove(player.OnReloadedGun, new Action<PlayerController, Gun>(DoEffect));
		component.m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
