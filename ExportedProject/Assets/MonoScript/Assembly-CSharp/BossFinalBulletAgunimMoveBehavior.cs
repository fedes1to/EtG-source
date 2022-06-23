using FullInspector;
using Pathfinding;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalBullet/AgunimMoveBehavior")]
public class BossFinalBulletAgunimMoveBehavior : BasicAttackBehavior
{
	private enum MoveState
	{
		None,
		PreMove,
		Move,
		PostMove
	}

	public float MoveTime = 1f;

	public float MinSpeed;

	public float MaxSpeed;

	public bool DisableCollisionDuringMove;

	public int MinDistFromHorizontalWall = 4;

	public int MinDistFromNorthWall = 2;

	public int MinTilesAbovePlayer = 4;

	public int MaxTilesAbovePlayer = 8;

	public int MinDistanceFromPlayer = 4;

	public bool UseSouthWall;

	[InspectorIndent]
	[InspectorShowIf("UseSouthWall")]
	public int MinTilesBelowPlayer = 4;

	[InspectorShowIf("UseSouthWall")]
	[InspectorIndent]
	public int MaxTilesBelowPlayer = 4;

	[InspectorShowIf("UseSouthWall")]
	[InspectorIndent]
	public int MinDistFromSouthWall = 2;

	[InspectorCategory("Visuals")]
	public string preMoveAnimation;

	[InspectorCategory("Visuals")]
	public string moveAnimation;

	[InspectorCategory("Visuals")]
	public string postMoveAnimation;

	[InspectorCategory("Visuals")]
	public bool enableShadowTrail;

	private MoveState m_state;

	private Vector2 m_startPoint;

	private Vector2 m_targetPoint;

	private float m_moveTime;

	private float m_setupTimer;

	private AfterImageTrailController m_shadowTrail;

	private MoveState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	public override void Start()
	{
		base.Start();
		m_shadowTrail = m_aiActor.GetComponent<AfterImageTrailController>();
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		m_aiActor.ClearPath();
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_aiAnimator.LockFacingDirection = true;
		m_aiAnimator.FacingDirection = -90f;
		if (!string.IsNullOrEmpty(preMoveAnimation))
		{
			State = MoveState.PreMove;
		}
		else
		{
			State = MoveState.Move;
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == MoveState.PreMove)
		{
			if (!m_aiAnimator.IsPlaying(preMoveAnimation))
			{
				State = MoveState.Move;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (State == MoveState.Move)
		{
			if (m_setupTimer > m_moveTime)
			{
				m_aiActor.BehaviorVelocity = Vector2.zero;
				if (!string.IsNullOrEmpty(postMoveAnimation))
				{
					State = MoveState.PostMove;
					return ContinuousBehaviorResult.Continue;
				}
				return ContinuousBehaviorResult.Finished;
			}
			if (m_deltaTime > 0f)
			{
				Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
				Vector2 vector = Vector2Extensions.SmoothStep(m_startPoint, m_targetPoint, m_setupTimer / m_moveTime);
				m_aiActor.BehaviorVelocity = (vector - unitCenter) / m_deltaTime;
				m_setupTimer += m_deltaTime;
			}
		}
		else if (State == MoveState.PostMove && !m_aiAnimator.IsPlaying(postMoveAnimation))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (DisableCollisionDuringMove)
		{
			m_aiActor.specRigidbody.CollideWithOthers = true;
			m_aiActor.IsGone = false;
		}
		State = MoveState.None;
		if (!string.IsNullOrEmpty(preMoveAnimation))
		{
			m_aiAnimator.EndAnimationIf(preMoveAnimation);
		}
		if (!string.IsNullOrEmpty(moveAnimation))
		{
			m_aiAnimator.EndAnimationIf(moveAnimation);
		}
		if (!string.IsNullOrEmpty(postMoveAnimation))
		{
			m_aiAnimator.EndAnimationIf(postMoveAnimation);
		}
		m_aiAnimator.LockFacingDirection = false;
		m_aiActor.BehaviorOverridesVelocity = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void UpdateTargetPoint()
	{
		float minDistanceFromPlayerSquared = MinDistanceFromPlayer * MinDistanceFromPlayer;
		bool hasOtherPlayer = false;
		Vector2 playerLowerLeft = m_aiActor.TargetRigidbody.HitboxPixelCollider.UnitBottomLeft;
		Vector2 playerUpperRight = m_aiActor.TargetRigidbody.HitboxPixelCollider.UnitTopRight;
		Vector2 otherPlayerLowerLeft = Vector2.zero;
		Vector2 otherPlayerUpperRight = Vector2.zero;
		float maxPlayerY = playerLowerLeft.y;
		float minPlayerY = playerLowerLeft.y;
		PlayerController playerController = m_behaviorSpeculator.PlayerTarget as PlayerController;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)playerController)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(playerController);
			if ((bool)otherPlayer && otherPlayer.healthHaver.IsAlive)
			{
				hasOtherPlayer = true;
				otherPlayerLowerLeft = otherPlayer.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
				otherPlayerUpperRight = otherPlayer.specRigidbody.HitboxPixelCollider.UnitTopRight;
				maxPlayerY = Mathf.Max(maxPlayerY, otherPlayerLowerLeft.y);
				minPlayerY = Mathf.Min(minPlayerY, otherPlayerLowerLeft.y);
			}
		}
		int minDx = -MinDistFromHorizontalWall;
		int maxDx = MinDistFromHorizontalWall + m_aiActor.Clearance.x - 2;
		float roomMinY = m_aiActor.ParentRoom.area.UnitBottomLeft.y;
		float roomMaxY = m_aiActor.ParentRoom.area.UnitTopRight.y;
		int minTilesAbovePlayer = MinTilesAbovePlayer;
		int minTilesBelowPlayer = MinTilesBelowPlayer;
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < m_aiActor.Clearance.x; i++)
			{
				int x = c.x + i;
				for (int j = 0; j < m_aiActor.Clearance.y; j++)
				{
					int y = c.y + j;
					if (GameManager.Instance.Dungeon.data.isTopWall(x, y))
					{
						return false;
					}
				}
			}
			float num = (float)c.y - maxPlayerY;
			float num2 = minPlayerY - (float)c.y;
			bool flag = num >= (float)minTilesAbovePlayer && num <= (float)MaxTilesAbovePlayer;
			bool flag2 = UseSouthWall && num2 >= (float)minTilesBelowPlayer && num <= (float)MaxTilesBelowPlayer;
			if (!flag && !flag2)
			{
				return false;
			}
			if (MinDistanceFromPlayer > 0)
			{
				PixelCollider hitboxPixelCollider = m_aiActor.specRigidbody.HitboxPixelCollider;
				Vector2 vector = new Vector2((float)c.x + 0.5f * ((float)m_aiActor.Clearance.x - hitboxPixelCollider.UnitWidth), c.y);
				Vector2 aMax = vector + hitboxPixelCollider.UnitDimensions;
				if (MinDistanceFromPlayer > 0)
				{
					if (BraveMathCollege.AABBDistanceSquared(vector, aMax, playerLowerLeft, playerUpperRight) < minDistanceFromPlayerSquared)
					{
						return false;
					}
					if (hasOtherPlayer && BraveMathCollege.AABBDistanceSquared(vector, aMax, otherPlayerLowerLeft, otherPlayerUpperRight) < minDistanceFromPlayerSquared)
					{
						return false;
					}
				}
			}
			for (int k = minDx; k <= maxDx; k++)
			{
				if (GameManager.Instance.Dungeon.data.isWall(c.x + k, c.y))
				{
					return false;
				}
			}
			if (roomMaxY - (float)c.y < (float)(MinDistFromNorthWall + 1))
			{
				return false;
			}
			return (!UseSouthWall || !((float)c.y - roomMinY < (float)MinDistFromSouthWall)) ? true : false;
		};
		IntVector2? randomAvailableCell = m_aiActor.ParentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, cellValidator);
		if (!randomAvailableCell.HasValue)
		{
			minTilesAbovePlayer = 0;
			minTilesBelowPlayer = 0;
			randomAvailableCell = m_aiActor.ParentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, cellValidator);
		}
		if (randomAvailableCell.HasValue)
		{
			m_targetPoint = randomAvailableCell.Value.ToCenterVector2();
			return;
		}
		Debug.LogWarning("AGUNIM MOVE FAILED!", m_aiActor);
		m_targetPoint = m_aiActor.specRigidbody.UnitCenter;
	}

	private void BeginState(MoveState state)
	{
		switch (state)
		{
		case MoveState.PreMove:
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_aiAnimator.PlayUntilCancelled(preMoveAnimation);
			if (DisableCollisionDuringMove)
			{
				m_aiActor.specRigidbody.CollideWithOthers = false;
				m_aiActor.IsGone = true;
			}
			break;
		case MoveState.Move:
		{
			m_startPoint = m_aiActor.specRigidbody.UnitCenter;
			UpdateTargetPoint();
			Vector2 vector = m_targetPoint - m_startPoint;
			float magnitude = vector.magnitude;
			m_moveTime = MoveTime;
			if (MinSpeed > 0f)
			{
				m_moveTime = Mathf.Min(m_moveTime, magnitude / MinSpeed);
			}
			if (MaxSpeed > 0f)
			{
				m_moveTime = Mathf.Max(m_moveTime, magnitude / MaxSpeed);
			}
			m_aiAnimator.FacingDirection = vector.ToAngle();
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.PlayUntilCancelled(moveAnimation);
			m_setupTimer = 0f;
			if (DisableCollisionDuringMove)
			{
				m_aiActor.specRigidbody.CollideWithOthers = false;
				m_aiActor.IsGone = true;
			}
			if (enableShadowTrail)
			{
				m_shadowTrail.spawnShadows = true;
			}
			break;
		}
		case MoveState.PostMove:
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_aiAnimator.PlayUntilCancelled(postMoveAnimation);
			if (DisableCollisionDuringMove)
			{
				m_aiActor.specRigidbody.CollideWithOthers = true;
				m_aiActor.IsGone = false;
			}
			break;
		}
	}

	private void EndState(MoveState state)
	{
		if (state == MoveState.Move && enableShadowTrail)
		{
			m_shadowTrail.spawnShadows = false;
		}
	}
}
