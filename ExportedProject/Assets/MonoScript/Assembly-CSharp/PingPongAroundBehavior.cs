using System;
using UnityEngine;

public class PingPongAroundBehavior : MovementBehaviorBase
{
	public enum MotionType
	{
		Diagonals = 10,
		Horizontal = 20,
		Vertical = 30
	}

	public float[] startingAngles = new float[4] { 45f, 135f, 225f, 315f };

	public MotionType motionType = MotionType.Diagonals;

	private bool m_isBouncing;

	private float m_startingAngle;

	private bool ReflectX
	{
		get
		{
			return motionType == MotionType.Diagonals || motionType == MotionType.Horizontal;
		}
	}

	private bool ReflectY
	{
		get
		{
			return motionType == MotionType.Diagonals || motionType == MotionType.Vertical;
		}
	}

	public override void Start()
	{
		base.Start();
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
	}

	public override BehaviorResult Update()
	{
		m_startingAngle = BraveMathCollege.ClampAngle360(BraveUtility.RandomElement(startingAngles));
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_startingAngle, m_aiActor.MovementSpeed);
		m_isBouncing = true;
		return BehaviorResult.RunContinuousInClass;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		return (!m_aiActor.BehaviorOverridesVelocity) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_isBouncing = false;
	}

	protected virtual void OnCollision(CollisionData collision)
	{
		if (m_isBouncing && (!collision.OtherRigidbody || !collision.OtherRigidbody.projectile) && (collision.CollidedX || collision.CollidedY))
		{
			Vector2 velocity = collision.MyRigidbody.Velocity;
			if (collision.CollidedX && ReflectX)
			{
				velocity.x *= -1f;
			}
			if (collision.CollidedY && ReflectY)
			{
				velocity.y *= -1f;
			}
			if (motionType == MotionType.Horizontal)
			{
				velocity.y = 0f;
			}
			if (motionType == MotionType.Vertical)
			{
				velocity.x = 0f;
			}
			velocity = velocity.normalized * m_aiActor.MovementSpeed;
			PhysicsEngine.PostSliceVelocity = velocity;
			m_aiActor.BehaviorVelocity = velocity;
		}
	}
}
