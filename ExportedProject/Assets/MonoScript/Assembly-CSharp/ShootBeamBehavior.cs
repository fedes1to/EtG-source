using System;
using System.Collections.Generic;
using FullInspector;
using UnityEngine;

public class ShootBeamBehavior : BasicAttackBehavior
{
	public enum BeamSelection
	{
		All,
		Random,
		Specify
	}

	public enum TrackingType
	{
		Follow,
		ConstantTurn,
		AccelTurn
	}

	public enum InitialAimType
	{
		FacingDirection,
		Aim,
		Absolute,
		Transform
	}

	private enum State
	{
		Idle,
		WaitingForTell,
		Firing,
		WaitingForPostAnim
	}

	public BeamSelection beamSelection;

	[InspectorShowIf("ShowSpecificBeamShooter")]
	public AIBeamShooter specificBeamShooter;

	public float firingTime;

	public bool stopWhileFiring;

	public InitialAimType initialAimType;

	public float initialAimOffset;

	[InspectorIndent]
	[InspectorShowIf("ShowRandomInitialAimOffsetSign")]
	public bool randomInitialAimOffsetSign;

	public bool restrictBeamLengthToAim;

	[InspectorIndent]
	[InspectorShowIf("restrictBeamLengthToAim")]
	public float beamLengthOFfset;

	[InspectorIndent]
	[InspectorShowIf("restrictBeamLengthToAim")]
	public float beamLengthSinMagnitude;

	[InspectorIndent]
	[InspectorShowIf("restrictBeamLengthToAim")]
	public float beamLengthSinPeriod;

	[InspectorHeader("Tracking")]
	public TrackingType trackingType;

	[InspectorShowIf("ShowFollowVars")]
	public float maxUnitTurnRate;

	[InspectorShowIf("ShowFollowVars")]
	public float unitTurnRateAcceleration;

	[InspectorShowIf("ShowFollowVars")]
	public float minUnitRadius = 5f;

	[InspectorShowIf("ShowFollowVars")]
	public bool useDegreeCatchUp;

	[InspectorShowIf("ShowDegCatchUpVars")]
	[InspectorIndent]
	public float minDegreesForCatchUp;

	[InspectorIndent]
	[InspectorShowIf("ShowDegCatchUpVars")]
	public float degreeCatchUpSpeed;

	[InspectorShowIf("ShowFollowVars")]
	public bool useUnitCatchUp;

	[InspectorShowIf("ShowUnitCatchUpVars")]
	[InspectorIndent]
	public float minUnitForCatchUp;

	[InspectorShowIf("ShowUnitCatchUpVars")]
	[InspectorIndent]
	public float maxUnitForCatchUp;

	[InspectorShowIf("ShowUnitCatchUpVars")]
	[InspectorIndent]
	public float unitCatchUpSpeed;

	[InspectorShowIf("ShowFollowVars")]
	public bool useUnitOvershoot;

	[InspectorIndent]
	[InspectorShowIf("ShowUnitOvershootVars")]
	public float minUnitForOvershoot;

	[InspectorShowIf("ShowUnitOvershootVars")]
	[InspectorIndent]
	public float unitOvershootTime;

	[InspectorShowIf("ShowUnitOvershootVars")]
	[InspectorIndent]
	public float unitOvershootSpeed;

	[InspectorShowIf("ShowDegRate")]
	public float maxDegTurnRate;

	[InspectorShowIf("ShowDegAccel")]
	public float degTurnRateAcceleration;

	[InspectorCategory("Visuals")]
	public string TellAnimation;

	[InspectorCategory("Visuals")]
	public string FireAnimation;

	[InspectorCategory("Visuals")]
	public string PostFireAnimation;

	private List<AIBeamShooter> m_allBeamShooters;

	private readonly List<AIBeamShooter> m_currentBeamShooters = new List<AIBeamShooter>();

	private float m_timer;

	private float m_firingTime;

	private Vector2 m_targetPosition;

	private float m_currentUnitTurnRate;

	private float m_currentDegTurnRate;

	private float m_unitOvershootFixedDirection;

	private float m_unitOvershootTimer;

	private SpeculativeRigidbody m_backupTarget;

	private State m_state;

	private State state
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

	private bool ShowSpecificBeamShooter()
	{
		return beamSelection == BeamSelection.Specify;
	}

	private bool ShowFollowVars()
	{
		return trackingType == TrackingType.Follow;
	}

	private bool ShowDegRate()
	{
		return trackingType == TrackingType.ConstantTurn || trackingType == TrackingType.AccelTurn;
	}

	private bool ShowDegAccel()
	{
		return trackingType == TrackingType.AccelTurn;
	}

	private bool ShowDegCatchUpVars()
	{
		return trackingType == TrackingType.Follow && useDegreeCatchUp;
	}

	private bool ShowUnitCatchUpVars()
	{
		return trackingType == TrackingType.Follow && useUnitCatchUp;
	}

	private bool ShowUnitOvershootVars()
	{
		return trackingType == TrackingType.Follow && useUnitOvershoot;
	}

	private bool ShowRandomInitialAimOffsetSign()
	{
		return initialAimOffset > 0f;
	}

	public override void Start()
	{
		base.Start();
		m_allBeamShooters = new List<AIBeamShooter>(m_aiActor.GetComponents<AIBeamShooter>());
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
			if ((bool)m_aiAnimator.ChildAnimator)
			{
				tk2dSpriteAnimator spriteAnimator2 = m_aiAnimator.ChildAnimator.spriteAnimator;
				spriteAnimator2.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator2.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
			}
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if ((bool)m_aiActor.TargetRigidbody)
		{
			m_targetPosition = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			m_backupTarget = m_aiActor.TargetRigidbody;
		}
		else if ((bool)m_backupTarget)
		{
			m_targetPosition = m_backupTarget.GetUnitCenter(ColliderType.HitBox);
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
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			m_aiAnimator.PlayUntilFinished(TellAnimation, true);
			state = State.WaitingForTell;
		}
		else
		{
			Fire();
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (state == State.WaitingForTell)
		{
			if (!m_aiAnimator.IsPlaying(TellAnimation))
			{
				Fire();
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (state == State.Firing)
		{
			m_firingTime += m_deltaTime;
			m_timer -= m_deltaTime;
			if (m_timer <= 0f || !m_currentBeamShooters[0].IsFiringLaser)
			{
				StopLasers();
				if (!string.IsNullOrEmpty(PostFireAnimation))
				{
					state = State.WaitingForPostAnim;
					m_aiAnimator.PlayUntilFinished(PostFireAnimation);
					return ContinuousBehaviorResult.Continue;
				}
				return ContinuousBehaviorResult.Finished;
			}
			float num = 0f;
			if (trackingType == TrackingType.Follow)
			{
				AIBeamShooter aIBeamShooter = m_currentBeamShooters[0];
				Vector2 laserFiringCenter = aIBeamShooter.LaserFiringCenter;
				float b = Vector2.Distance(m_targetPosition, laserFiringCenter);
				b = Mathf.Max(minUnitRadius, b);
				float num2 = (m_targetPosition - laserFiringCenter).ToAngle();
				float num3 = BraveMathCollege.ClampAngle180(num2 - aIBeamShooter.LaserAngle);
				float f = num3 * b * ((float)Math.PI / 180f);
				float num4 = maxUnitTurnRate;
				float num5 = Mathf.Sign(num3);
				if (m_unitOvershootTimer > 0f)
				{
					num5 = m_unitOvershootFixedDirection;
					m_unitOvershootTimer -= m_deltaTime;
					num4 = unitOvershootSpeed;
				}
				m_currentUnitTurnRate = Mathf.Clamp(m_currentUnitTurnRate + num5 * unitTurnRateAcceleration * m_deltaTime, 0f - num4, num4);
				float num6 = m_currentUnitTurnRate / b * 57.29578f;
				float num7 = 0f;
				if (useDegreeCatchUp && Mathf.Abs(num3) > minDegreesForCatchUp)
				{
					float b2 = Mathf.InverseLerp(minDegreesForCatchUp, 180f, Mathf.Abs(num3)) * degreeCatchUpSpeed;
					num7 = Mathf.Max(num7, b2);
				}
				if (useUnitCatchUp && Mathf.Abs(f) > minUnitForCatchUp)
				{
					float num8 = Mathf.InverseLerp(minUnitForCatchUp, maxUnitForCatchUp, Mathf.Abs(f)) * unitCatchUpSpeed;
					float b3 = num8 / b * 57.29578f;
					num7 = Mathf.Max(num7, b3);
				}
				if (useUnitOvershoot && Mathf.Abs(f) < minUnitForOvershoot)
				{
					m_unitOvershootFixedDirection = ((m_currentUnitTurnRate > 0f) ? 1 : (-1));
					m_unitOvershootTimer = unitOvershootTime;
				}
				num7 *= Mathf.Sign(num3);
				num = num6 + num7;
			}
			else if (trackingType == TrackingType.ConstantTurn)
			{
				num = maxDegTurnRate;
			}
			else if (trackingType == TrackingType.AccelTurn)
			{
				m_currentDegTurnRate = Mathf.Clamp(m_currentDegTurnRate + degTurnRateAcceleration * m_deltaTime, 0f - maxDegTurnRate, maxDegTurnRate);
				num = m_currentDegTurnRate;
			}
			for (int i = 0; i < m_currentBeamShooters.Count; i++)
			{
				AIBeamShooter aIBeamShooter2 = m_currentBeamShooters[i];
				aIBeamShooter2.LaserAngle = BraveMathCollege.ClampAngle360(aIBeamShooter2.LaserAngle + num * m_deltaTime);
				if (!restrictBeamLengthToAim || !m_aiActor.TargetRigidbody)
				{
					continue;
				}
				float magnitude = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - aIBeamShooter2.LaserFiringCenter).magnitude;
				aIBeamShooter2.MaxBeamLength = magnitude + beamLengthOFfset;
				if (beamLengthSinMagnitude > 0f && beamLengthSinPeriod > 0f)
				{
					aIBeamShooter2.MaxBeamLength += Mathf.Sin(m_firingTime / beamLengthSinPeriod * (float)Math.PI) * beamLengthSinMagnitude;
					if (aIBeamShooter2.MaxBeamLength < 0f)
					{
						aIBeamShooter2.MaxBeamLength = 0f;
					}
				}
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (state == State.WaitingForPostAnim)
		{
			return (!m_aiAnimator.IsPlaying(PostFireAnimation)) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			m_aiAnimator.EndAnimationIf(TellAnimation);
		}
		if (!string.IsNullOrEmpty(FireAnimation))
		{
			m_aiAnimator.EndAnimationIf(FireAnimation);
		}
		if (!string.IsNullOrEmpty(PostFireAnimation))
		{
			m_aiAnimator.EndAnimationIf(PostFireAnimation);
		}
		StopLasers();
		state = State.Idle;
		m_aiAnimator.LockFacingDirection = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		StopLasers();
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (state == State.WaitingForTell && frame.eventInfo == "fire")
		{
			Fire();
		}
	}

	private void Fire()
	{
		if (!string.IsNullOrEmpty(FireAnimation))
		{
			m_aiAnimator.EndAnimation();
			m_aiAnimator.PlayUntilFinished(FireAnimation);
		}
		if (stopWhileFiring)
		{
			m_aiActor.ClearPath();
		}
		if (beamSelection == BeamSelection.All)
		{
			m_currentBeamShooters.AddRange(m_allBeamShooters);
		}
		else if (beamSelection == BeamSelection.Random)
		{
			m_currentBeamShooters.Add(BraveUtility.RandomElement(m_allBeamShooters));
		}
		else if (beamSelection == BeamSelection.Specify)
		{
			m_currentBeamShooters.Add(specificBeamShooter);
		}
		float facingDirection = m_currentBeamShooters[0].CurrentAiAnimator.FacingDirection;
		float num = ((!randomInitialAimOffsetSign) ? 1f : BraveUtility.RandomSign());
		for (int i = 0; i < m_currentBeamShooters.Count; i++)
		{
			AIBeamShooter aIBeamShooter = m_currentBeamShooters[i];
			if (restrictBeamLengthToAim && (bool)m_aiActor.TargetRigidbody)
			{
				float num2 = (aIBeamShooter.MaxBeamLength = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - aIBeamShooter.LaserFiringCenter).magnitude);
			}
			float num3 = 0f;
			if (initialAimType == InitialAimType.FacingDirection)
			{
				num3 = facingDirection;
			}
			else if (initialAimType == InitialAimType.Aim)
			{
				if ((bool)m_aiActor.TargetRigidbody)
				{
					num3 = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - aIBeamShooter.LaserFiringCenter).ToAngle();
				}
			}
			else if (initialAimType == InitialAimType.Absolute)
			{
				num3 = 0f;
			}
			else if (initialAimType == InitialAimType.Transform)
			{
				num3 = aIBeamShooter.beamTransform.eulerAngles.z;
			}
			num3 += num * initialAimOffset;
			aIBeamShooter.StartFiringLaser(num3);
		}
		m_timer = firingTime;
		m_currentUnitTurnRate = 0f;
		m_currentDegTurnRate = 0f;
		m_firingTime = 0f;
		state = State.Firing;
	}

	private void StopLasers()
	{
		for (int i = 0; i < m_currentBeamShooters.Count; i++)
		{
			m_currentBeamShooters[i].StopFiringLaser();
		}
		m_currentBeamShooters.Clear();
	}

	private void BeginState(State state)
	{
	}

	private void EndState(State state)
	{
	}
}
