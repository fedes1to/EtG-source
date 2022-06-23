using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RobotechProjectile : Projectile
{
	private enum Mode
	{
		InitialDumbfire,
		InitialTarget,
		TargetLocked,
		Dumbfire
	}

	private enum CounterCurveState
	{
		Ready,
		Active,
		Done,
		Mandated
	}

	[Header("Robotech Params")]
	public float angularAcceleration = 220f;

	public float searchRadius = 10f;

	public float searchTime = 0.5f;

	public bool canLoseTarget = true;

	public bool reacquiresTargets;

	public bool targetAcquisitionRandom;

	public float counterCurveChance = 0.66f;

	public float counterCurveDuration = 0.2f;

	public float counterCurveMaxDistance = 7f;

	public float initialDumfireTime;

	public bool selectRandomAutoAimTarget;

	[NonSerialized]
	public Vector2? initialOverrideTargetPoint;

	private Mode m_mode = Mode.InitialTarget;

	private CounterCurveState m_counterCurveState;

	private float m_counterCurveDeltaAngle;

	private float m_counterCurveTimer;

	private Vector2 m_counterCurveMandatedDirection;

	private IAutoAimTarget m_currentTarget;

	private Vector2 m_targetPoint;

	private float m_targetSearchTimer;

	private float m_initialDumbfireTimer;

	private bool m_hasGoodLock;

	private static List<AIActor> s_activeEnemies = new List<AIActor>();

	public override void Start()
	{
		base.Start();
		m_usesNormalMoveRegardless = true;
		if (initialOverrideTargetPoint.HasValue)
		{
			m_targetPoint = initialOverrideTargetPoint.Value;
			m_mode = Mode.InitialTarget;
		}
		else if (base.Owner is PlayerController)
		{
			if (BraveInput.GetInstanceForPlayer((base.Owner as PlayerController).PlayerIDX).IsKeyboardAndMouse())
			{
				Camera component = GameManager.Instance.MainCameraController.GetComponent<Camera>();
				Ray ray = component.ScreenPointToRay(Input.mousePosition);
				float enter;
				if (new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out enter))
				{
					m_targetPoint = ray.GetPoint(enter);
					m_targetPoint += UnityEngine.Random.insideUnitCircle.normalized * 0.7f;
				}
			}
			else
			{
				m_targetPoint = (base.Owner as PlayerController).unadjustedAimPoint;
			}
			m_mode = Mode.InitialTarget;
		}
		else if (base.Owner is DumbGunShooter)
		{
			m_targetPoint = base.Owner.transform.position + Vector3.right * 50f;
			m_mode = Mode.InitialTarget;
		}
		else
		{
			m_currentTarget = GameManager.Instance.GetPlayerClosestToPoint(base.Owner.transform.position.XY());
			m_mode = Mode.TargetLocked;
		}
		if (initialDumfireTime > 0f)
		{
			m_mode = Mode.InitialDumbfire;
		}
		TrailController componentInChildren = GetComponentInChildren<TrailController>();
		if ((bool)componentInChildren)
		{
			Vector2 vector = m_targetPoint - base.transform.position.XY();
			float f = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			if (Mathf.Abs(f) > 90f)
			{
				componentInChildren.FlipUvsY = true;
			}
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
		m_targetSearchTimer = searchTime;
		if (base.Owner is DumbGunShooter)
		{
			m_targetSearchTimer = -1000000f;
		}
		UpdateCollisionMask();
	}

	public override void Update()
	{
		if (m_mode == Mode.InitialDumbfire)
		{
			m_initialDumbfireTimer += base.LocalDeltaTime;
			if (m_initialDumbfireTimer > initialDumfireTime)
			{
				m_mode = ((base.Owner is PlayerController || base.Owner is DumbGunShooter) ? Mode.InitialTarget : Mode.TargetLocked);
			}
		}
		else if (m_mode == Mode.InitialTarget)
		{
			m_targetSearchTimer += base.LocalDeltaTime;
			if (m_targetSearchTimer > searchTime && (bool)base.Owner && GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel)
			{
				RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.Owner.transform.position.IntXY(VectorConversions.Floor));
				if (selectRandomAutoAimTarget && absoluteRoomFromPosition != null)
				{
					List<IAutoAimTarget> autoAimTargets = absoluteRoomFromPosition.GetAutoAimTargets();
					if (autoAimTargets != null && autoAimTargets.Count > 0)
					{
						m_currentTarget = BraveUtility.RandomElement(autoAimTargets);
						m_mode = Mode.TargetLocked;
						m_hasGoodLock = false;
					}
				}
				if (m_mode == Mode.InitialTarget)
				{
					if (s_activeEnemies == null)
					{
						s_activeEnemies = new List<AIActor>();
					}
					else
					{
						s_activeEnemies.Clear();
					}
					if (absoluteRoomFromPosition != null)
					{
						absoluteRoomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref s_activeEnemies);
					}
					if (s_activeEnemies.Count > 0)
					{
						if (targetAcquisitionRandom)
						{
							for (int i = 0; i < s_activeEnemies.Count; i++)
							{
								AIActor aIActor = s_activeEnemies[i];
								if (!aIActor || !aIActor.healthHaver || aIActor.healthHaver.IsDead || aIActor.IsGone)
								{
									s_activeEnemies.RemoveAt(i);
									i--;
								}
							}
							if (s_activeEnemies.Count > 0)
							{
								m_currentTarget = s_activeEnemies[UnityEngine.Random.Range(0, s_activeEnemies.Count)];
								m_mode = Mode.TargetLocked;
								m_hasGoodLock = false;
							}
						}
						else
						{
							float num = float.MaxValue;
							for (int j = 0; j < s_activeEnemies.Count; j++)
							{
								AIActor aIActor2 = s_activeEnemies[j];
								if ((bool)aIActor2 && (bool)aIActor2.healthHaver && (bool)aIActor2.specRigidbody && !aIActor2.healthHaver.IsDead && !aIActor2.IsGone)
								{
									float num2 = Vector2.Distance(aIActor2.specRigidbody.UnitCenter, m_targetPoint);
									if (num2 < num)
									{
										m_currentTarget = aIActor2;
										num = num2;
										m_mode = Mode.TargetLocked;
										m_hasGoodLock = false;
									}
								}
							}
						}
					}
				}
			}
		}
		else if (m_mode == Mode.TargetLocked && (m_currentTarget == null || !m_currentTarget.IsValid))
		{
			if (reacquiresTargets)
			{
				m_mode = Mode.InitialTarget;
			}
			else
			{
				m_mode = Mode.Dumbfire;
			}
		}
		if (m_mode == Mode.TargetLocked && m_currentTarget != null)
		{
			m_targetPoint = m_currentTarget.AimCenter;
		}
		if (canLoseTarget && (m_mode == Mode.InitialTarget || m_mode == Mode.TargetLocked) && (bool)this)
		{
			Vector2 vector = m_targetPoint - base.specRigidbody.UnitCenter;
			float f = BraveMathCollege.ClampAngle180(Vector3.Angle(vector, m_currentDirection));
			if (m_counterCurveState != CounterCurveState.Active && m_counterCurveState != CounterCurveState.Mandated)
			{
				if (!m_hasGoodLock && Mathf.Abs(f) < 10f)
				{
					m_hasGoodLock = true;
				}
				else if (m_hasGoodLock && Mathf.Abs(f) > 90f)
				{
					m_hasGoodLock = false;
					m_mode = Mode.Dumbfire;
				}
			}
		}
		base.Update();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ForceCurveDirection(Vector2 dirVec, float duration)
	{
		m_counterCurveState = CounterCurveState.Mandated;
		m_counterCurveMandatedDirection = dirVec;
		counterCurveDuration = duration;
		m_counterCurveTimer = 0f;
		m_hasGoodLock = false;
	}

	protected override void Move()
	{
		Vector2 currentDirection = m_currentDirection;
		if (baseData.UsesCustomAccelerationCurve)
		{
			float time = Mathf.Clamp01(m_timeElapsed / baseData.CustomAccelerationCurveDuration);
			m_currentSpeed = baseData.AccelerationCurve.Evaluate(time) * baseData.speed;
		}
		if (m_mode == Mode.InitialTarget || m_mode == Mode.TargetLocked)
		{
			Vector2 vector = m_targetPoint - base.specRigidbody.UnitCenter;
			float num = Mathf.Atan2(m_currentDirection.y, m_currentDirection.x) * 57.29578f;
			float num2 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			float f = BraveMathCollege.ClampAngle180(num2 - num);
			if (m_counterCurveState == CounterCurveState.Ready && Mathf.Abs(f) < 1f)
			{
				if (vector.magnitude < counterCurveMaxDistance)
				{
					m_counterCurveState = CounterCurveState.Done;
				}
				else if (UnityEngine.Random.value > counterCurveChance)
				{
					m_counterCurveState = CounterCurveState.Done;
				}
				else
				{
					m_counterCurveState = CounterCurveState.Active;
					m_counterCurveDeltaAngle = angularAcceleration * (float)((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
					m_counterCurveTimer = 0f;
					m_hasGoodLock = false;
				}
			}
			float num3 = num;
			if (m_counterCurveState == CounterCurveState.Mandated)
			{
				m_counterCurveTimer += base.LocalDeltaTime;
				num3 = m_counterCurveMandatedDirection.ToAngle();
				if (m_counterCurveTimer > counterCurveDuration)
				{
					m_counterCurveState = CounterCurveState.Done;
				}
			}
			else if (m_counterCurveState == CounterCurveState.Active)
			{
				m_counterCurveTimer += base.LocalDeltaTime;
				num3 += m_counterCurveDeltaAngle * base.LocalDeltaTime;
				if (m_counterCurveTimer > counterCurveDuration)
				{
					m_counterCurveState = CounterCurveState.Done;
				}
			}
			else
			{
				float num4 = Mathf.Sign(f) * Mathf.Min(Mathf.Abs(f), Mathf.Abs(angularAcceleration * base.LocalDeltaTime));
				num3 += num4;
			}
			m_currentDirection = Quaternion.Euler(0f, 0f, num3) * Vector3.right;
			if (shouldRotate)
			{
				base.transform.rotation = Quaternion.Euler(0f, 0f, num3);
			}
		}
		else if ((m_mode == Mode.InitialDumbfire || m_mode == Mode.Dumbfire) && shouldRotate)
		{
			float z = Mathf.Atan2(m_currentDirection.y, m_currentDirection.x) * 57.29578f;
			base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
		base.specRigidbody.Velocity = m_currentDirection * m_currentSpeed;
		base.LastVelocity = base.specRigidbody.Velocity;
		if (OverrideMotionModule != null)
		{
			float angleDiff = Mathf.DeltaAngle(BraveMathCollege.Atan2Degrees(currentDirection), BraveMathCollege.Atan2Degrees(base.LastVelocity));
			OverrideMotionModule.AdjustRightVector(angleDiff);
			OverrideMotionModule.Move(this, base.transform, base.sprite, base.specRigidbody, ref m_timeElapsed, ref m_currentDirection, base.Inverted, shouldRotate);
			base.LastVelocity = base.specRigidbody.Velocity;
		}
	}

	public override void SetNewShooter(SpeculativeRigidbody newShooter)
	{
		m_mode = Mode.Dumbfire;
		base.SetNewShooter(newShooter);
	}

	protected virtual void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (collidesWithProjectiles && (bool)otherRigidbody.projectile && !(otherRigidbody.projectile.Owner is PlayerController))
		{
			PhysicsEngine.SkipCollision = true;
		}
	}
}
