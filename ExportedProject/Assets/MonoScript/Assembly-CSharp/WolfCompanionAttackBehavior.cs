using Dungeonator;
using UnityEngine;

public class WolfCompanionAttackBehavior : AttackBehaviorBase
{
	private enum State
	{
		Idle,
		Charging,
		Leaping
	}

	public float minLeapDistance = 1f;

	public float leapDistance = 4f;

	public float maxTravelDistance = 5f;

	public float leadAmount;

	public float leapTime = 0.75f;

	public float maximumChargeTime = 0.25f;

	public string chargeAnim;

	public string leapAnim;

	[LongNumericEnum]
	public CustomSynergyType DebuffSynergy;

	public AIActorDebuffEffect EnemyDebuff;

	private float m_elapsed;

	private State m_state;

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		Vector2 vector = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		if (leadAmount > 0f)
		{
			Vector2 b = vector + m_aiActor.TargetRigidbody.specRigidbody.Velocity * 0.75f;
			vector = Vector2.Lerp(vector, b, leadAmount);
		}
		float num = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, vector);
		if (num > minLeapDistance && num < leapDistance)
		{
			m_state = State.Charging;
			m_aiAnimator.PlayForDuration(chargeAnim, maximumChargeTime, true);
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_updateEveryFrame = true;
			return BehaviorResult.RunContinuous;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == State.Charging)
		{
			if (!m_aiAnimator.IsPlaying(chargeAnim))
			{
				m_state = State.Leaping;
				if (!m_aiActor.TargetRigidbody || !m_aiActor.TargetRigidbody.enabled)
				{
					m_state = State.Idle;
					return ContinuousBehaviorResult.Finished;
				}
				Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
				Vector2 vector = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				if (leadAmount > 0f)
				{
					Vector2 b = vector + m_aiActor.TargetRigidbody.specRigidbody.Velocity * 0.75f;
					vector = Vector2.Lerp(vector, b, leadAmount);
				}
				float num = Vector2.Distance(unitCenter, vector);
				if (num > maxTravelDistance)
				{
					vector = unitCenter + (vector - unitCenter).normalized * maxTravelDistance;
					num = Vector2.Distance(unitCenter, vector);
				}
				m_aiActor.ClearPath();
				m_aiActor.BehaviorOverridesVelocity = true;
				m_aiActor.BehaviorVelocity = (vector - unitCenter).normalized * (num / leapTime);
				float facingDirection = m_aiActor.BehaviorVelocity.ToAngle();
				m_aiAnimator.LockFacingDirection = true;
				m_aiAnimator.FacingDirection = facingDirection;
				m_aiActor.PathableTiles = CellTypes.FLOOR | CellTypes.PIT;
				m_aiActor.DoDustUps = false;
				m_aiAnimator.PlayUntilFinished(leapAnim, true);
			}
		}
		else if (m_state == State.Leaping)
		{
			m_elapsed += m_deltaTime;
			if (m_elapsed >= leapTime)
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if ((bool)m_aiActor.TargetRigidbody && (bool)m_aiActor.TargetRigidbody.healthHaver)
		{
			m_aiActor.TargetRigidbody.healthHaver.ApplyDamage(5f, m_aiActor.specRigidbody.Velocity, "Wolf");
			if ((bool)m_aiActor.CompanionOwner && m_aiActor.CompanionOwner.HasActiveBonusSynergy(DebuffSynergy))
			{
				m_aiActor.TargetRigidbody.aiActor.ApplyEffect(EnemyDebuff);
			}
		}
		m_state = State.Idle;
		m_aiActor.PathableTiles = CellTypes.FLOOR;
		m_aiActor.DoDustUps = true;
		m_aiActor.BehaviorOverridesVelocity = false;
		m_aiAnimator.LockFacingDirection = false;
		m_updateEveryFrame = false;
	}

	public override bool IsReady()
	{
		return true;
	}

	public override float GetMinReadyRange()
	{
		return leapDistance;
	}

	public override float GetMaxRange()
	{
		return leapDistance;
	}
}
