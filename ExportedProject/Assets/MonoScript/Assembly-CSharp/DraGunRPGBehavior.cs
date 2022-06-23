using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/RPGBehavior")]
public class DraGunRPGBehavior : BasicAttackBehavior
{
	public float delay;

	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	public Animation unityAnimation;

	public string unityShootAnim;

	public AIAnimator aiAnimator;

	public string aiShootAnim;

	public bool overrideHeadPosition;

	[InspectorShowIf("overrideHeadPosition")]
	public float headPosition;

	private DraGunController m_dragun;

	private BulletScriptSource m_bulletSource;

	private float m_timer;

	private bool m_isAttacking;

	public override void Start()
	{
		base.Start();
		m_dragun = m_aiActor.GetComponent<DraGunController>();
		if ((bool)aiAnimator)
		{
			tk2dSpriteAnimator spriteAnimator = aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
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
		if (delay <= 0f)
		{
			StartThrow();
		}
		else
		{
			m_timer = delay;
			m_isAttacking = false;
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_isAttacking)
		{
			if (m_timer <= 0f)
			{
				StartThrow();
			}
		}
		else
		{
			bool flag = true;
			if ((bool)unityAnimation)
			{
				flag &= !unityAnimation.IsPlaying(unityShootAnim);
			}
			if ((bool)aiAnimator)
			{
				flag &= !aiAnimator.IsPlaying(aiShootAnim);
			}
			if (flag)
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if ((bool)aiAnimator)
		{
			aiAnimator.EndAnimation();
		}
		if ((bool)unityAnimation)
		{
			unityAnimation.Stop();
			unityAnimation.GetClip(unityShootAnim).SampleAnimation(unityAnimation.gameObject, 1000f);
			unityAnimation.GetComponent<DraGunArmController>().UnclipHandSprite();
		}
		if ((bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
		if (overrideHeadPosition)
		{
			m_dragun.OverrideTargetX = null;
		}
		m_isAttacking = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_isAttacking && clip.GetFrame(frame).eventInfo == "fire")
		{
			Fire();
		}
	}

	private void StartThrow()
	{
		if ((bool)unityAnimation)
		{
			unityAnimation.Play(unityShootAnim);
		}
		if ((bool)aiAnimator)
		{
			aiAnimator.PlayUntilCancelled(aiShootAnim);
		}
		if (overrideHeadPosition)
		{
			m_dragun.OverrideTargetX = headPosition;
		}
		m_isAttacking = true;
	}

	private void Fire()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = BulletScript;
		m_bulletSource.Initialize();
	}
}
