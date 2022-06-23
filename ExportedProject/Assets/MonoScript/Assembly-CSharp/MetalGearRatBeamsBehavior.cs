using System;
using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MetalGearRat/BeamsBehavior")]
public class MetalGearRatBeamsBehavior : BasicAttackBehavior
{
	private struct TargetData
	{
		public Vector2 pos;

		public float slitherCounter;

		public float direction;

		public float slitherDirection;

		public float angularVelocity;

		public bool hasFixedTarget;

		public Vector2 fixedTarget;

		public float fixedTargetTimer;

		public SpeculativeRigidbody targetRigidbody;
	}

	private enum State
	{
		Idle,
		WaitingForTell,
		Firing,
		WaitingForPostAnim
	}

	public List<AIBeamShooter> specificBeamShooters;

	public float firingTime;

	public bool stopWhileFiring;

	public float turnTime = 1f;

	public float slitherPeriod;

	public float slitherMagnitude;

	public float targetMoveSpeed = 3f;

	public float targetMoveAcceleration = 0.25f;

	public int randomTargets = 2;

	public float randomRetargetMin = 1f;

	public float randomRetargetMax = 2f;

	public BulletScriptSelector BulletScript;

	public Transform ShootPoint;

	[InspectorCategory("Visuals")]
	public string TellAnimation;

	[InspectorCategory("Visuals")]
	public string FireAnimation;

	[InspectorCategory("Visuals")]
	public string PostFireAnimation;

	private TargetData[] m_targetData;

	private float m_timer;

	private float m_slitherCounter;

	private float m_moveSpeed;

	private Vector2 m_roomLowerLeft;

	private Vector2 m_roomUpperRight;

	private BulletScriptSource m_bulletSource;

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

	public override void Start()
	{
		base.Start();
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
		m_roomLowerLeft = m_aiActor.ParentRoom.area.UnitBottomLeft;
		m_roomUpperRight = m_aiActor.ParentRoom.area.UnitTopRight + new Vector2(0f, 3f);
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_slitherCounter += m_deltaTime * m_aiActor.behaviorSpeculator.CooldownScale;
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
			m_timer -= m_deltaTime;
			m_moveSpeed += targetMoveAcceleration * m_deltaTime;
			if (m_timer <= 0f || !specificBeamShooters[0].IsFiringLaser)
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
			for (int i = 0; i < specificBeamShooters.Count; i++)
			{
				AIBeamShooter aIBeamShooter = specificBeamShooters[i];
				Vector2? vector = null;
				if (m_targetData[i].hasFixedTarget)
				{
					vector = m_targetData[i].fixedTarget;
					m_targetData[i].fixedTargetTimer -= m_deltaTime;
					if (m_targetData[i].fixedTargetTimer <= 0f)
					{
						m_targetData[i].fixedTarget = RandomTargetPosition();
						m_targetData[i].fixedTargetTimer = UnityEngine.Random.Range(randomRetargetMin, randomRetargetMax);
					}
				}
				else if ((bool)m_targetData[i].targetRigidbody)
				{
					vector = m_targetData[i].targetRigidbody.GetUnitCenter(ColliderType.HitBox);
				}
				if (vector.HasValue)
				{
					Vector2 pos = m_targetData[i].pos;
					float target = (vector.Value - pos).ToAngle();
					m_targetData[i].direction = Mathf.SmoothDampAngle(m_targetData[i].direction, target, ref m_targetData[i].angularVelocity, turnTime);
				}
				m_targetData[i].slitherDirection = Mathf.Sin(m_slitherCounter * (float)Math.PI / slitherPeriod) * slitherMagnitude;
				Vector2 vector2 = BraveMathCollege.DegreesToVector(m_targetData[i].direction + m_targetData[i].slitherDirection, m_moveSpeed);
				m_targetData[i].pos += vector2 * m_deltaTime;
				m_targetData[i].pos = Vector2Extensions.Clamp(m_targetData[i].pos, m_roomLowerLeft, m_roomUpperRight);
				Vector2 vector3 = m_targetData[i].pos - aIBeamShooter.LaserFiringCenter;
				aIBeamShooter.LaserAngle = vector3.ToAngle();
				aIBeamShooter.MaxBeamLength = vector3.magnitude;
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
		m_moveSpeed = targetMoveSpeed;
		m_slitherCounter = 0f;
		if (!string.IsNullOrEmpty(FireAnimation))
		{
			m_aiAnimator.EndAnimation();
			m_aiAnimator.PlayUntilFinished(FireAnimation);
		}
		if (stopWhileFiring)
		{
			m_aiActor.ClearPath();
		}
		m_targetData = new TargetData[specificBeamShooters.Count];
		for (int i = 0; i < specificBeamShooters.Count; i++)
		{
			AIBeamShooter aIBeamShooter = specificBeamShooters[i];
			aIBeamShooter.IgnoreAiActorPlayerChecks = true;
			Vector2 vector = RandomTargetPosition();
			m_targetData[i] = new TargetData
			{
				pos = vector,
				direction = BraveUtility.RandomAngle()
			};
			Vector2 vector2 = vector - aIBeamShooter.LaserFiringCenter;
			aIBeamShooter.MaxBeamLength = vector2.magnitude;
			if (i < randomTargets)
			{
				m_targetData[i].hasFixedTarget = true;
				m_targetData[i].fixedTarget = RandomTargetPosition();
				m_targetData[i].fixedTargetTimer = UnityEngine.Random.Range(randomRetargetMin, randomRetargetMax);
			}
			else
			{
				PlayerController randomActivePlayer = GameManager.Instance.GetRandomActivePlayer();
				if ((bool)randomActivePlayer && (bool)randomActivePlayer.specRigidbody)
				{
					m_targetData[i].targetRigidbody = randomActivePlayer.specRigidbody;
				}
			}
			aIBeamShooter.StartFiringLaser(vector2.ToAngle());
		}
		m_timer = firingTime;
		if (!m_bulletSource)
		{
			m_bulletSource = ShootPoint.gameObject.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = BulletScript;
		m_bulletSource.Initialize();
		state = State.Firing;
	}

	private void StopLasers()
	{
		for (int i = 0; i < specificBeamShooters.Count; i++)
		{
			specificBeamShooters[i].StopFiringLaser();
		}
		if ((bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
	}

	private Vector2 RandomTargetPosition()
	{
		Vector2 min = m_roomLowerLeft + new Vector2(1f, 3f);
		Vector2 max = m_roomUpperRight.WithY(m_aiActor.transform.position.y) - new Vector2(1f, 0f);
		return BraveUtility.RandomVector2(min, max);
	}

	private void BeginState(State state)
	{
	}

	private void EndState(State state)
	{
	}
}
