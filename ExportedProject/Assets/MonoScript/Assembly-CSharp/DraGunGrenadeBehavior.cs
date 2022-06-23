using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/GrenadeBehavior")]
public class DraGunGrenadeBehavior : BasicAttackBehavior
{
	public float delay;

	public float delay2;

	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	public Animation unityAnimation;

	public string unityShootAnim;

	public AIAnimator aiAnimator;

	public string aiShootAnim;

	public Animation unityAnimation2;

	public string unityShootAnim2;

	public AIAnimator aiAnimator2;

	public string aiShootAnim2;

	public bool overrideHeadPosition;

	[InspectorShowIf("overrideHeadPosition")]
	public float headPosition;

	private DraGunController m_dragun;

	private BulletScriptSource m_bulletSource;

	private float m_timer;

	private bool m_isAttacking;

	private bool m_isAttacking2;

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
			StartAttack();
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
				StartAttack();
			}
		}
		else if (!m_isAttacking2)
		{
			if (m_timer <= 0f)
			{
				StartAttack2();
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
			if ((bool)unityAnimation2)
			{
				flag &= !unityAnimation2.IsPlaying(unityShootAnim2);
			}
			if ((bool)aiAnimator2)
			{
				flag &= !aiAnimator2.IsPlaying(aiShootAnim2);
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
		if ((bool)aiAnimator2)
		{
			aiAnimator2.EndAnimation();
		}
		if ((bool)unityAnimation2)
		{
			unityAnimation2.Stop();
			unityAnimation2.GetClip(unityShootAnim2).SampleAnimation(unityAnimation2.gameObject, 1000f);
			unityAnimation2.GetComponent<DraGunArmController>().UnclipHandSprite();
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
		m_isAttacking2 = false;
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

	private void StartAttack()
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
		if (delay2 <= 0f)
		{
			StartAttack2();
			return;
		}
		m_timer = delay2;
		m_isAttacking2 = false;
	}

	private void StartAttack2()
	{
		if ((bool)unityAnimation2)
		{
			unityAnimation2.Play(unityShootAnim2);
		}
		if ((bool)aiAnimator2)
		{
			aiAnimator2.PlayUntilCancelled(aiShootAnim2);
		}
		m_isAttacking2 = true;
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
