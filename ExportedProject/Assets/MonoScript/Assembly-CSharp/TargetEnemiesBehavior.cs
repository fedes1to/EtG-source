using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class TargetEnemiesBehavior : TargetBehaviorBase
{
	public bool LineOfSight = true;

	public bool ObjectPermanence = true;

	public float SearchInterval = 0.25f;

	private float m_losTimer;

	public override void Start()
	{
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_losTimer);
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_losTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		m_losTimer = SearchInterval;
		if ((bool)m_aiActor.PlayerTarget)
		{
			if (m_aiActor.PlayerTarget.IsFalling)
			{
				m_aiActor.PlayerTarget = null;
				m_aiActor.ClearPath();
				return BehaviorResult.SkipRemainingClassBehaviors;
			}
			if ((bool)m_aiActor.PlayerTarget.healthHaver && m_aiActor.PlayerTarget.healthHaver.IsDead)
			{
				m_aiActor.PlayerTarget = null;
				m_aiActor.ClearPath();
				return BehaviorResult.SkipRemainingClassBehaviors;
			}
		}
		else
		{
			m_aiActor.PlayerTarget = null;
		}
		if (!ObjectPermanence)
		{
			m_aiActor.PlayerTarget = null;
		}
		if (m_aiActor.PlayerTarget != null)
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.CanTargetEnemies)
		{
			return BehaviorResult.Continue;
		}
		List<AIActor> activeEnemies = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(m_aiActor.GridPosition).GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies != null && activeEnemies.Count > 0)
		{
			AIActor playerTarget = null;
			float num = float.MaxValue;
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				AIActor aIActor = activeEnemies[i];
				if (aIActor == m_aiActor)
				{
					continue;
				}
				float num2 = Vector2.Distance(m_aiActor.CenterPosition, aIActor.CenterPosition);
				if (!(num2 < num))
				{
					continue;
				}
				if (LineOfSight)
				{
					int standardPlayerVisibilityMask = CollisionMask.StandardPlayerVisibilityMask;
					RaycastResult result;
					if (!PhysicsEngine.Instance.Raycast(m_aiActor.CenterPosition, aIActor.CenterPosition - m_aiActor.CenterPosition, num2, out result, true, true, standardPlayerVisibilityMask, null, false, null, m_aiActor.specRigidbody))
					{
						RaycastResult.Pool.Free(ref result);
						continue;
					}
					if (result.SpeculativeRigidbody == null || result.SpeculativeRigidbody.GetComponent<PlayerController>() == null)
					{
						RaycastResult.Pool.Free(ref result);
						continue;
					}
					RaycastResult.Pool.Free(ref result);
				}
				playerTarget = aIActor;
				num = num2;
			}
			m_aiActor.PlayerTarget = playerTarget;
		}
		if (m_aiShooter != null && m_aiActor.PlayerTarget != null)
		{
			m_aiShooter.AimAtPoint(m_aiActor.PlayerTarget.CenterPosition);
		}
		if (!m_aiActor.HasBeenEngaged)
		{
			m_aiActor.HasBeenEngaged = true;
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
