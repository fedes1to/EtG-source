using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class CerebralBoreProjectile : Projectile
{
	private enum BoreMotionType
	{
		TRACKING,
		BORING
	}

	public ExplosionData explosionData;

	public float boreTime = 2f;

	public AnimationCurve boreCurve;

	public ParticleSystem SparksSystem;

	private AIActor m_targetEnemy;

	private bool m_hasExploded;

	private BoreMotionType m_currentMotionType;

	private Vector2 m_startPosition;

	private Vector2 m_initialAimPoint;

	private float m_currentBezierPoint;

	private HashSet<SpeculativeRigidbody> m_rigidbodiesDamagedInFlight = new HashSet<SpeculativeRigidbody>();

	private Vector2 m_targetTrackingPoint;

	private float m_bezierUpdateTimer;

	private float m_elapsedBore;

	public override void Start()
	{
		base.Start();
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(ProcessPreCollision));
		AcquireTarget();
	}

	private void ProcessPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (m_currentMotionType == BoreMotionType.TRACKING)
		{
			if (otherRigidbody.aiActor != null)
			{
				if (otherRigidbody.aiActor != m_targetEnemy && !m_rigidbodiesDamagedInFlight.Contains(otherRigidbody))
				{
					bool killedTarget = false;
					HandleDamage(otherRigidbody, otherPixelCollider, out killedTarget, null);
					m_rigidbodiesDamagedInFlight.Add(otherRigidbody);
				}
				PhysicsEngine.SkipCollision = true;
			}
		}
		else if (m_currentMotionType == BoreMotionType.BORING)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	protected void AcquireTarget()
	{
		m_startPosition = base.transform.position.XY();
		m_initialAimPoint = m_startPosition + base.transform.right.XY() * 3f;
		m_currentBezierPoint = 0f;
		Func<AIActor, bool> isValid = (AIActor targ) => !targ.UniquePlayerTargetFlag;
		if (base.Owner is PlayerController)
		{
			PlayerController playerController = base.Owner as PlayerController;
			if (playerController.CurrentRoom == null)
			{
				return;
			}
			List<AIActor> activeEnemies = playerController.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies == null)
			{
				return;
			}
			m_targetEnemy = BraveUtility.GetClosestToPosition(activeEnemies, playerController.unadjustedAimPoint.XY(), isValid);
		}
		else if ((bool)base.Owner)
		{
			List<AIActor> activeEnemies2 = base.Owner.GetAbsoluteParentRoom().GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies2 == null)
			{
				return;
			}
			m_targetEnemy = BraveUtility.GetClosestToPosition(activeEnemies2, base.transform.position.XY(), isValid);
		}
		if (m_targetEnemy != null)
		{
			m_targetEnemy.UniquePlayerTargetFlag = true;
		}
	}

	protected override void Move()
	{
		if (m_targetEnemy == null || !m_targetEnemy)
		{
			AcquireTarget();
		}
		switch (m_currentMotionType)
		{
		case BoreMotionType.TRACKING:
			HandleTracking();
			break;
		case BoreMotionType.BORING:
			HandleBoring();
			break;
		}
	}

	protected void HandleBoring()
	{
		if (m_targetEnemy != null)
		{
			m_targetEnemy.UniquePlayerTargetFlag = false;
		}
		m_elapsedBore += BraveTime.DeltaTime;
		float time = Mathf.Clamp01(m_elapsedBore / boreTime);
		if (m_elapsedBore < boreTime && m_targetEnemy != null && (bool)m_targetEnemy && m_targetEnemy.healthHaver.IsAlive && m_targetEnemy.specRigidbody.CollideWithOthers)
		{
			if (!m_targetEnemy.healthHaver.IsBoss)
			{
				m_targetEnemy.ClearPath();
				if (m_targetEnemy.behaviorSpeculator.IsInterruptable)
				{
					m_targetEnemy.behaviorSpeculator.Interrupt();
				}
				m_targetEnemy.behaviorSpeculator.Stun(1f);
			}
			Vector2 unitTopCenter = m_targetEnemy.specRigidbody.HitboxPixelCollider.UnitTopCenter;
			unitTopCenter += new Vector2(0f, boreCurve.Evaluate(time));
			Vector2 vector = unitTopCenter - base.transform.PositionVector2();
			base.specRigidbody.Velocity = vector / BraveTime.DeltaTime;
			base.LastVelocity = base.specRigidbody.Velocity;
			float z = BraveMathCollege.Atan2Degrees(Vector2.down);
			base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
		else
		{
			if ((bool)m_targetEnemy && m_targetEnemy.healthHaver.IsAlive)
			{
				m_targetEnemy.healthHaver.ApplyDamage(base.ModifiedDamage, Vector2.down, base.OwnerName);
				Exploder.Explode(base.specRigidbody.UnitCenter.ToVector3ZUp(), explosionData, Vector2.up);
				m_hasExploded = true;
			}
			DieInAir();
		}
	}

	protected void HandleTracking()
	{
		float num = baseData.speed;
		if (m_targetEnemy != null)
		{
			m_bezierUpdateTimer -= BraveTime.DeltaTime;
			if (m_bezierUpdateTimer <= 0f)
			{
				m_bezierUpdateTimer = 0.1f;
				m_targetTrackingPoint = m_targetEnemy.sprite.WorldTopCenter;
			}
			Vector2 p = m_startPosition + (m_initialAimPoint - m_startPosition).normalized * 5f;
			Vector2 vector = m_targetTrackingPoint + Vector2.up * 4f;
			IntVector2 intVector = vector.ToIntVector2(VectorConversions.Floor);
			Func<IntVector2, bool> func = (IntVector2 pos) => GameManager.Instance.Dungeon.data[pos] == null || GameManager.Instance.Dungeon.data[pos].type == CellType.WALL;
			if (func(intVector + IntVector2.Down) || func(intVector + IntVector2.Down * 2))
			{
				vector = m_targetTrackingPoint + Vector2.down;
			}
			float num2 = BraveMathCollege.EstimateBezierPathLength(m_startPosition, p, vector, m_targetTrackingPoint, 20);
			float num3 = num2 / baseData.speed;
			float t = m_currentBezierPoint + BraveTime.DeltaTime * 2f / num3;
			m_currentBezierPoint += BraveTime.DeltaTime / num3;
			if (m_currentBezierPoint >= 1f)
			{
				base.specRigidbody.CollideWithTileMap = false;
				SparksSystem.gameObject.SetActive(true);
				m_currentMotionType = BoreMotionType.BORING;
			}
			Vector2 vector2 = BraveMathCollege.CalculateBezierPoint(t, m_startPosition, p, vector, m_targetTrackingPoint);
			Vector2 v = vector2 - base.transform.PositionVector2();
			num = Mathf.Min(num, v.magnitude / BraveTime.DeltaTime);
			float value = BraveMathCollege.Atan2Degrees(v);
			value = value.Quantize(3f);
			base.transform.rotation = Quaternion.Euler(0f, 0f, value);
		}
		base.specRigidbody.Velocity = base.transform.right * num;
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	protected override void OnDestroy()
	{
		if (!m_hasExploded && !GameManager.Instance.IsLoadingLevel)
		{
			Exploder.Explode(base.specRigidbody.UnitCenter.ToVector3ZUp(), explosionData, Vector2.up);
		}
		base.OnDestroy();
		AkSoundEngine.PostEvent("Stop_WPN_cerebralbore_loop_01", base.gameObject);
		if (m_targetEnemy != null)
		{
			m_targetEnemy.UniquePlayerTargetFlag = false;
		}
	}
}
