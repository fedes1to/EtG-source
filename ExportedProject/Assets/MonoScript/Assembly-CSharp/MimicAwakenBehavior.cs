using System;
using UnityEngine;

public class MimicAwakenBehavior : BasicAttackBehavior
{
	public string awakenAnim;

	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	private bool m_hasFired;

	private BulletScriptSource m_bulletScriptSource;

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_aiActor.HasBeenEngaged = true;
		m_aiActor.CollisionDamage = 0f;
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_hasFired)
		{
			return BehaviorResult.Continue;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.GetComponent<WallMimicController>())
		{
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.FacingDirection = -90f;
		}
		m_aiActor.ClearPath();
		m_aiAnimator.PlayUntilFinished(awakenAnim, true);
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_aiAnimator.IsPlaying(awakenAnim))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiAnimator.LockFacingDirection = false;
		m_aiActor.CollisionDamage = 0.5f;
		m_aiActor.knockbackDoer.weight = 35f;
		m_hasFired = true;
		tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
	}

	private void ShootBulletScript()
	{
		if (!m_bulletScriptSource)
		{
			m_bulletScriptSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletScriptSource.BulletManager = m_aiActor.bulletBank;
		m_bulletScriptSource.BulletScript = BulletScript;
		m_bulletScriptSource.Initialize();
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (frame.eventInfo == "fire")
		{
			ShootBulletScript();
		}
	}
}
