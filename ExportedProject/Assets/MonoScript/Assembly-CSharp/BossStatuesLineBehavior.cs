using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossStatues/LineBehavior")]
public class BossStatuesLineBehavior : BossStatuesPatternBehavior
{
	public enum Direction
	{
		LeftToRight,
		RightToLeft,
		TopToBottom,
		BottomToTop
	}

	public float Duration;

	public Direction direction;

	private Vector2[] m_statuePositions;

	protected float m_durationTimer;

	private Vector2 m_minPos;

	private Vector2 m_maxPos;

	private Vector2 m_deltaPos;

	private Vector2 m_velocity;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (m_state == PatternState.InProgress)
		{
			DecrementTimer(ref m_durationTimer);
		}
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
	}

	protected override void InitPositions()
	{
		m_statuePositions = new Vector2[m_activeStatueCount];
		RoomHandler parentRoom = m_activeStatues[0].aiActor.ParentRoom;
		Vector2 vector = parentRoom.area.basePosition.ToVector2() + new Vector2(1f, 1f);
		Vector2 vector2 = (parentRoom.area.basePosition + parentRoom.area.dimensions).ToVector2() + new Vector2(-1f, -2f);
		float num = 0f;
		switch (direction)
		{
		case Direction.LeftToRight:
			m_minPos = new Vector2(vector.x, vector2.y);
			m_maxPos = new Vector2(vector.x, vector.y);
			m_velocity = Vector2.right;
			num = vector2.x - vector.x;
			break;
		case Direction.RightToLeft:
			m_minPos = new Vector2(vector2.x, vector2.y);
			m_maxPos = new Vector2(vector2.x, vector.y);
			m_velocity = -Vector2.right;
			num = vector2.x - vector.x;
			break;
		case Direction.TopToBottom:
			m_minPos = new Vector2(vector.x, vector2.y);
			m_maxPos = new Vector2(vector2.x, vector2.y);
			m_velocity = -Vector2.up;
			num = vector2.y - vector.y;
			break;
		case Direction.BottomToTop:
			m_minPos = new Vector2(vector.x, vector.y);
			m_maxPos = new Vector2(vector2.x, vector.y);
			m_velocity = Vector2.up;
			num = vector2.y - vector.y;
			break;
		}
		m_deltaPos = (m_maxPos - m_minPos) / m_activeStatueCount;
		float effectiveMoveSpeed = m_statuesController.GetEffectiveMoveSpeed((!(OverrideMoveSpeed > 0f)) ? m_statuesController.moveSpeed : OverrideMoveSpeed);
		m_velocity *= effectiveMoveSpeed;
		if (Duration > 0f)
		{
			m_durationTimer = Duration;
		}
		else
		{
			m_durationTimer = num / effectiveMoveSpeed;
		}
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			m_statuePositions[i] = m_minPos + ((float)i + 0.5f) * m_deltaPos;
		}
		ReorderStatues(m_statuePositions);
		for (int j = 0; j < m_activeStatueCount; j++)
		{
			m_activeStatues[j].Target = m_statuePositions[j];
		}
	}

	protected override void UpdatePositions()
	{
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			m_statuePositions[i] += m_velocity * m_deltaTime;
			m_activeStatues[i].Target = m_statuePositions[i];
		}
	}

	protected override bool IsFinished()
	{
		return m_durationTimer <= 0f;
	}
}
