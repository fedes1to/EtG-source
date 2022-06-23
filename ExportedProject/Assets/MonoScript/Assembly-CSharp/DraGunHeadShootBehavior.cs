using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/HeadShootBehavior")]
public class DraGunHeadShootBehavior : BasicAttackBehavior
{
	private enum State
	{
		None,
		MovingToPosition,
		Animating
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

	public string HeadAiAnim = "sweep";

	public string MotionClip = "DraGunHeadSweep1";

	private DraGunController m_dragun;

	private DraGunHeadController m_head;

	private AIAnimator m_headAnimator;

	private Animation m_unityAnimation;

	private State m_state;

	private AnimationClip m_clip;

	private float m_cachedShootPointRotation;

	private BulletScriptSource m_bulletSource;

	private GameObject s_dummyGameObject;

	private GameObject s_dummyHeadObject;

	public override void Start()
	{
		base.Start();
		m_dragun = m_aiActor.GetComponent<DraGunController>();
		m_unityAnimation = m_dragun.neck.GetComponent<Animation>();
		m_head = m_dragun.head;
		m_headAnimator = m_head.aiAnimator;
		m_clip = m_unityAnimation.GetClip(MotionClip);
		if (fireType == FireType.tk2dAnimEvent)
		{
			tk2dSpriteAnimator spriteAnimator = m_head.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(tk2dAnimationEventTriggered));
		}
		if (fireType == FireType.UnityAnimEvent)
		{
			m_aiActor.behaviorSpeculator.AnimationEventTriggered += UnityAnimationEventTriggered;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
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
		m_state = State.MovingToPosition;
		m_head.OverrideDesiredPosition = GetStartPosition();
		if ((bool)ShootPoint)
		{
			m_cachedShootPointRotation = ShootPoint.transform.eulerAngles.z;
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.MovingToPosition)
		{
			if (m_head.ReachedOverridePosition)
			{
				m_state = State.Animating;
				m_head.OverrideDesiredPosition = null;
				m_headAnimator.AnimatedFacingDirection = m_headAnimator.FacingDirection;
				m_headAnimator.UseAnimatedFacingDirection = true;
				if (fireType == FireType.Immediate)
				{
					ShootBulletScript();
				}
				m_clip.SampleAnimation(m_aiActor.gameObject, 0f);
				m_unityAnimation.Stop();
				m_unityAnimation.cullingType = AnimationCullingType.BasedOnRenderers;
				m_unityAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
				m_unityAnimation.Play(MotionClip);
				m_headAnimator.PlayUntilCancelled(HeadAiAnim);
			}
		}
		else if (m_state == State.Animating)
		{
			if ((bool)ShootPoint)
			{
				ShootPoint.transform.rotation = Quaternion.Euler(ShootPoint.transform.eulerAngles.WithZ(m_headAnimator.FacingDirection));
			}
			if (!m_unityAnimation.IsPlaying(MotionClip))
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_state = State.None;
		m_headAnimator.UseAnimatedFacingDirection = false;
		m_headAnimator.EndAnimationIf(HeadAiAnim);
		if ((bool)m_unityAnimation)
		{
			m_unityAnimation.Stop();
			m_unityAnimation.GetClip(MotionClip).SampleAnimation(m_unityAnimation.gameObject, 1000f);
		}
		if ((bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
		if ((bool)ShootPoint)
		{
			ShootPoint.transform.rotation = Quaternion.Euler(ShootPoint.transform.eulerAngles.WithZ(m_cachedShootPointRotation));
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
		}
	}

	private Vector2 GetStartPosition()
	{
		if (!s_dummyGameObject)
		{
			s_dummyGameObject = new GameObject("Dummy Game Object");
			s_dummyHeadObject = new GameObject("head");
			s_dummyHeadObject.transform.parent = s_dummyGameObject.transform;
		}
		m_clip.SampleAnimation(s_dummyGameObject, 0f);
		return s_dummyHeadObject.transform.position + m_aiActor.transform.position;
	}
}
