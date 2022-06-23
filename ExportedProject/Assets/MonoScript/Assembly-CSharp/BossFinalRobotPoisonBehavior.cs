using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRobot/PoisonBehavior")]
public class BossFinalRobotPoisonBehavior : BasicAttackBehavior
{
	private enum State
	{
		None,
		WaitingForTell,
		Firing,
		WaitingForAnim
	}

	public float initialAimDirection;

	public float turnRate = 360f;

	public float totalTurnDegrees = 360f;

	public int divisions = 6;

	[InspectorCategory("Visuals")]
	public string tellAnimation;

	private AIBeamShooter m_beamShooter;

	private float m_turnedDegrees;

	private float m_nextToggleDegrees;

	private float m_toggleWidthDegrees;

	private State m_state;

	public override void Start()
	{
		base.Start();
		m_beamShooter = m_aiActor.GetComponent<AIBeamShooter>();
		if (!string.IsNullOrEmpty(tellAnimation))
		{
			tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
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
		m_turnedDegrees = 0f;
		m_toggleWidthDegrees = 360f / (float)divisions;
		m_nextToggleDegrees = UnityEngine.Random.Range(0f, m_toggleWidthDegrees);
		m_beamShooter.LaserAngle = initialAimDirection;
		if (!string.IsNullOrEmpty(tellAnimation))
		{
			m_aiAnimator.PlayUntilFinished(tellAnimation, true);
			m_state = State.WaitingForTell;
		}
		else
		{
			m_state = State.Firing;
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.WaitingForTell)
		{
			if (!m_aiAnimator.IsPlaying(tellAnimation))
			{
				m_state = State.Firing;
			}
		}
		else if (m_state == State.Firing)
		{
			float num = Mathf.Sign(turnRate);
			float num2 = Mathf.Abs(turnRate * m_deltaTime);
			float num3 = m_beamShooter.LaserAngle;
			bool flag = m_beamShooter.IsFiringLaser;
			while (num2 > 0f)
			{
				if (num2 < m_nextToggleDegrees)
				{
					m_turnedDegrees += num2;
					num3 += num2 * num;
					m_nextToggleDegrees -= num2;
					num2 = 0f;
					continue;
				}
				m_turnedDegrees += m_nextToggleDegrees;
				num3 += m_nextToggleDegrees * num;
				num2 -= m_nextToggleDegrees;
				m_nextToggleDegrees = m_toggleWidthDegrees;
				if (flag)
				{
					m_beamShooter.StopFiringLaser();
					flag = false;
					continue;
				}
				m_beamShooter.StartFiringLaser(m_beamShooter.LaserAngle);
				if ((bool)m_beamShooter.LaserBeam)
				{
					m_beamShooter.LaserBeam.projectile.ImmuneToSustainedBlanks = true;
				}
				flag = true;
			}
			m_beamShooter.LaserAngle = BraveMathCollege.ClampAngle360(num3);
			if (m_turnedDegrees >= totalTurnDegrees)
			{
				if (!string.IsNullOrEmpty(tellAnimation) && m_aiAnimator.IsPlaying(tellAnimation))
				{
					if ((bool)m_beamShooter && m_beamShooter.IsFiringLaser)
					{
						m_beamShooter.StopFiringLaser();
					}
					m_state = State.WaitingForAnim;
					return ContinuousBehaviorResult.Continue;
				}
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.WaitingForAnim && !m_aiAnimator.IsPlaying(tellAnimation))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if ((bool)m_beamShooter && m_beamShooter.IsFiringLaser)
		{
			m_beamShooter.StopFiringLaser();
		}
		if (!string.IsNullOrEmpty(tellAnimation))
		{
			m_aiAnimator.EndAnimationIf(tellAnimation);
		}
		m_state = State.None;
		m_aiAnimator.LockFacingDirection = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		m_beamShooter.StopFiringLaser();
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (m_state == State.WaitingForTell && frame.eventInfo == "fire")
		{
			m_state = State.Firing;
		}
	}
}
