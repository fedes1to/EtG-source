using System;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class PickupMover : BraveBehaviour
{
	public float pathInterval = 0.25f;

	public float acceleration = 2.5f;

	public float maxSpeed = 15f;

	public float minRadius;

	[NonSerialized]
	public bool moveIfRoomUnclear;

	[NonSerialized]
	public bool stopPathingOnContact;

	private RoomHandler m_room;

	private bool m_radiusValid;

	private bool m_shouldPath;

	private Path m_currentPath;

	private float m_pathTimer;

	private Vector2 m_lastPosition;

	private float m_currentSpeed;

	private const CellTypes c_pathableTiles = CellTypes.FLOOR | CellTypes.PIT;

	public void Start()
	{
		m_room = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		if (m_room == null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else if (moveIfRoomUnclear)
		{
			m_shouldPath = true;
		}
		else if (!m_room.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			m_shouldPath = true;
		}
		else
		{
			RoomHandler room = m_room;
			room.OnEnemiesCleared = (Action)Delegate.Combine(room.OnEnemiesCleared, new Action(RoomCleared));
		}
		m_lastPosition = base.specRigidbody.UnitCenter;
	}

	private void TestRadius(PlayerController targetPlayer)
	{
		if ((bool)targetPlayer && (!m_radiusValid || stopPathingOnContact))
		{
			if (targetPlayer.CurrentRoom != m_room)
			{
				m_radiusValid = true;
			}
			if (minRadius <= 0f)
			{
				m_radiusValid = true;
			}
			else if (Vector2.Distance(targetPlayer.CenterPosition, base.sprite.WorldCenter) > minRadius)
			{
				m_radiusValid = true;
			}
			else if (stopPathingOnContact)
			{
				m_shouldPath = false;
				UnityEngine.Object.Destroy(this);
			}
		}
	}

	public void Update()
	{
		m_pathTimer -= BraveTime.DeltaTime;
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(unitCenter, true);
		TestRadius(activePlayerClosestToPoint);
		if (m_shouldPath && m_radiusValid)
		{
			if ((bool)base.debris)
			{
				base.debris.enabled = false;
			}
			if (!activePlayerClosestToPoint || activePlayerClosestToPoint.IsFalling || !activePlayerClosestToPoint.specRigidbody.CollideWithOthers)
			{
				m_currentSpeed = 0f;
			}
			else
			{
				if (m_pathTimer <= 0f)
				{
					IntVector2 intVector = unitCenter.ToIntVector2(VectorConversions.Floor);
					IntVector2 intVector2 = activePlayerClosestToPoint.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
					if (Pathfinder.Instance.IsValidPathCell(intVector) && Pathfinder.Instance.IsValidPathCell(intVector2) && Pathfinder.Instance.GetPath(intVector, intVector2, out m_currentPath, IntVector2.One, CellTypes.FLOOR | CellTypes.PIT))
					{
						if (m_currentPath.Count == 0)
						{
							m_currentPath = null;
						}
						else
						{
							m_currentPath.Smooth(base.specRigidbody.UnitCenter, base.specRigidbody.UnitDimensions / 2f, CellTypes.FLOOR | CellTypes.PIT, false, IntVector2.One);
						}
					}
					m_pathTimer = pathInterval;
				}
				m_currentSpeed = Mathf.Min(m_currentSpeed + acceleration * BraveTime.DeltaTime, maxSpeed);
			}
			base.specRigidbody.Velocity = GetPathVelocityContribution(activePlayerClosestToPoint);
		}
		else
		{
			if (!Mathf.Approximately(base.transform.position.x % 0.0625f, 0f))
			{
				base.transform.position = base.transform.position.Quantize(0.0625f);
				base.specRigidbody.Reinitialize();
			}
			m_currentSpeed = 0f;
			base.specRigidbody.Velocity = Vector2.zero;
		}
		m_lastPosition = unitCenter;
	}

	public void RoomCleared()
	{
		m_shouldPath = true;
		RoomHandler room = m_room;
		room.OnEnemiesCleared = (Action)Delegate.Remove(room.OnEnemiesCleared, new Action(RoomCleared));
	}

	private Vector2 GetPathVelocityContribution(PlayerController targetPlayer)
	{
		if (!targetPlayer)
		{
			return Vector2.zero;
		}
		if (m_currentPath == null || m_currentPath.Count == 0)
		{
			return m_currentSpeed * (targetPlayer.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter).normalized;
		}
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 firstCenterVector = m_currentPath.GetFirstCenterVector2();
		bool flag = false;
		if (Vector2.Distance(unitCenter, firstCenterVector) < PhysicsEngine.PixelToUnit(1))
		{
			flag = true;
		}
		else
		{
			Vector2 b = BraveMathCollege.ClosestPointOnLineSegment(firstCenterVector, m_lastPosition, unitCenter);
			if (Vector2.Distance(firstCenterVector, b) < PhysicsEngine.PixelToUnit(1))
			{
				flag = true;
			}
		}
		if (flag)
		{
			m_currentPath.RemoveFirst();
			if (m_currentPath.Count == 0)
			{
				m_currentPath = null;
				return Vector2.zero;
			}
		}
		return m_currentSpeed * (firstCenterVector - unitCenter).normalized;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
