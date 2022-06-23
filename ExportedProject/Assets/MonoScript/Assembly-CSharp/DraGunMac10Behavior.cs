using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Mac10Behavior")]
public class DraGunMac10Behavior : BasicAttackBehavior
{
	private enum HandShootState
	{
		None,
		Intro,
		Shooting,
		Outro
	}

	public enum FireType
	{
		Immediate,
		tk2dAnimEvent,
		UnityAnimEvent
	}

	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	public FireType fireType;

	public Animation unityAnimation;

	[InspectorShowIf("UseUnityAnimation")]
	[InspectorIndent]
	public string unityIntroAnim;

	[InspectorIndent]
	[InspectorShowIf("UseUnityAnimation")]
	public string unityShootAnim;

	[InspectorIndent]
	[InspectorShowIf("UseUnityAnimation")]
	public string unityOutroAnim;

	public AIAnimator aiAnimator;

	[InspectorIndent]
	[InspectorShowIf("UseAiAnimator")]
	public bool useAnimationDirection;

	[InspectorIndent]
	[InspectorShowIf("UseAiAnimator")]
	public string aiIntroAnim;

	[InspectorIndent]
	[InspectorShowIf("UseAiAnimator")]
	public string aiShootAnim;

	[InspectorShowIf("UseAiAnimator")]
	[InspectorIndent]
	public string aiOutroAnim;

	private HandShootState m_state;

	private bool m_isShooting;

	private BulletScriptSource m_bulletSource;

	private HandShootState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	private bool UseUnityAnimation()
	{
		return unityAnimation != null;
	}

	private bool UseAiAnimator()
	{
		return aiAnimator != null;
	}

	public override void Start()
	{
		base.Start();
		if (fireType == FireType.tk2dAnimEvent)
		{
			tk2dSpriteAnimator spriteAnimator = aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(tk2dAnimationEventTriggered));
		}
		if (fireType == FireType.UnityAnimEvent)
		{
			m_aiActor.behaviorSpeculator.AnimationEventTriggered += UnityAnimationEventTriggered;
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
		State = HandShootState.Intro;
		if ((bool)aiAnimator && useAnimationDirection)
		{
			aiAnimator.UseAnimatedFacingDirection = true;
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == HandShootState.Intro)
		{
			bool flag = true;
			if (UseUnityAnimation())
			{
				flag &= !unityAnimation.IsPlaying(unityIntroAnim);
			}
			if (UseAiAnimator())
			{
				flag &= !aiAnimator.IsPlaying(aiIntroAnim);
			}
			if (flag)
			{
				State = HandShootState.Shooting;
			}
		}
		else if (State == HandShootState.Shooting)
		{
			if (fireType == FireType.Immediate && !m_isShooting)
			{
				ShootBulletScript();
			}
			bool flag2 = true;
			if ((bool)unityAnimation)
			{
				flag2 &= !unityAnimation.IsPlaying(unityShootAnim);
			}
			if ((bool)aiAnimator)
			{
				flag2 &= !aiAnimator.IsPlaying(aiShootAnim) || aiAnimator.spriteAnimator.CurrentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Loop;
			}
			if (flag2)
			{
				State = HandShootState.Outro;
			}
		}
		else if (State == HandShootState.Outro)
		{
			bool flag3 = true;
			if ((bool)unityAnimation)
			{
				flag3 &= !unityAnimation.IsPlaying(unityOutroAnim);
			}
			if ((bool)aiAnimator)
			{
				flag3 &= !aiAnimator.IsPlaying(aiOutroAnim) || aiAnimator.spriteAnimator.CurrentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Loop;
			}
			if (flag3)
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		State = HandShootState.None;
		if ((bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
		if ((bool)aiAnimator)
		{
			aiAnimator.EndAnimation();
			if (useAnimationDirection)
			{
				aiAnimator.UseAnimatedFacingDirection = false;
			}
		}
		if ((bool)unityAnimation)
		{
			unityAnimation.Stop();
			unityAnimation.GetClip(unityOutroAnim).SampleAnimation(unityAnimation.gameObject, 1000f);
			unityAnimation.GetComponent<DraGunArmController>().ClipArmSprites();
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void tk2dAnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		UnityAnimationEventTriggered(clip.GetFrame(frame).eventInfo);
	}

	private void UnityAnimationEventTriggered(string eventInfo)
	{
		if (eventInfo == "fire")
		{
			ShootBulletScript();
		}
		else if (eventInfo == "cease_fire" && (bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
	}

	private void ShootBulletScript()
	{
		if (!string.IsNullOrEmpty(BulletScript.scriptTypeName))
		{
			if (!m_bulletSource)
			{
				m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
			}
			m_bulletSource.BulletManager = m_aiActor.bulletBank;
			m_bulletSource.BulletScript = BulletScript;
			m_bulletSource.Initialize();
			m_isShooting = true;
		}
	}

	private void BeginState(HandShootState state)
	{
		switch (state)
		{
		case HandShootState.Intro:
			if ((bool)unityAnimation)
			{
				unityAnimation.Play(unityIntroAnim);
			}
			if ((bool)aiAnimator)
			{
				aiAnimator.PlayUntilCancelled(aiIntroAnim);
			}
			break;
		case HandShootState.Shooting:
			if ((bool)unityAnimation)
			{
				unityAnimation.Play(unityShootAnim);
			}
			if ((bool)aiAnimator)
			{
				aiAnimator.PlayUntilCancelled(aiShootAnim);
			}
			m_isShooting = false;
			break;
		case HandShootState.Outro:
			if ((bool)unityAnimation)
			{
				unityAnimation.Play(unityOutroAnim);
			}
			if ((bool)aiAnimator)
			{
				aiAnimator.PlayUntilCancelled(aiOutroAnim);
			}
			break;
		}
	}

	private void EndState(HandShootState state)
	{
		if (state == HandShootState.Shooting && m_isShooting && (bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
	}
}
