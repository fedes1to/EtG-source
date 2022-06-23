using Dungeonator;
using Pathfinding;
using UnityEngine;

public class MoveErraticallyBehavior : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	public float PointReachedPauseTime;

	public bool PreventFiringWhileMoving;

	public float InitialDelay;

	public bool StayOnScreen = true;

	public bool AvoidTarget = true;

	public bool UseTargetsRoom;

	private float m_repathTimer;

	private float m_pauseTimer;

	private IntVector2? m_targetPos;

	private IntVector2 m_cachedCameraBottomLeft;

	private IntVector2 m_cachedCameraBottomRight;

	private float m_cachedAngleFromTarget;

	private Vector2 m_cachedTargetPos;

	private float? m_cachedAngleFromOtherTarget;

	private Vector2? m_cachedOtherTargetPos;

	public override bool AllowFearRunState
	{
		get
		{
			return true;
		}
	}

	public override void Start()
	{
		base.Start();
		m_pauseTimer = InitialDelay;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		DecrementTimer(ref m_pauseTimer);
		if (StayOnScreen)
		{
			Vector2 vector = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
			Vector2 vector2 = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
			m_cachedCameraBottomLeft = vector.ToIntVector2(VectorConversions.Ceil);
			m_cachedCameraBottomRight = vector2.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
		}
		if (AvoidTarget && (bool)m_aiActor.TargetRigidbody)
		{
			m_cachedTargetPos = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.Ground);
			m_cachedAngleFromTarget = (m_aiActor.specRigidbody.UnitCenter - m_cachedTargetPos).ToAngle();
			PlayerController playerController = m_aiActor.PlayerTarget as PlayerController;
			if ((bool)playerController && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(playerController);
				m_cachedOtherTargetPos = otherPlayer.specRigidbody.GetUnitCenter(ColliderType.Ground);
				m_cachedAngleFromOtherTarget = (m_aiActor.specRigidbody.UnitCenter - m_cachedTargetPos).ToAngle();
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
		IntVector2? targetPos = m_targetPos;
		if (!targetPos.HasValue && m_repathTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		if (m_pauseTimer > 0f)
		{
			return BehaviorResult.SkipRemainingClassBehaviors;
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
				RoomHandler roomHandler = m_aiActor.ParentRoom;
				if (UseTargetsRoom && (bool)m_aiActor.TargetRigidbody)
				{
					PlayerController playerController = ((!m_aiActor.TargetRigidbody.gameActor) ? null : (m_aiActor.TargetRigidbody.gameActor as PlayerController));
					if ((bool)playerController)
					{
						roomHandler = playerController.CurrentRoom;
					}
				}
				m_targetPos = roomHandler.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, FullCellValidator);
			}
			IntVector2? targetPos5 = m_targetPos;
			if (!targetPos5.HasValue)
			{
				return BehaviorResult.Continue;
			}
			m_aiActor.PathfindToPosition(m_targetPos.Value.ToCenterVector2());
		}
		if (PreventFiringWhileMoving)
		{
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	public void ResetPauseTimer()
	{
		m_pauseTimer = 0f;
	}

	private bool SimpleCellValidator(IntVector2 c)
	{
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
		if (StayOnScreen && (c.x < m_cachedCameraBottomLeft.x || c.y < m_cachedCameraBottomLeft.y || c.x + m_aiActor.Clearance.x - 1 > m_cachedCameraBottomRight.x || c.y + m_aiActor.Clearance.y - 1 > m_cachedCameraBottomRight.y))
		{
			return false;
		}
		return true;
	}

	private bool FullCellValidator(IntVector2 c)
	{
		if (!SimpleCellValidator(c))
		{
			return false;
		}
		if (AvoidTarget && (bool)m_aiActor.TargetRigidbody)
		{
			float a = (Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance) - m_cachedTargetPos).ToAngle();
			if (BraveMathCollege.AbsAngleBetween(a, m_cachedAngleFromTarget) > 90f)
			{
				return false;
			}
			Vector2? cachedOtherTargetPos = m_cachedOtherTargetPos;
			if (cachedOtherTargetPos.HasValue)
			{
				float? cachedAngleFromOtherTarget = m_cachedAngleFromOtherTarget;
				if (cachedAngleFromOtherTarget.HasValue)
				{
					a = (Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance) - m_cachedOtherTargetPos.Value).ToAngle();
					if (BraveMathCollege.AbsAngleBetween(a, m_cachedAngleFromOtherTarget.Value) > 90f)
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
