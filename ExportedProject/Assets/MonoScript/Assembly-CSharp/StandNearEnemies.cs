using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class StandNearEnemies : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	public float DesiredDistance = 5f;

	private float m_repathTimer;

	private List<AIActor> m_roomEnemies = new List<AIActor>();

	private Vector2? m_targetPos;

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	public override BehaviorResult Update()
	{
		if (m_repathTimer > 0f)
		{
			Vector2? targetPos = m_targetPos;
			return targetPos.HasValue ? BehaviorResult.SkipRemainingClassBehaviors : BehaviorResult.Continue;
		}
		UpdateTarget();
		Vector2? targetPos2 = m_targetPos;
		if (targetPos2.HasValue)
		{
			m_aiActor.PathfindToPosition(m_targetPos.Value);
			m_repathTimer = PathInterval;
		}
		Vector2? targetPos3 = m_targetPos;
		return targetPos3.HasValue ? BehaviorResult.SkipRemainingClassBehaviors : BehaviorResult.Continue;
	}

	private void UpdateTarget()
	{
		m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref m_roomEnemies);
		for (int num = m_roomEnemies.Count - 1; num >= 0; num--)
		{
			AIActor aIActor = m_roomEnemies[num];
			if (aIActor.IsHarmlessEnemy || !aIActor.IsNormalEnemy || aIActor.healthHaver.IsDead || aIActor == m_aiActor)
			{
				m_roomEnemies.RemoveAt(num);
			}
		}
		if (m_roomEnemies.Count <= 0)
		{
			m_targetPos = null;
			return;
		}
		bool flag = false;
		while (!flag)
		{
			flag = true;
			Vector2 prevAverage = Vector2.zero;
			int prevCount = 0;
			for (int i = 0; i < m_roomEnemies.Count; i++)
			{
				Vector2 unitCenter = m_roomEnemies[i].specRigidbody.UnitCenter;
				BraveMathCollege.WeightedAverage(unitCenter, ref prevAverage, ref prevCount);
			}
			if (prevCount == 1)
			{
				Vector2 normalized = (m_aiActor.specRigidbody.UnitCenter - prevAverage).normalized;
				Vector2 newValue = prevAverage + normalized * DesiredDistance;
				BraveMathCollege.WeightedAverage(newValue, ref prevAverage, ref prevCount);
			}
			int num2 = -1;
			float num3 = float.MinValue;
			for (int j = 0; j < m_roomEnemies.Count; j++)
			{
				Vector2 unitCenter2 = m_roomEnemies[j].specRigidbody.UnitCenter;
				float num4 = Vector2.Distance(unitCenter2, prevAverage);
				if (num4 > DesiredDistance && num4 > num3)
				{
					num2 = j;
					num3 = num4;
				}
			}
			if (num2 >= 0)
			{
				m_roomEnemies.RemoveAt(num2);
				flag = false;
			}
			m_targetPos = prevAverage;
		}
	}
}
