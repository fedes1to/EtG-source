using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/ArmosBehavior")]
public class GiantPowderSkullArmosBehavior : BasicAttackBehavior
{
	public GameObject shootPoint;

	public BulletScriptSelector bulletScript;

	public float time = 8f;

	public float speed = 6f;

	public float startingAngle = -90f;

	public float rotationSpeed = -180f;

	[InspectorCategory("Visuals")]
	public string armosAnim;

	private bool m_isRunning;

	private float m_timer;

	private float m_currentAngle;

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
		m_aiAnimator.PlayUntilFinished(armosAnim);
		m_timer = time;
		m_isRunning = true;
		m_aiActor.ClearPath();
		m_aiActor.BehaviorOverridesVelocity = true;
		m_currentAngle = startingAngle;
		m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_currentAngle, speed);
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_timer <= 0f)
		{
			m_aiAnimator.EndAnimation();
			return ContinuousBehaviorResult.Finished;
		}
		m_currentAngle = BraveMathCollege.ClampAngle180(m_currentAngle + rotationSpeed * m_deltaTime);
		m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_currentAngle, speed);
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_isRunning = false;
		m_updateEveryFrame = false;
		m_aiActor.BehaviorOverridesVelocity = false;
		UpdateCooldowns();
	}

	private void ShootBulletScript()
	{
		if (!m_bulletScriptSource)
		{
			m_bulletScriptSource = shootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletScriptSource.BulletManager = m_aiActor.bulletBank;
		m_bulletScriptSource.BulletScript = bulletScript;
		m_bulletScriptSource.Initialize();
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		if (m_isRunning && clip.GetFrame(frameNum).eventInfo == "fire")
		{
			ShootBulletScript();
		}
	}
}
