using System;
using UnityEngine;

public class BabyGoodMimicAttackBehavior : BasicAttackBehavior
{
	public string AttackAnimationName = "attack";

	public ProjectileVolleyData Volley;

	public float TimeBetweenAttacks = 0.25f;

	public int NumberOfAttacks = 10;

	public VFXPool ShootVFX;

	private bool m_wasDamaged;

	private float m_continuousShotTimer;

	private float m_continuousElapsed;

	public override void Start()
	{
		base.Start();
		HealthHaver healthHaver = m_aiActor.healthHaver;
		healthHaver.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Combine(healthHaver.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(ModifyIncomingDamage));
	}

	private void ModifyIncomingDamage(HealthHaver health, HealthHaver.ModifyDamageEventArgs damageArgs)
	{
		m_wasDamaged = true;
		damageArgs.ModifiedDamage = 0f;
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if ((bool)m_aiActor && (bool)m_aiAnimator && (bool)m_aiActor.CompanionOwner && m_aiActor.CompanionOwner.IsInCombat)
		{
			m_aiAnimator.OverrideIdleAnimation = "mimic";
		}
		else if ((bool)m_aiAnimator)
		{
			m_aiAnimator.OverrideIdleAnimation = string.Empty;
		}
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_wasDamaged)
		{
			return BehaviorResult.Continue;
		}
		m_wasDamaged = false;
		UpdateCooldowns();
		m_continuousElapsed = 0f;
		m_continuousShotTimer = 0f;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_continuousElapsed > (float)NumberOfAttacks * TimeBetweenAttacks)
		{
			return ContinuousBehaviorResult.Finished;
		}
		m_continuousShotTimer -= BraveTime.DeltaTime;
		if (m_continuousShotTimer <= 0f)
		{
			Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
			m_aiAnimator.FacingDirection = normalized.ToAngle();
			if (m_aiAnimator != null)
			{
				m_aiAnimator.PlayUntilFinished(AttackAnimationName, true);
			}
			ShootVFX.SpawnAtPosition(m_aiActor.CenterPosition, normalized.ToAngle());
			VolleyUtility.FireVolley(Volley, m_aiActor.CenterPosition, normalized, m_aiActor.CompanionOwner, true);
			m_continuousShotTimer += TimeBetweenAttacks;
		}
		m_continuousElapsed += BraveTime.DeltaTime;
		return base.ContinuousUpdate();
	}

	public override void EndContinuousUpdate()
	{
		m_updateEveryFrame = false;
		m_continuousShotTimer = 0f;
		m_continuousElapsed = 0f;
		if ((bool)m_aiAnimator)
		{
			m_aiAnimator.EndAnimationIf(AttackAnimationName);
		}
		base.EndContinuousUpdate();
	}
}
