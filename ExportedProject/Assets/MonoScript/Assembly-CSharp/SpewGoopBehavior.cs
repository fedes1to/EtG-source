using System;
using UnityEngine;

public class SpewGoopBehavior : BasicAttackBehavior
{
	public string spewAnimation;

	public Transform goopPoint;

	public GoopDefinition goopToUse;

	public float goopConeLength = 5f;

	public float goopConeArc = 45f;

	public AnimationCurve goopCurve;

	public float goopDuration = 0.5f;

	private float m_goopTimer;

	private bool m_hasGooped;

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_goopTimer);
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
		m_aiActor.ClearPath();
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_hasGooped = false;
		m_aiAnimator.PlayUntilFinished(spewAnimation);
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_hasGooped || m_goopTimer > 0f)
		{
			return ContinuousBehaviorResult.Continue;
		}
		return ContinuousBehaviorResult.Finished;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiAnimator.EndAnimationIf(spewAnimation);
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (!m_hasGooped && clip.GetFrame(frame).eventInfo == "spew")
		{
			DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopToUse);
			goopManagerForGoopType.TimedAddGoopArc(goopPoint.transform.position, goopConeLength, goopConeArc, goopPoint.transform.right, goopDuration, goopCurve);
			m_goopTimer = goopDuration;
			m_hasGooped = true;
		}
	}
}
