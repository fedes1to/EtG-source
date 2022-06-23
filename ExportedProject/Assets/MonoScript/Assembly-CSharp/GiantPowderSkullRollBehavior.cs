using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/RollBehavior")]
public class GiantPowderSkullRollBehavior : BasicAttackBehavior
{
	private enum RollState
	{
		None,
		Charge,
		Rolling,
		Stopping
	}

	public float[] startingAngles = new float[4] { 45f, 135f, 225f, 315f };

	public float rollSpeed = 9f;

	public int numBounces = 3;

	public BulletScriptSelector collisionBulletScript;

	[InspectorCategory("Visuals")]
	public PowderSkullParticleController trailParticleSystem;

	private RollState m_state;

	private float m_cachedVelocityFraction;

	private float m_timeSinceLastBounce;

	private int m_bounces;

	private float m_startingAngle;

	public override void Start()
	{
		base.Start();
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_timeSinceLastBounce += m_deltaTime;
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_startingAngle = BraveMathCollege.ClampAngle360(BraveUtility.RandomElement(startingAngles));
		m_bounces = 0;
		if ((bool)trailParticleSystem)
		{
			m_cachedVelocityFraction = trailParticleSystem.VelocityFraction;
			trailParticleSystem.VelocityFraction = 0f;
		}
		m_state = RollState.Charge;
		m_aiAnimator.LockFacingDirection = true;
		m_aiAnimator.FacingDirection = m_startingAngle;
		m_aiAnimator.PlayUntilFinished("roll_charge");
		m_aiActor.ClearPath();
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_aiActor.specRigidbody.PixelColliders[0].Enabled = false;
		m_aiActor.specRigidbody.PixelColliders[1].Enabled = false;
		m_aiActor.specRigidbody.PixelColliders[3].Enabled = true;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == RollState.Charge)
		{
			if (!m_aiAnimator.IsPlaying("roll_charge"))
			{
				m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_startingAngle, rollSpeed);
				m_aiAnimator.FacingDirection = m_aiActor.BehaviorVelocity.ToAngle();
				m_state = RollState.Rolling;
				m_aiAnimator.PlayUntilCancelled("roll");
			}
		}
		else if (m_state != RollState.Rolling && m_state == RollState.Stopping && !m_aiAnimator.IsPlaying("roll_out"))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiAnimator.LockFacingDirection = false;
		if ((bool)trailParticleSystem)
		{
			trailParticleSystem.VelocityFraction = m_cachedVelocityFraction;
		}
		m_aiActor.specRigidbody.PixelColliders[0].Enabled = true;
		m_aiActor.specRigidbody.PixelColliders[1].Enabled = true;
		m_aiActor.specRigidbody.PixelColliders[3].Enabled = false;
		m_state = RollState.None;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	protected virtual void OnCollision(CollisionData collision)
	{
		if (m_state != RollState.Rolling || (bool)collision.OtherRigidbody)
		{
			return;
		}
		if (m_timeSinceLastBounce > 1f)
		{
			m_bounces++;
			SpawnManager.SpawnBulletScript(m_aiActor, collisionBulletScript);
		}
		AkSoundEngine.PostEvent("Play_ENM_statue_stomp_01", m_aiActor.gameObject);
		if (m_bounces > numBounces)
		{
			PhysicsEngine.PostSliceVelocity = Vector2.zero;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_state = RollState.Stopping;
			m_aiAnimator.PlayUntilFinished("roll_stop");
			return;
		}
		Vector2 velocity = collision.MyRigidbody.Velocity;
		if (collision.CollidedX)
		{
			velocity.x *= -1f;
		}
		if (collision.CollidedY)
		{
			velocity.y *= -1f;
		}
		velocity = velocity.normalized * rollSpeed;
		m_aiAnimator.FacingDirection = velocity.ToAngle();
		PhysicsEngine.PostSliceVelocity = velocity;
		m_aiActor.BehaviorVelocity = velocity;
		m_timeSinceLastBounce = 0f;
	}
}
