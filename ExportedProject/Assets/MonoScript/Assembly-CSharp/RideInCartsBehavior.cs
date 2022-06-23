using System.Collections.Generic;
using UnityEngine;

public class RideInCartsBehavior : MovementBehaviorBase
{
	private MineCartController m_currentTarget;

	private bool m_ridingCart;

	protected float m_findNewCartTimer;

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_findNewCartTimer);
	}

	private MineCartController GetAvailableMineCart()
	{
		List<MineCartController> componentsInRoom = m_aiActor.ParentRoom.GetComponentsInRoom<MineCartController>();
		for (int i = 0; i < componentsInRoom.Count; i++)
		{
			if (componentsInRoom[i].IsOnlyPlayerMinecart || componentsInRoom[i].occupation != 0)
			{
				componentsInRoom.RemoveAt(i);
				i--;
			}
		}
		componentsInRoom.Sort((MineCartController a, MineCartController b) => Vector2.Distance(m_aiActor.CenterPosition, a.sprite.WorldCenter).CompareTo(Vector2.Distance(m_aiActor.CenterPosition, b.sprite.WorldCenter)));
		if (componentsInRoom.Count == 0)
		{
			return null;
		}
		return componentsInRoom[0];
	}

	public override BehaviorResult Update()
	{
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.MINEGEON)
		{
			return BehaviorResult.Continue;
		}
		if (m_ridingCart)
		{
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		if (m_findNewCartTimer <= 0f)
		{
			m_currentTarget = GetAvailableMineCart();
			m_findNewCartTimer = 5f;
		}
		if (m_currentTarget != null)
		{
			if (m_currentTarget.occupation != 0)
			{
				m_findNewCartTimer = 0f;
				return BehaviorResult.Continue;
			}
			m_aiActor.PathfindToPosition(m_currentTarget.sprite.WorldCenter);
			if (Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, m_currentTarget.specRigidbody.UnitCenter) < 5f && BraveMathCollege.DistBetweenRectangles(m_aiActor.specRigidbody.UnitBottomLeft, m_aiActor.specRigidbody.UnitDimensions, m_currentTarget.specRigidbody.UnitBottomLeft, m_currentTarget.specRigidbody.UnitDimensions) < 0.5f)
			{
				m_aiActor.ClearPath();
				m_currentTarget.BecomeOccupied(m_aiActor);
				m_ridingCart = true;
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		return BehaviorResult.Continue;
	}
}
