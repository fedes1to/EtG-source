using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/GlockBehavior")]
public class DraGunGlockBehavior : BasicAttackBehavior
{
	private enum HandState
	{
		None,
		Intro,
		In,
		MoveToOut,
		Out,
		MoveToIn,
		Outro
	}

	public enum FacingDirection
	{
		Out,
		In,
		Aim
	}

	[Serializable]
	public class GlockAttack
	{
		public float preDelay;

		public FacingDirection dir;

		public BulletScriptSelector bulletScript;
	}

	public GameObject shootPoint;

	public Animation unityAnimation;

	public AIAnimator aiAnimator;

	public GlockAttack[] attacks;

	private HandState m_state;

	private float m_delayTimer;

	private int m_attackIndex;

	private bool m_isShooting;

	private bool m_facingLeft;

	private string m_unityAnimPrefix;

	private BulletScriptSource m_bulletSource;

	private HandState State
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

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = aiAnimator.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		m_facingLeft = aiAnimator.name.Contains("left", true);
		m_unityAnimPrefix = ((!m_facingLeft) ? "DraGunRight" : "DraGunLeft");
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
		m_attackIndex = -1;
		State = HandState.Intro;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == HandState.Intro)
		{
			if (!aiAnimator.IsPlaying("glock_draw"))
			{
				State = HandState.Out;
				AdvanceAttack();
			}
		}
		else if (State == HandState.Out || State == HandState.In)
		{
			if (m_attackIndex >= attacks.Length)
			{
				State = ((State != HandState.Out) ? HandState.MoveToOut : HandState.Outro);
			}
			else if (m_delayTimer > 0f)
			{
				m_delayTimer -= m_deltaTime;
				if (m_delayTimer <= 0f)
				{
					HandleAim();
				}
			}
			else if (!m_isShooting)
			{
				Fire();
			}
			else if (!aiAnimator.IsPlaying((State != HandState.Out) ? "glock_fire_in" : "glock_fire_out"))
			{
				m_isShooting = false;
				AdvanceAttack();
			}
		}
		else if (State == HandState.MoveToIn)
		{
			if (!aiAnimator.IsPlaying("glock_flip_in"))
			{
				State = HandState.In;
			}
		}
		else if (State == HandState.MoveToOut)
		{
			if (!aiAnimator.IsPlaying("glock_flip_out"))
			{
				State = HandState.Out;
			}
		}
		else if (State == HandState.Outro && !aiAnimator.IsPlaying("glock_putaway"))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		State = HandState.None;
		if ((bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
		if ((bool)aiAnimator)
		{
			aiAnimator.EndAnimation();
		}
		if ((bool)unityAnimation)
		{
			unityAnimation.Stop();
			unityAnimation.GetClip(m_unityAnimPrefix + "GlockPutAway").SampleAnimation(unityAnimation.gameObject, 1000f);
			unityAnimation.GetComponent<DraGunArmController>().ClipArmSprites();
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_isShooting && clip.GetFrame(frame).eventInfo == "fire")
		{
			ShootBulletScript();
		}
	}

	private void AdvanceAttack()
	{
		m_attackIndex++;
		if (m_attackIndex < attacks.Length)
		{
			m_delayTimer = attacks[m_attackIndex].preDelay;
			if (m_delayTimer <= 0f)
			{
				HandleAim();
			}
		}
	}

	private void HandleAim()
	{
		if (m_attackIndex >= attacks.Length)
		{
			return;
		}
		GlockAttack glockAttack = attacks[m_attackIndex];
		FacingDirection facingDirection = glockAttack.dir;
		if (facingDirection == FacingDirection.Aim && (bool)m_aiActor.TargetRigidbody)
		{
			facingDirection = (m_facingLeft ? ((!(m_aiActor.TargetRigidbody.UnitCenter.x < m_aiActor.specRigidbody.UnitCenter.x - 12.5f)) ? FacingDirection.In : FacingDirection.Out) : ((!(m_aiActor.TargetRigidbody.UnitCenter.x > m_aiActor.specRigidbody.UnitCenter.x + 12.5f)) ? FacingDirection.In : FacingDirection.Out));
		}
		switch (facingDirection)
		{
		case FacingDirection.In:
			if (State == HandState.Out)
			{
				State = HandState.MoveToIn;
			}
			break;
		case FacingDirection.Out:
			if (State == HandState.In)
			{
				State = HandState.MoveToOut;
			}
			break;
		}
	}

	private void Fire()
	{
		m_isShooting = true;
		if (State == HandState.In)
		{
			aiAnimator.PlayUntilCancelled("glock_fire_in");
		}
		else if (State == HandState.Out)
		{
			aiAnimator.PlayUntilCancelled("glock_fire_out");
		}
	}

	private void ShootBulletScript()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = shootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = attacks[m_attackIndex].bulletScript;
		m_bulletSource.Initialize();
	}

	private void BeginState(HandState state)
	{
		if (state == HandState.Intro)
		{
			aiAnimator.PlayUntilCancelled("glock_draw");
			if ((bool)unityAnimation)
			{
				unityAnimation.Play(m_unityAnimPrefix + "GlockDraw");
			}
		}
		switch (state)
		{
		case HandState.MoveToOut:
			aiAnimator.PlayUntilCancelled("glock_flip_out");
			unityAnimation.Play(m_unityAnimPrefix + "GlockFlipOut");
			break;
		case HandState.MoveToIn:
			aiAnimator.PlayUntilCancelled("glock_flip_in");
			unityAnimation.Play(m_unityAnimPrefix + "GlockFlipIn");
			break;
		case HandState.Outro:
			aiAnimator.PlayUntilCancelled("glock_putaway");
			unityAnimation.Play(m_unityAnimPrefix + "GlockPutAway");
			break;
		}
	}

	private void EndState(HandState state)
	{
		if ((state == HandState.In || state == HandState.Out) && m_isShooting && (bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
	}
}
