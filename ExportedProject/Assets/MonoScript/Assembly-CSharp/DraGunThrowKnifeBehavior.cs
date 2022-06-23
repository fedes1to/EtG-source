using System;
using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/ThrowKnifeBehavior")]
public class DraGunThrowKnifeBehavior : BasicAttackBehavior
{
	public float delay;

	public GameObject ShootPoint;

	public string BulletName;

	public float angle;

	public Animation unityAnimation;

	public string unityShootAnim;

	public AIAnimator aiAnimator;

	public string aiShootAnim;

	private DraGunController m_dragun;

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
		m_isAttacking = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		List<AIActor> activeEnemies = m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (activeEnemies[i].name.Contains("knife", true))
			{
				return false;
			}
		}
		return true;
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_isAttacking && clip.GetFrame(frame).eventInfo == "fire")
		{
			m_dragun.bulletBank.CreateProjectileFromBank(ShootPoint.transform.position, angle, "knife");
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
		m_isAttacking = true;
	}
}
