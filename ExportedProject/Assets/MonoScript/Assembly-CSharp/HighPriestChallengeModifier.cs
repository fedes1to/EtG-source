using System;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class HighPriestChallengeModifier : ChallengeModifier
{
	[EnemyIdentifier]
	public string CandleGuid;

	public int NumCandles = 6;

	public float MergoCooldown = 25f;

	private AIActor m_boss;

	private RoomHandler m_room;

	private void Start()
	{
		m_room = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		List<AIActor> activeEnemies = m_room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if ((bool)activeEnemies[i] && (bool)activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss)
			{
				m_boss = activeEnemies[i];
			}
		}
		if ((bool)m_boss.behaviorSpeculator)
		{
			for (int j = 0; j < m_boss.behaviorSpeculator.AttackBehaviors.Count; j++)
			{
				if (!(m_boss.behaviorSpeculator.AttackBehaviors[j] is AttackBehaviorGroup))
				{
					continue;
				}
				AttackBehaviorGroup attackBehaviorGroup = m_boss.behaviorSpeculator.AttackBehaviors[j] as AttackBehaviorGroup;
				for (int k = 0; k < attackBehaviorGroup.AttackBehaviors.Count; k++)
				{
					if (attackBehaviorGroup.AttackBehaviors[k].Behavior is HighPriestMergoBehavior)
					{
						attackBehaviorGroup.AttackBehaviors[k].Probability = 1000f;
						HighPriestMergoBehavior highPriestMergoBehavior = attackBehaviorGroup.AttackBehaviors[k].Behavior as HighPriestMergoBehavior;
						highPriestMergoBehavior.Cooldown = MergoCooldown;
					}
				}
			}
		}
		RoomHandler room = m_room;
		room.OnChangedTerrifyingDarkState = (Action<bool>)Delegate.Combine(room.OnChangedTerrifyingDarkState, new Action<bool>(HandleDarkStateChange));
	}

	private void HandleDarkStateChange(bool isDark)
	{
		if (!isDark)
		{
			SpawnWave();
		}
	}

	private void OnDestroy()
	{
		DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(GameManager.Instance.PrimaryPlayer.CenterPosition, 100f);
	}

	private void SpawnWave()
	{
		int numCandles = NumCandles;
		for (int i = 0; i < numCandles; i++)
		{
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(CandleGuid);
			IntVector2? intVector = PreprocessSpawn(orLoadByGuid, m_boss.specRigidbody.UnitCenter, m_room);
			if (intVector.HasValue)
			{
				AIActor.Spawn(orLoadByGuid, intVector.Value, m_room, true);
			}
		}
	}

	private IntVector2? PreprocessSpawn(AIActor enemy, Vector2 center, RoomHandler sourceRoom)
	{
		PixelCollider groundPixelCollider = enemy.specRigidbody.GroundPixelCollider;
		IntVector2 m_enemyClearance;
		if (groundPixelCollider != null && groundPixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.Manual)
		{
			m_enemyClearance = new Vector2((float)groundPixelCollider.ManualWidth / 16f, (float)groundPixelCollider.ManualHeight / 16f).ToIntVector2(VectorConversions.Ceil);
		}
		else
		{
			Debug.LogFormat("Enemy type {0} does not have a manually defined ground collider!", enemy.name);
			m_enemyClearance = IntVector2.One;
		}
		float minDistanceSquared = 0f;
		float maxDistanceSquared = 400f;
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < m_enemyClearance.x; i++)
			{
				for (int j = 0; j < m_enemyClearance.y; j++)
				{
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
					{
						return false;
					}
				}
			}
			float num = (float)c.x + 0.5f - center.x;
			float num2 = (float)c.y + 0.5f - center.y;
			float num3 = num * num + num2 * num2;
			return (!(num3 < minDistanceSquared) && !(num3 > maxDistanceSquared)) ? true : false;
		};
		return sourceRoom.GetRandomAvailableCell(m_enemyClearance, enemy.PathableTiles, true, cellValidator);
	}
}
