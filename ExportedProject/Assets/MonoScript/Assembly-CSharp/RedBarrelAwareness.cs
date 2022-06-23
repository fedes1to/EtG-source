using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RedBarrelAwareness : OverrideBehaviorBase
{
	public bool AvoidRedBarrels = true;

	public bool ShootRedBarrels = true;

	public bool PushRedBarrels = true;

	protected List<MinorBreakable> m_roomRedBarrels;

	public override void Start()
	{
		base.Start();
		GameManager.Instance.Dungeon.StartCoroutine(Initialize());
	}

	private IEnumerator Initialize()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		DungeonData dungeonData = GameManager.Instance.Dungeon.data;
		RoomHandler room = dungeonData.GetAbsoluteRoomFromPosition(m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		List<MinorBreakable> minorBreakables = StaticReferenceManager.AllMinorBreakables;
		m_roomRedBarrels = new List<MinorBreakable>();
		for (int i = 0; i < minorBreakables.Count; i++)
		{
			if (minorBreakables[i].explodesOnBreak && dungeonData.GetAbsoluteRoomFromPosition(minorBreakables[i].transform.position.IntXY(VectorConversions.Floor)) == room)
			{
				m_roomRedBarrels.Add(minorBreakables[i]);
			}
		}
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_aiActor.OverrideTarget != null && !m_aiActor.OverrideTarget)
		{
			m_aiActor.OverrideTarget = null;
		}
		for (int i = 0; i < m_roomRedBarrels.Count; i++)
		{
			MinorBreakable minorBreakable = m_roomRedBarrels[i];
			if (!minorBreakable)
			{
				m_roomRedBarrels.RemoveAt(i);
				i--;
			}
			else if (minorBreakable.IsBroken)
			{
				if (m_aiActor.OverrideTarget == minorBreakable.specRigidbody)
				{
					m_aiActor.OverrideTarget = null;
				}
				m_roomRedBarrels.RemoveAt(i);
				i--;
			}
		}
		if (AvoidRedBarrels)
		{
			behaviorResult = HandleAvoidance();
		}
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (ShootRedBarrels)
		{
			behaviorResult = HandleShooting();
		}
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (PushRedBarrels)
		{
			behaviorResult = HandlePushing();
		}
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		return behaviorResult;
	}

	protected BehaviorResult HandleAvoidance()
	{
		return BehaviorResult.Continue;
	}

	protected BehaviorResult HandleShooting()
	{
		if (m_aiActor.TargetRigidbody == null)
		{
			return BehaviorResult.Continue;
		}
		float desiredCombatDistance = m_aiActor.DesiredCombatDistance;
		for (int i = 0; i < m_roomRedBarrels.Count; i++)
		{
			Vector2 unitCenter = m_roomRedBarrels[i].specRigidbody.UnitCenter;
			if (GameManager.Instance.Dungeon.data.isTopWall((int)unitCenter.x, (int)unitCenter.y))
			{
				continue;
			}
			float num = Vector2.Distance(unitCenter, m_aiActor.TargetRigidbody.UnitCenter);
			if (!(num > m_roomRedBarrels[i].explosionData.GetDefinedDamageRadius()))
			{
				float num2 = Vector2.Distance(unitCenter, m_aiActor.specRigidbody.UnitCenter);
				if (!(num2 > desiredCombatDistance * 1.25f))
				{
					m_aiActor.OverrideTarget = m_roomRedBarrels[i].specRigidbody;
				}
			}
		}
		return BehaviorResult.Continue;
	}

	protected BehaviorResult HandlePushing()
	{
		return BehaviorResult.Continue;
	}
}
