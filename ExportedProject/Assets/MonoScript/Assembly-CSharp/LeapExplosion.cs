using Dungeonator;
using UnityEngine;

public class LeapExplosion : AttackBehaviorBase
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

	public string chargeAnim;

	public string leapAnim;

	private PixelCollider m_enemyCollider;

	private float m_elapsed;

	private State m_state;

	public override void Start()
	{
		base.Start();
		for (int i = 0; i < m_aiActor.specRigidbody.PixelColliders.Count; i++)
		{
			PixelCollider pixelCollider = m_aiActor.specRigidbody.PixelColliders[i];
			if (pixelCollider.CollisionLayer == CollisionLayer.EnemyCollider)
			{
				m_enemyCollider = pixelCollider;
			}
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

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
			m_aiAnimator.PlayUntilFinished(chargeAnim, true);
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
				if (!m_aiActor.TargetRigidbody)
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
				m_enemyCollider.CollisionLayer = CollisionLayer.TileBlocker;
				m_aiActor.specRigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.BulletBlocker, CollisionLayer.EnemyBulletBlocker));
				m_aiActor.DoDustUps = false;
				m_aiAnimator.PlayUntilCancelled(leapAnim, true);
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
		return leapDistance;
	}

	public override float GetMaxRange()
	{
		return leapDistance;
	}
}
