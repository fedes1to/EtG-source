using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class TakeCoverBehavior : MovementBehaviorBase
{
	private enum CoverState
	{
		Disinterested,
		MovingToCover,
		InCover,
		PopOut
	}

	protected static FlippableCover[] allCover;

	protected static HashSet<FlippableCover> ClaimedCover = new HashSet<FlippableCover>();

	public float PathInterval = 0.25f;

	public bool LineOfSightToLeaveCover = true;

	public float MaxCoverDistance = 10f;

	public float MaxCoverDistanceToTarget = 10f;

	public float FlipCoverDistance = 1f;

	public float InsideCoverTime = 2f;

	public float OutsideCoverTime = 1f;

	public float PopOutSpeedMultiplier = 1f;

	public float PopInSpeedMultiplier = 1f;

	public float InitialCoverChance = 0.33f;

	public float RepeatingCoverChance = 0.05f;

	public float RepeatingCoverInterval = 1f;

	private CoverState m_state;

	private int m_tableQuadrant;

	private float m_repathTimer;

	private float m_coverTimer;

	private float m_seekTimer;

	private float m_failedLineOfSightTimer;

	private float m_cachedSpeed;

	private FlippableCover m_claimedCover;

	private Vector2 m_coverPosition;

	private Vector2 m_popOutPosition;

	private string[] coverAnimations = new string[4] { "cover_idle_right", "cover_idle_right", "cover_idle_left", "cover_idle_left" };

	private string[] emergeAnimations = new string[4] { "cover_leap_right", "cover_leap_right", "cover_leap_left", "cover_leap_left" };

	private bool LastEnemyAndCantSeePlayer
	{
		get
		{
			return m_aiActor.ParentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) == 1 && m_failedLineOfSightTimer > 1f;
		}
	}

	private DungeonData.Direction DesiredFlipDirection
	{
		get
		{
			if (m_tableQuadrant == 0)
			{
				return DungeonData.Direction.SOUTH;
			}
			if (m_tableQuadrant == 1)
			{
				return DungeonData.Direction.WEST;
			}
			if (m_tableQuadrant == 2)
			{
				return DungeonData.Direction.NORTH;
			}
			if (m_tableQuadrant == 3)
			{
				return DungeonData.Direction.EAST;
			}
			Debug.LogError("Unknown flip direction!");
			return DungeonData.Direction.NORTH;
		}
	}

	public static void ClearPerLevelData()
	{
		allCover = null;
		ClaimedCover.Clear();
	}

	public override void Start()
	{
		if (allCover == null || allCover.Length == 0)
		{
			allCover = Object.FindObjectsOfType<FlippableCover>();
		}
		m_cachedSpeed = m_aiActor.MovementSpeed;
		m_state = CoverState.Disinterested;
		if (Random.value < InitialCoverChance)
		{
			SearchForCover();
			m_seekTimer = RepeatingCoverInterval;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		DecrementTimer(ref m_coverTimer);
		DecrementTimer(ref m_seekTimer);
	}

	public override void Destroy()
	{
		base.Destroy();
		if (m_claimedCover != null)
		{
			ClaimedCover.Remove(m_claimedCover);
		}
	}

	public override BehaviorResult Update()
	{
		if (m_aiActor.TargetRigidbody == null)
		{
			return BehaviorResult.Continue;
		}
		m_aiShooter.OverrideAimPoint = null;
		bool flag = m_aiActor.CanTargetEnemies && !m_aiActor.CanTargetPlayers;
		if (m_state == CoverState.Disinterested)
		{
			if (flag || LastEnemyAndCantSeePlayer)
			{
				return BehaviorResult.Continue;
			}
			if (m_seekTimer == 0f)
			{
				m_seekTimer = RepeatingCoverInterval;
				if (Random.value < RepeatingCoverChance)
				{
					SearchForCover();
					if (m_claimedCover != null)
					{
						return BehaviorResult.SkipRemainingClassBehaviors;
					}
				}
			}
			return BehaviorResult.Continue;
		}
		bool flag2 = !m_claimedCover || m_claimedCover.IsBroken;
		int tableQuadrant = m_tableQuadrant;
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 unitCenter2 = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		Vector2? vector = ((!flag2) ? CalculateCoverPosition(unitCenter2) : null);
		bool flag3 = false;
		if ((bool)m_claimedCover)
		{
			flag3 = Vector2.Distance(m_claimedCover.specRigidbody.UnitCenter, unitCenter2) >= MaxCoverDistanceToTarget;
		}
		bool flag4 = m_state == CoverState.InCover && m_coverTimer <= 0f && LastEnemyAndCantSeePlayer;
		if (flag2 || !vector.HasValue || flag || m_aiActor.aiAnimator.FpsScale < 1f || flag3 || flag4)
		{
			BecomeDisinterested(tableQuadrant);
			return BehaviorResult.Continue;
		}
		if (m_state != CoverState.PopOut)
		{
			m_coverPosition = vector.Value;
		}
		if ((bool)m_claimedCover && !m_claimedCover.IsFlipped && Vector2.Distance(m_coverPosition, unitCenter) < FlipCoverDistance && DesiredFlipDirection == m_claimedCover.GetFlipDirection(m_aiActor.specRigidbody))
		{
			m_claimedCover.Flip(m_aiActor.specRigidbody);
		}
		if (m_state == CoverState.MovingToCover)
		{
			if (m_repathTimer == 0f)
			{
				if (Vector2.Distance(unitCenter, m_coverPosition) > PhysicsEngine.PixelToUnit(2) && !m_aiActor.PathfindToPosition(m_coverPosition, m_coverPosition))
				{
					BecomeDisinterested(tableQuadrant);
					m_aiActor.ClearPath();
					return BehaviorResult.Continue;
				}
				m_repathTimer = PathInterval;
			}
			if (m_aiActor.PathComplete)
			{
				m_state = CoverState.InCover;
				m_coverTimer = InsideCoverTime;
				m_repathTimer = 0f;
				m_failedLineOfSightTimer = 0f;
			}
		}
		else if (m_state == CoverState.InCover)
		{
			if (!m_aiActor.HasLineOfSightToTarget)
			{
				m_aiShooter.OverrideAimDirection = IntVector2.Cardinals[m_tableQuadrant / 2 * 2 + 1].ToVector2();
			}
			if (m_coverTimer == 0f)
			{
				m_popOutPosition = CalculatePopOutPosition(unitCenter2);
				if (!LineOfSightToLeaveCover || m_aiActor.HasLineOfSightToTargetFromPosition(m_popOutPosition))
				{
					m_state = CoverState.PopOut;
					m_coverTimer = OutsideCoverTime;
					m_repathTimer = 0f;
					m_aiActor.MovementSpeed = m_cachedSpeed * PopOutSpeedMultiplier;
					m_aiAnimator.PlayForDuration(emergeAnimations[m_tableQuadrant], OutsideCoverTime);
					return BehaviorResult.SkipRemainingClassBehaviors;
				}
				if (LineOfSightToLeaveCover)
				{
					m_failedLineOfSightTimer += m_deltaTime;
				}
			}
			if (m_repathTimer == 0f)
			{
				if (Vector2.Distance(unitCenter, m_coverPosition) > PhysicsEngine.PixelToUnit(2))
				{
					bool flag5 = m_aiActor.PathfindToPosition(m_coverPosition, m_coverPosition);
					m_aiAnimator.EndAnimationIf(coverAnimations[tableQuadrant]);
					if (!flag5)
					{
						BecomeDisinterested(tableQuadrant);
						m_aiActor.ClearPath();
						return BehaviorResult.Continue;
					}
				}
				m_repathTimer = PathInterval;
			}
			if (m_aiActor.PathComplete && !m_aiActor.spriteAnimator.IsPlaying(coverAnimations[m_tableQuadrant]))
			{
				m_aiAnimator.PlayUntilFinished(coverAnimations[m_tableQuadrant]);
			}
		}
		else if (m_state == CoverState.PopOut)
		{
			if (m_coverTimer == 0f)
			{
				m_state = CoverState.InCover;
				m_coverTimer = InsideCoverTime;
				m_repathTimer = 0f;
				m_failedLineOfSightTimer = 0f;
				m_aiActor.MovementSpeed = m_cachedSpeed * PopInSpeedMultiplier;
			}
			else if (m_repathTimer == 0f)
			{
				Vector2 unitCenter3 = m_aiActor.specRigidbody.UnitCenter;
				if (Vector2.Distance(unitCenter3, m_popOutPosition) < 2f)
				{
					m_aiActor.FakePathToPosition(m_popOutPosition);
				}
				else if (!m_aiActor.PathfindToPosition(m_popOutPosition, m_popOutPosition))
				{
					BecomeDisinterested(tableQuadrant);
					m_aiActor.ClearPath();
					return BehaviorResult.Continue;
				}
				m_repathTimer = PathInterval;
			}
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	protected void SearchForCover()
	{
		if (m_claimedCover != null)
		{
			ClaimedCover.Remove(m_claimedCover);
		}
		m_claimedCover = null;
		if (!m_aiActor.TargetRigidbody)
		{
			return;
		}
		RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		Vector2 unitCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		float num = float.MaxValue;
		for (int i = 0; i < allCover.Length; i++)
		{
			if (!allCover[i] || allCover[i].IsBroken || ClaimedCover.Contains(allCover[i]))
			{
				continue;
			}
			RoomHandler roomFromPosition2 = GameManager.Instance.Dungeon.GetRoomFromPosition(allCover[i].transform.position.IntXY(VectorConversions.Floor));
			if (roomFromPosition == roomFromPosition2)
			{
				float num2 = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, allCover[i].specRigidbody.UnitCenter);
				float num3 = Vector2.Distance(allCover[i].specRigidbody.UnitCenter, unitCenter);
				if (num2 < MaxCoverDistance && num2 < num && num3 < MaxCoverDistanceToTarget)
				{
					num = num2;
					m_claimedCover = allCover[i];
				}
			}
		}
		if (m_claimedCover != null)
		{
			ClaimedCover.Add(m_claimedCover);
			m_repathTimer = 0f;
			m_state = CoverState.MovingToCover;
		}
	}

	protected Vector2? CalculateCoverPosition(Vector2 targetPosition)
	{
		Vector2? result = null;
		PixelCollider primaryPixelCollider = m_aiActor.specRigidbody.PrimaryPixelCollider;
		PixelCollider pixelCollider = m_aiActor.specRigidbody[CollisionLayer.EnemyHitBox];
		PixelCollider pixelCollider2 = ((!m_claimedCover.IsFlipped) ? m_claimedCover.specRigidbody.PrimaryPixelCollider : m_claimedCover.specRigidbody[CollisionLayer.LowObstacle]);
		Vector2 unitCenter = pixelCollider2.UnitCenter;
		Vector2 vector = targetPosition - unitCenter;
		Vector2 vector2 = BraveUtility.GetMajorAxis(vector);
		for (int i = 0; i < 2; vector2 = BraveUtility.GetMinorAxis(vector), i++)
		{
			Vector2 vector3;
			if (vector2.x != 0f)
			{
				if (pixelCollider2.Height < pixelCollider.Height)
				{
					continue;
				}
				vector3 = new Vector2((!(vector2.x > 0f)) ? Mathf.Ceil(pixelCollider2.UnitRight) : Mathf.Floor(pixelCollider2.UnitLeft), pixelCollider2.UnitCenter.y);
				m_tableQuadrant = ((!(vector2.x > 0f)) ? 1 : 3);
			}
			else
			{
				if (pixelCollider2.Width < pixelCollider.Width)
				{
					continue;
				}
				vector3 = new Vector2(pixelCollider2.UnitCenter.x, (!(vector2.y > 0f)) ? Mathf.Ceil(pixelCollider2.UnitTop) : Mathf.Floor(pixelCollider2.UnitBottom));
				m_tableQuadrant = ((vector2.y > 0f) ? 2 : 0);
			}
			result = vector3 + Vector2.Scale(-vector2, primaryPixelCollider.UnitDimensions / 2f);
			break;
		}
		if (!result.HasValue)
		{
			Debug.LogError("Didn't find a valid cover position!");
			return m_claimedCover.transform.position.XY();
		}
		return result;
	}

	protected Vector2 CalculatePopOutPosition(Vector2 targetPosition)
	{
		PixelCollider primaryPixelCollider = m_aiActor.specRigidbody.PrimaryPixelCollider;
		PixelCollider hitboxPixelCollider = m_aiActor.specRigidbody.HitboxPixelCollider;
		PixelCollider pixelCollider = m_claimedCover.specRigidbody[CollisionLayer.BulletBlocker];
		Vector2 coverPosition = m_coverPosition;
		Vector2 vector = targetPosition - pixelCollider.UnitCenter;
		Vector2 vector2 = primaryPixelCollider.UnitDimensions / 2f;
		if (m_tableQuadrant == 0 || m_tableQuadrant == 2)
		{
			coverPosition.x = ((!(vector.x < 0f)) ? (pixelCollider.UnitRight + vector2.x) : (pixelCollider.UnitLeft - vector2.x));
		}
		else
		{
			coverPosition.y = ((!(vector.y < 0f)) ? (pixelCollider.UnitTop + vector2.y) : (pixelCollider.UnitBottom - hitboxPixelCollider.UnitDimensions.y + vector2.y));
		}
		return coverPosition;
	}

	private void BecomeDisinterested(int previousTableQuadrant)
	{
		m_state = CoverState.Disinterested;
		m_seekTimer = RepeatingCoverInterval;
		m_aiShooter.OverrideAimPoint = null;
		m_aiActor.MovementSpeed = m_cachedSpeed;
		m_aiAnimator.EndAnimationIf(coverAnimations[previousTableQuadrant]);
		m_aiAnimator.EndAnimationIf(emergeAnimations[previousTableQuadrant]);
	}
}
