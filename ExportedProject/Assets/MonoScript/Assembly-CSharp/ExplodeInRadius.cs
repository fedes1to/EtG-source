using UnityEngine;

public class ExplodeInRadius : AttackBehaviorBase
{
	public float explodeDistance = 1f;

	public float explodeCountDown;

	public bool stopMovement;

	public float minLifetime;

	protected float m_closeEnoughToExplodeTimer;

	protected float m_explodeTime;

	protected float m_lifetime;

	protected float m_elapsed;

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimationClip clipByName = m_gameObject.GetComponent<tk2dSpriteAnimator>().GetClipByName("explode");
		if (clipByName != null)
		{
			m_explodeTime = (float)clipByName.frames.Length / clipByName.fps;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (minLifetime > 0f)
		{
			m_lifetime += m_deltaTime;
		}
	}

	public override BehaviorResult Update()
	{
		if (m_aiActor.healthHaver.IsDead)
		{
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		if (minLifetime > 0f && m_lifetime < minLifetime)
		{
			return BehaviorResult.Continue;
		}
		if (m_aiActor.TargetRigidbody != null && m_aiActor.DistanceToTarget < explodeDistance)
		{
			m_closeEnoughToExplodeTimer += m_deltaTime;
			if (m_closeEnoughToExplodeTimer > explodeCountDown)
			{
				m_aiAnimator.PlayForDuration("explode", m_explodeTime);
				if (stopMovement)
				{
					m_aiActor.ClearPath();
				}
				m_updateEveryFrame = true;
				return BehaviorResult.RunContinuous;
			}
		}
		else
		{
			m_closeEnoughToExplodeTimer = 0f;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_elapsed < m_explodeTime)
		{
			m_elapsed += m_deltaTime;
			return ContinuousBehaviorResult.Continue;
		}
		return ContinuousBehaviorResult.Finished;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (m_aiActor.healthHaver.PreventAllDamage)
		{
			m_aiActor.healthHaver.PreventAllDamage = false;
		}
		ExplodeOnDeath component = m_aiActor.GetComponent<ExplodeOnDeath>();
		if ((bool)component && component.LinearChainExplosion)
		{
			component.ChainIsReversed = false;
			component.explosionData.damage = 5f;
		}
		if (m_aiActor.healthHaver.IsAlive)
		{
			m_aiActor.healthHaver.ApplyDamage(float.MaxValue, Vector2.zero, "self-immolation", CoreDamageTypes.Fire, DamageCategory.Unstoppable, true);
		}
		m_updateEveryFrame = false;
	}

	public override bool IsReady()
	{
		return true;
	}

	public override float GetMinReadyRange()
	{
		return -1f;
	}

	public override float GetMaxRange()
	{
		return -1f;
	}
}
