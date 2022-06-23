using UnityEngine;

public class DazedBehavior : OverrideBehaviorBase
{
	public float PointReachedPauseTime = 0.5f;

	public float PathInterval = 0.5f;

	private float m_repathTimer;

	private float m_pauseTimer;

	private IntVector2? m_targetPos;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override bool OverrideOtherBehaviors()
	{
		return true;
	}

	public override BehaviorResult Update()
	{
		m_repathTimer -= m_aiActor.LocalDeltaTime;
		m_pauseTimer -= m_aiActor.LocalDeltaTime;
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		IntVector2? targetPos = m_targetPos;
		if (!targetPos.HasValue && m_repathTimer > 0f)
		{
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		if (m_pauseTimer > 0f)
		{
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		IntVector2? targetPos2 = m_targetPos;
		if (targetPos2.HasValue && m_aiActor.PathComplete)
		{
			m_targetPos = null;
			if (PointReachedPauseTime > 0f)
			{
				m_pauseTimer = PointReachedPauseTime;
				return BehaviorResult.SkipAllRemainingBehaviors;
			}
		}
		if (m_repathTimer <= 0f)
		{
			m_repathTimer = PathInterval;
			IntVector2? targetPos3 = m_targetPos;
			if (targetPos3.HasValue && !SimpleCellValidator(m_targetPos.Value))
			{
				m_targetPos = null;
			}
			IntVector2? targetPos4 = m_targetPos;
			if (!targetPos4.HasValue)
			{
				m_targetPos = m_aiActor.ParentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, SimpleCellValidator);
			}
			IntVector2? targetPos5 = m_targetPos;
			if (!targetPos5.HasValue)
			{
				return BehaviorResult.SkipAllRemainingBehaviors;
			}
			m_aiActor.PathfindToPosition(m_targetPos.Value.ToCenterVector2());
		}
		return BehaviorResult.SkipAllRemainingBehaviors;
	}

	private bool SimpleCellValidator(IntVector2 c)
	{
		if (Vector2.Distance(c.ToVector2(), m_aiActor.CenterPosition) > 4f)
		{
			return false;
		}
		for (int i = 0; i < m_aiActor.Clearance.x; i++)
		{
			for (int j = 0; j < m_aiActor.Clearance.y; j++)
			{
				if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
				{
					return false;
				}
			}
		}
		return true;
	}
}
