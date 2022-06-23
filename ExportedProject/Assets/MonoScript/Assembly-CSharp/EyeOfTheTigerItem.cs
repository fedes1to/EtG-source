using System;
using System.Collections;
using UnityEngine;

public class EyeOfTheTigerItem : PassiveItem
{
	public float HealthThreshold = 1f;

	public float ChanceToIgnoreDamage = 0.5f;

	public float DamageMultiplier = 2f;

	public bool ModifiesDodgeRoll;

	public float DodgeRollTimeMultiplier = 1f;

	public float DodgeRollDistanceMultiplier = 1f;

	public bool DoesFullHeal;

	public GameObject OnIgnoredDamageVFX;

	public bool HasTimedStatSynergyBuffOnKill;

	[ShowInInspectorIf("HasTimedStatSynergyBuffOnKill", false)]
	public TimedSynergyStatBuff KillTimedSynergyBuff;

	private float m_remainingKillModifierTime;

	private StatModifier m_temporaryKillStatModifier;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			HealthHaver obj = player.healthHaver;
			obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Combine(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(ModifyIncomingDamage));
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
			if (ModifiesDodgeRoll)
			{
				player.rollStats.rollTimeMultiplier *= DodgeRollTimeMultiplier;
				player.rollStats.rollDistanceMultiplier *= DodgeRollDistanceMultiplier;
			}
			player.OnKilledEnemy += HandleKilledEnemy;
			if (!m_pickedUpThisRun && DoesFullHeal)
			{
				player.healthHaver.FullHeal();
			}
			base.Pickup(player);
		}
	}

	private void HandleKilledEnemy(PlayerController targetPlayer)
	{
		if (HasTimedStatSynergyBuffOnKill && targetPlayer.HasActiveBonusSynergy(KillTimedSynergyBuff.RequiredSynergy))
		{
			if (m_temporaryKillStatModifier == null)
			{
				m_temporaryKillStatModifier = new StatModifier();
				m_temporaryKillStatModifier.statToBoost = KillTimedSynergyBuff.statToBoost;
				m_temporaryKillStatModifier.modifyType = KillTimedSynergyBuff.modifyType;
				m_temporaryKillStatModifier.amount = KillTimedSynergyBuff.amount;
			}
			if (m_remainingKillModifierTime <= 0f)
			{
				targetPlayer.StartCoroutine(HandleTimedKillStatBoost(targetPlayer));
			}
			m_remainingKillModifierTime = KillTimedSynergyBuff.duration;
		}
	}

	private IEnumerator HandleTimedKillStatBoost(PlayerController target)
	{
		target.ownerlessStatModifiers.Add(m_temporaryKillStatModifier);
		target.stats.RecalculateStats(target);
		GameObject buffVFXPrefab = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Buff_Status");
		GameObject spawnedVFX = target.PlayEffectOnActor(buffVFXPrefab, new Vector3(0f, 1.25f, 0f));
		yield return null;
		while (m_remainingKillModifierTime > 0f)
		{
			m_remainingKillModifierTime -= BraveTime.DeltaTime;
			yield return null;
		}
		SpawnManager.Despawn(spawnedVFX.gameObject);
		target.ownerlessStatModifiers.Remove(m_temporaryKillStatModifier);
		target.stats.RecalculateStats(target);
	}

	private void PostProcessBeam(BeamController obj)
	{
		obj.projectile.baseData.damage *= DamageMultiplier;
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		obj.baseData.damage *= DamageMultiplier;
	}

	private void ModifyIncomingDamage(HealthHaver source, HealthHaver.ModifyDamageEventArgs args)
	{
		if (args != EventArgs.Empty && source.GetCurrentHealth() <= HealthThreshold && UnityEngine.Random.value < ChanceToIgnoreDamage)
		{
			if (OnIgnoredDamageVFX != null)
			{
				source.GetComponent<PlayerController>().PlayEffectOnActor(OnIgnoredDamageVFX, Vector3.zero);
			}
			args.ModifiedDamage = 0f;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		HealthHaver obj = player.healthHaver;
		obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Remove(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(ModifyIncomingDamage));
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		player.OnKilledEnemy -= HandleKilledEnemy;
		if (ModifiesDodgeRoll)
		{
			player.rollStats.rollTimeMultiplier /= DodgeRollTimeMultiplier;
			player.rollStats.rollDistanceMultiplier /= DodgeRollDistanceMultiplier;
		}
		debrisObject.GetComponent<EyeOfTheTigerItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp)
		{
			HealthHaver obj = m_owner.healthHaver;
			obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Combine(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(ModifyIncomingDamage));
			m_owner.PostProcessProjectile -= PostProcessProjectile;
			m_owner.PostProcessBeam -= PostProcessBeam;
			m_owner.OnKilledEnemy -= HandleKilledEnemy;
			if (ModifiesDodgeRoll)
			{
				m_owner.rollStats.rollTimeMultiplier /= DodgeRollTimeMultiplier;
				m_owner.rollStats.rollDistanceMultiplier /= DodgeRollDistanceMultiplier;
			}
		}
		base.OnDestroy();
	}
}
