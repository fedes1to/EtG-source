using System;
using System.Collections.ObjectModel;
using FullInspector;
using UnityEngine;

public class DestroyBulletsBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		WaitingForTell,
		Blanking
	}

	public float SkippableCooldown;

	public float SkippableRadius;

	public float Radius;

	public float BlankTime;

	public AnimationCurve RadiusCurve;

	[InspectorCategory("Visuals")]
	public string TellAnimation;

	[InspectorCategory("Visuals")]
	public string BlankAnimation;

	[InspectorCategory("Visuals")]
	public string BlankVfx;

	[InspectorCategory("Visuals")]
	public GameObject OverrideHitVfx;

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
		if (!(m_behaviorSpeculator.AttackCooldown <= 0f) || !(m_behaviorSpeculator.GlobalCooldown <= 0f) || !(m_cooldownTimer < SkippableCooldown))
		{
			return;
		}
		bool flag = false;
		Vector2 unitCenter = m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter;
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int i = 0; i < allProjectiles.Count; i++)
		{
			Projectile projectile = allProjectiles[i];
			if (projectile.Owner is PlayerController && (bool)projectile.specRigidbody && !(Vector2.Distance(unitCenter, projectile.specRigidbody.UnitCenter) > SkippableRadius))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			m_cooldownTimer = 0f;
		}
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
			StartBlanking();
		}
		m_aiActor.ClearPath();
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(true, "DestroyBulletsBehavior");
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
				StartBlanking();
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (m_state == State.Blanking)
		{
			Vector2 unitCenter = m_aiActor.specRigidbody.HitboxPixelCollider.UnitCenter;
			float num = RadiusCurve.Evaluate(Mathf.InverseLerp(BlankTime, 0f, m_timer)) * Radius;
			ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
			for (int i = 0; i < allProjectiles.Count; i++)
			{
				Projectile projectile = allProjectiles[i];
				if (projectile.Owner is PlayerController && (bool)projectile.specRigidbody && !(Vector2.Distance(unitCenter, projectile.specRigidbody.UnitCenter) > num))
				{
					if ((bool)OverrideHitVfx)
					{
						projectile.hitEffects.overrideMidairDeathVFX = OverrideHitVfx;
					}
					projectile.DieInAir();
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
		if (!string.IsNullOrEmpty(BlankAnimation))
		{
			m_aiAnimator.EndAnimationIf(BlankAnimation);
		}
		if (!string.IsNullOrEmpty(BlankVfx))
		{
			m_aiAnimator.StopVfx(BlankVfx);
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "DestroyBulletsBehavior");
		}
		m_state = State.Idle;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (m_state == State.WaitingForTell && frame.eventInfo == "blank")
		{
			StartBlanking();
		}
	}

	private void StartBlanking()
	{
		if (!string.IsNullOrEmpty(BlankAnimation))
		{
			m_aiAnimator.PlayUntilFinished(BlankAnimation);
		}
		if (!string.IsNullOrEmpty(BlankVfx))
		{
			m_aiAnimator.PlayVfx(BlankVfx);
		}
		m_timer = BlankTime;
		m_state = State.Blanking;
	}
}
