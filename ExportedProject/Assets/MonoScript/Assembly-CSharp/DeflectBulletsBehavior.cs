using System;
using System.Collections.ObjectModel;
using FullInspector;
using UnityEngine;

public class DeflectBulletsBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		WaitingForTell,
		Deflecting
	}

	public float Radius;

	public float DeflectTime;

	public AnimationCurve RadiusCurve;

	public float force = 10f;

	[InspectorCategory("Visuals")]
	public string TellAnimation;

	[InspectorCategory("Visuals")]
	public string DeflectAnimation;

	[InspectorCategory("Visuals")]
	public string DeflectVfx;

	private State m_state;

	private float m_timer;

	public override void Start()
	{
		base.Start();
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
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
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			if (!string.IsNullOrEmpty(TellAnimation))
			{
				m_aiAnimator.PlayUntilFinished(TellAnimation);
			}
			m_state = State.WaitingForTell;
		}
		else
		{
			StartDeflecting();
		}
		m_aiActor.ClearPath();
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(true, "DeflectBulletsBehavior");
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == State.WaitingForTell)
		{
			if (!m_aiAnimator.IsPlaying(TellAnimation))
			{
				StartDeflecting();
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Deflecting)
		{
			Vector2 unitCenter = m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter;
			float num = RadiusCurve.Evaluate(Mathf.InverseLerp(DeflectTime, 0f, m_timer)) * Radius;
			ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
			for (int i = 0; i < allProjectiles.Count; i++)
			{
				Projectile projectile = allProjectiles[i];
				if (projectile.Owner is PlayerController && (bool)projectile.specRigidbody && !(Vector2.Distance(unitCenter, projectile.specRigidbody.UnitCenter) > num))
				{
					AdjustProjectileVelocity(projectile, unitCenter, num);
				}
			}
			if (m_timer <= 0f)
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			m_aiAnimator.EndAnimationIf(TellAnimation);
		}
		if (!string.IsNullOrEmpty(DeflectAnimation))
		{
			m_aiAnimator.EndAnimationIf(DeflectAnimation);
		}
		if (!string.IsNullOrEmpty(DeflectVfx))
		{
			m_aiAnimator.StopVfx(DeflectVfx);
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "DeflectBulletsBehavior");
		}
		m_state = State.Idle;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (m_state == State.WaitingForTell && frame.eventInfo == "deflect")
		{
			StartDeflecting();
		}
	}

	private void StartDeflecting()
	{
		if (!string.IsNullOrEmpty(DeflectAnimation))
		{
			m_aiAnimator.PlayUntilFinished(DeflectAnimation);
		}
		if (!string.IsNullOrEmpty(DeflectVfx))
		{
			m_aiAnimator.PlayVfx(DeflectVfx);
		}
		m_timer = DeflectTime;
		m_state = State.Deflecting;
	}

	private void AdjustProjectileVelocity(Projectile p, Vector2 deflectCenter, float deflectRadius)
	{
		Vector2 a = p.specRigidbody.UnitCenter - deflectCenter;
		float f = Vector2.SqrMagnitude(a);
		Vector2 velocity = p.specRigidbody.Velocity;
		if (velocity == Vector2.zero)
		{
			return;
		}
		float num = Mathf.Lerp(1f, 0.5f, Mathf.Sqrt(f) / deflectRadius);
		Vector2 vector = a.normalized * (force * velocity.magnitude * num * num);
		Vector2 vector2 = vector * Mathf.Clamp(BraveTime.DeltaTime, 0f, 0.02f);
		Vector2 vector3 = velocity + vector2;
		if (BraveTime.DeltaTime > 0.02f)
		{
			vector3 *= 0.02f / BraveTime.DeltaTime;
		}
		p.specRigidbody.Velocity = vector3;
		if (!(vector3 != Vector2.zero))
		{
			return;
		}
		p.Direction = vector3.normalized;
		p.Speed = velocity.magnitude;
		p.specRigidbody.Velocity = p.Direction * p.Speed;
		if (!p.shouldRotate || (vector3.x == 0f && vector3.y == 0f))
		{
			return;
		}
		float num2 = BraveMathCollege.Atan2Degrees(p.Direction);
		if (!float.IsNaN(num2) && !float.IsInfinity(num2))
		{
			Quaternion rotation = Quaternion.Euler(0f, 0f, num2);
			if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y))
			{
				p.transform.rotation = rotation;
			}
		}
	}
}
