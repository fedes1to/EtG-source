using Dungeonator;
using UnityEngine;

public class BabyGoodMimicCombatMovementBehavior : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	private float m_repathTimer;

	private Vector2? m_targetPos;

	private RoomHandler m_cachedRoom;

	public override void Start()
	{
		base.Start();
	}

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
		PlayerController playerController = GameManager.Instance.BestActivePlayer;
		if ((bool)m_aiActor && (bool)m_aiActor.CompanionOwner)
		{
			playerController = m_aiActor.CompanionOwner;
		}
		if (!playerController)
		{
			return BehaviorResult.Continue;
		}
		if (m_cachedRoom != null && playerController.CurrentRoom != m_cachedRoom)
		{
			m_cachedRoom = null;
			m_targetPos = null;
		}
		if (playerController.IsInCombat && playerController.CurrentRoom.IsSealed)
		{
			if (!m_targetPos.HasValue)
			{
				IntVector2? intVector = null;
				IntVector2 value = new IntVector2(3, 3);
				while (!intVector.HasValue && value.x > 0)
				{
					intVector = playerController.CurrentRoom.GetRandomAvailableCell(value, CellTypes.FLOOR).Value;
					value -= IntVector2.One;
				}
				if (intVector.HasValue)
				{
					m_targetPos = intVector.Value.ToVector2() + value.ToVector2() / 2f;
					m_cachedRoom = playerController.CurrentRoom;
				}
			}
		}
		else
		{
			m_targetPos = null;
		}
		Vector2? targetPos2 = m_targetPos;
		if (targetPos2.HasValue)
		{
			m_aiActor.PathfindToPosition(m_targetPos.Value);
			m_repathTimer = PathInterval;
		}
		Vector2? targetPos3 = m_targetPos;
		return targetPos3.HasValue ? BehaviorResult.SkipRemainingClassBehaviors : BehaviorResult.Continue;
	}
}
