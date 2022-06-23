using System;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossStatues/CircleBehavior")]
public class BossStatuesCircleBehavior : BossStatuesPatternBehavior
{
	public float Duration;

	public float CircleRadius;

	public bool UseFixedCircleCenter;

	public float CircleCenterVelocity;

	private float[] m_statueAngles;

	private float m_cachedStatueAngle;

	private float m_rotationSpeed;

	private float m_circularSpeed;

	private Vector2 m_roomLowerLeft;

	private Vector2 m_roomUpperRight;

	private Vector2 m_circleCenter;

	protected float m_durationTimer;

	public override void Start()
	{
		base.Start();
		m_cachedStatueAngle = 0.5f * (360f / (float)m_statuesController.allStatues.Count);
	}

	public override void Upkeep()
	{
		DecrementTimer(ref m_durationTimer);
		base.Upkeep();
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_cachedStatueAngle = BraveMathCollege.ClampAngle360(m_statueAngles[0]);
	}

	protected override void InitPositions()
	{
		RoomHandler parentRoom = m_activeStatues[0].aiActor.ParentRoom;
		m_roomLowerLeft = parentRoom.area.basePosition.ToVector2() + new Vector2(1f, 1f);
		m_roomUpperRight = (parentRoom.area.basePosition + parentRoom.area.dimensions).ToVector2() + new Vector2(-1f, -5f);
		float num = (float)Math.PI * 2f * CircleRadius;
		m_circularSpeed = m_statuesController.GetEffectiveMoveSpeed((!(OverrideMoveSpeed > 0f)) ? m_statuesController.moveSpeed : OverrideMoveSpeed);
		m_rotationSpeed = 360f / (num / m_circularSpeed);
		m_circleCenter = Vector2.zero;
		m_statueAngles = new float[m_activeStatueCount];
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			m_statueAngles[i] = m_cachedStatueAngle + (float)i * (360f / (float)m_activeStatueCount);
			m_circleCenter += m_activeStatues[i].GroundPosition;
		}
		m_circleCenter /= (float)m_activeStatueCount;
		m_circleCenter = BraveMathCollege.ClampToBounds(m_circleCenter, m_roomLowerLeft + new Vector2(CircleRadius, CircleRadius), m_roomUpperRight - new Vector2(CircleRadius, CircleRadius));
		if (UseFixedCircleCenter)
		{
			m_circleCenter = m_statuesController.PatternCenter;
		}
		Vector2[] array = new Vector2[m_activeStatueCount];
		for (int j = 0; j < m_activeStatueCount; j++)
		{
			array[j] = GetTargetPoint(m_statueAngles[j]);
		}
		ReorderStatues(array);
		for (int k = 0; k < array.Length; k++)
		{
			m_activeStatues[k].Target = GetTargetPoint(m_statueAngles[k]);
		}
	}

	protected override void UpdatePositions()
	{
		PlayerController playerClosestToPoint = GameManager.Instance.GetPlayerClosestToPoint(m_circleCenter);
		if ((bool)playerClosestToPoint)
		{
			m_circleCenter = Vector2.MoveTowards(m_circleCenter, playerClosestToPoint.specRigidbody.UnitCenter, CircleCenterVelocity * m_deltaTime);
			m_circleCenter = BraveMathCollege.ClampToBounds(m_circleCenter, m_roomLowerLeft + new Vector2(CircleRadius, CircleRadius), m_roomUpperRight - new Vector2(CircleRadius, CircleRadius));
		}
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			m_statueAngles[i] += m_deltaTime * m_rotationSpeed;
			m_activeStatues[i].Target = GetTargetPoint(m_statueAngles[i]);
		}
		m_statuesController.OverrideMoveSpeed = m_circularSpeed + CircleCenterVelocity * 2f;
	}

	protected override bool IsFinished()
	{
		return m_durationTimer <= 0f;
	}

	protected override void BeginState(PatternState state)
	{
		if (state == PatternState.InProgress)
		{
			m_durationTimer = Duration;
		}
		base.BeginState(state);
	}

	private Vector2 GetTargetPoint(float angle)
	{
		return m_circleCenter + BraveMathCollege.DegreesToVector(angle, CircleRadius);
	}
}
