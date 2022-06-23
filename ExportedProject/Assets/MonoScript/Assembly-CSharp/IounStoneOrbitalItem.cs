using System;
using System.Collections;
using UnityEngine;

public class IounStoneOrbitalItem : PlayerOrbitalItem
{
	public enum IounStoneIdentifier
	{
		GENERIC,
		CLEAR
	}

	public bool SlowBulletsOnDamage;

	public float SlowBulletsDuration = 15f;

	public float SlowBulletsMultiplier = 0.5f;

	public bool ChanceToHealOnDamage;

	public float HealChanceNormal = 0.2f;

	public float HealChanceCritical = 0.5f;

	public int GreenerSynergyMoneyGain = 20;

	public float GreenerChanceCritical = 0.7f;

	public bool ModifiesDodgeRoll;

	public float DodgeRollTimeMultiplier = 0.9f;

	public float DodgeRollDistanceMultiplier = 1.25f;

	public bool SynergyCharmsEnemiesOnDamage;

	public GameActorCharmEffect CharmEffect;

	public GameActorFreezeEffect DefaultFreezeEffect;

	public IounStoneIdentifier Identifier;

	private bool m_isSlowingBullets;

	private float m_slowDurationRemaining;

	public override void Pickup(PlayerController player)
	{
		player.OnReceivedDamage += OwnerTookDamage;
		player.OnHitByProjectile = (Action<Projectile, PlayerController>)Delegate.Combine(player.OnHitByProjectile, new Action<Projectile, PlayerController>(OwnerHitByProjectile));
		if (Identifier == IounStoneIdentifier.CLEAR)
		{
			player.PostProcessProjectile += HandlePostProcessClearSynergy;
		}
		if (ModifiesDodgeRoll)
		{
			player.rollStats.rollDistanceMultiplier *= DodgeRollDistanceMultiplier;
			player.rollStats.rollTimeMultiplier *= DodgeRollTimeMultiplier;
		}
		base.Pickup(player);
	}

	private void HandlePostProcessClearSynergy(Projectile targetProjectile, float arg2)
	{
		if (!m_owner || !m_synergyUpgradeActive || !m_owner.CurrentGoop)
		{
			return;
		}
		if (m_owner.CurrentGoop.CanBeIgnited)
		{
			if (!targetProjectile.AppliesFire)
			{
				targetProjectile.AppliesFire = true;
				targetProjectile.FireApplyChance = 1f;
				targetProjectile.fireEffect = m_owner.CurrentGoop.fireEffect;
			}
		}
		else if (m_owner.CurrentGoop.CanBeFrozen && !targetProjectile.AppliesFreeze)
		{
			targetProjectile.AppliesFreeze = true;
			targetProjectile.FreezeApplyChance = 1f;
			targetProjectile.freezeEffect = DefaultFreezeEffect;
		}
	}

	private void OwnerHitByProjectile(Projectile incomingProjectile, PlayerController arg2)
	{
		if (SynergyCharmsEnemiesOnDamage && m_synergyUpgradeActive && (bool)incomingProjectile && (bool)incomingProjectile.Owner && incomingProjectile.Owner is AIActor)
		{
			incomingProjectile.Owner.ApplyEffect(CharmEffect);
		}
	}

	private void OwnerTookDamage(PlayerController obj)
	{
		if (SlowBulletsOnDamage)
		{
			if (!m_isSlowingBullets)
			{
				obj.StartCoroutine(HandleSlowBullets());
			}
			else
			{
				m_slowDurationRemaining = SlowBulletsDuration;
			}
		}
		if (!ChanceToHealOnDamage)
		{
			return;
		}
		if (obj.healthHaver.GetCurrentHealth() < 0.5f)
		{
			bool flag = obj.HasActiveBonusSynergy(CustomSynergyType.GUON_UPGRADE_GREEN);
			if (UnityEngine.Random.value < ((!flag) ? HealChanceCritical : GreenerChanceCritical))
			{
				Debug.Log("Green Guon Critical Heal");
				obj.healthHaver.ApplyHealing(0.5f);
				if (flag)
				{
					LootEngine.SpawnCurrency(obj.CenterPosition, GreenerSynergyMoneyGain);
				}
			}
		}
		else if (UnityEngine.Random.value < HealChanceNormal)
		{
			Debug.Log("Green Guon Normal Heal");
			obj.healthHaver.ApplyHealing(0.5f);
			if (obj.HasActiveBonusSynergy(CustomSynergyType.GUON_UPGRADE_GREEN))
			{
				LootEngine.SpawnCurrency(obj.CenterPosition, GreenerSynergyMoneyGain);
			}
		}
	}

	private IEnumerator HandleSlowBullets()
	{
		yield return new WaitForEndOfFrame();
		m_isSlowingBullets = true;
		float slowMultiplier = SlowBulletsMultiplier;
		Projectile.BaseEnemyBulletSpeedMultiplier *= slowMultiplier;
		m_slowDurationRemaining = SlowBulletsDuration;
		while (m_slowDurationRemaining > 0f)
		{
			yield return null;
			m_slowDurationRemaining -= BraveTime.DeltaTime;
			Projectile.BaseEnemyBulletSpeedMultiplier /= slowMultiplier;
			slowMultiplier = Mathf.Lerp(SlowBulletsMultiplier, 1f, 1f - m_slowDurationRemaining);
			Projectile.BaseEnemyBulletSpeedMultiplier *= slowMultiplier;
		}
		Projectile.BaseEnemyBulletSpeedMultiplier /= slowMultiplier;
		m_isSlowingBullets = false;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		player.OnHitByProjectile = (Action<Projectile, PlayerController>)Delegate.Remove(player.OnHitByProjectile, new Action<Projectile, PlayerController>(OwnerHitByProjectile));
		player.OnReceivedDamage -= OwnerTookDamage;
		player.PostProcessProjectile -= HandlePostProcessClearSynergy;
		if (ModifiesDodgeRoll)
		{
			player.rollStats.rollDistanceMultiplier /= DodgeRollDistanceMultiplier;
			player.rollStats.rollTimeMultiplier /= DodgeRollTimeMultiplier;
		}
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			PlayerController owner = m_owner;
			owner.OnHitByProjectile = (Action<Projectile, PlayerController>)Delegate.Remove(owner.OnHitByProjectile, new Action<Projectile, PlayerController>(OwnerHitByProjectile));
			m_owner.OnReceivedDamage -= OwnerTookDamage;
			m_owner.PostProcessProjectile -= HandlePostProcessClearSynergy;
			if (ModifiesDodgeRoll)
			{
				m_owner.rollStats.rollDistanceMultiplier /= DodgeRollDistanceMultiplier;
				m_owner.rollStats.rollTimeMultiplier /= DodgeRollTimeMultiplier;
			}
		}
		base.OnDestroy();
	}
}
