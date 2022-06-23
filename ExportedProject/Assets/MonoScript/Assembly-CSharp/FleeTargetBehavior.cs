using System;
using Dungeonator;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class FleeTargetBehavior : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	public float CloseDistance = 9f;

	public float CloseTime = 3f;

	public float TooCloseDistance = 6f;

	public bool TooCloseLOS = true;

	public float DesiredDistance = 20f;

	public int PlayerPersonalSpace;

	public bool CanAttackWhileMoving;

	public bool ManuallyDefineRoom;

	[InspectorShowIf("ManuallyDefineRoom")]
	public Vector2 roomMin;

	[InspectorShowIf("ManuallyDefineRoom")]
	public Vector2 roomMax;

	[NonSerialized]
	public bool ForceRun;

	private float m_repathTimer;

	private float m_closeTimer;

	private bool m_wasDamaged;

	private bool m_shouldRun;

	private SpeculativeRigidbody m_otherTargetRigidbody;

	private IntVector2? m_targetPos;

	private IntVector2 m_cachedPlayerCell;

	private IntVector2? m_cachedOtherPlayerCell;

	public override void Start()
	{
		if ((bool)m_aiActor && (bool)m_aiActor.healthHaver)
		{
			m_aiActor.healthHaver.OnDamaged += OnDamaged;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		DecrementTimer(ref m_closeTimer);
		if (m_aiActor.DistanceToTarget > CloseDistance)
		{
			m_closeTimer = CloseTime;
		}
		m_shouldRun = false;
		if (m_wasDamaged)
		{
			m_shouldRun = true;
			m_wasDamaged = false;
		}
		m_otherTargetRigidbody = null;
		if (m_aiActor.PlayerTarget is PlayerController && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(m_aiActor.PlayerTarget as PlayerController);
			if ((bool)otherPlayer && (bool)otherPlayer.healthHaver && otherPlayer.healthHaver.IsAlive)
			{
				m_otherTargetRigidbody = otherPlayer.specRigidbody;
			}
		}
	}

	public override bool OverrideOtherBehaviors()
	{
		return ShouldRun();
	}

	public override BehaviorResult Update()
	{
		IntVector2? targetPos = m_targetPos;
		if (!targetPos.HasValue && m_repathTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		IntVector2? targetPos2 = m_targetPos;
		if (targetPos2.HasValue && m_aiActor.PathComplete)
		{
			m_targetPos = null;
		}
		IntVector2? targetPos3 = m_targetPos;
		if (!targetPos3.HasValue && (bool)m_aiActor.TargetRigidbody && ShouldRun() && m_aiActor.ParentRoom != null)
		{
			RoomHandler parentRoom = m_aiActor.ParentRoom;
			m_targetPos = parentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, CellValidator);
			IntVector2? targetPos4 = m_targetPos;
			if (!targetPos4.HasValue)
			{
				m_targetPos = parentRoom.GetRandomWeightedAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, CellValidator, CellWeighter);
			}
			m_repathTimer = 0f;
			m_closeTimer = 0f;
			ForceRun = false;
		}
		if (m_repathTimer <= 0f)
		{
			IntVector2? targetPos5 = m_targetPos;
			if (targetPos5.HasValue && (bool)m_aiActor.TargetRigidbody)
			{
				m_repathTimer = PathInterval;
				m_cachedPlayerCell = m_aiActor.TargetRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
				m_cachedOtherPlayerCell = ((!m_otherTargetRigidbody) ? null : new IntVector2?(m_otherTargetRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor)));
				m_aiActor.PathfindToPosition(m_targetPos.Value.ToCenterVector2(), null, true, null, CellPathingWeighter);
			}
		}
		IntVector2? targetPos6 = m_targetPos;
		if (!targetPos6.HasValue)
		{
			return BehaviorResult.Continue;
		}
		return CanAttackWhileMoving ? BehaviorResult.SkipRemainingClassBehaviors : BehaviorResult.SkipAllRemainingBehaviors;
	}

	private bool CellValidator(IntVector2 c)
	{
		if (ManuallyDefineRoom && ((float)c.x < roomMin.x || (float)c.x > roomMax.x || (float)c.y < roomMin.y || (float)c.y > roomMax.y))
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
		if (Vector2.Distance(Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance), m_aiActor.TargetRigidbody.UnitCenter) < DesiredDistance)
		{
			return false;
		}
		if ((bool)m_otherTargetRigidbody && Vector2.Distance(Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance), m_otherTargetRigidbody.UnitCenter) < DesiredDistance)
		{
			return false;
		}
		return true;
	}

	private float CellWeighter(IntVector2 c)
	{
		for (int i = 0; i < m_aiActor.Clearance.x; i++)
		{
			for (int j = 0; j < m_aiActor.Clearance.y; j++)
			{
				if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
				{
					return 1000000f;
				}
			}
		}
		float num = Vector2.Distance(Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance), m_aiActor.TargetRigidbody.UnitCenter);
		if ((bool)m_otherTargetRigidbody)
		{
			num = Mathf.Min(num, Vector2.Distance(Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance), m_otherTargetRigidbody.UnitCenter));
		}
		return num;
	}

	private int CellPathingWeighter(IntVector2 prevStep, IntVector2 thisStep)
	{
		if (IntVector2.Distance(thisStep, m_cachedPlayerCell) < (float)PlayerPersonalSpace)
		{
			return 100;
		}
		IntVector2? cachedOtherPlayerCell = m_cachedOtherPlayerCell;
		if (cachedOtherPlayerCell.HasValue && IntVector2.Distance(thisStep, m_cachedOtherPlayerCell.Value) < (float)PlayerPersonalSpace)
		{
			return 100;
		}
		return 0;
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		m_wasDamaged = true;
	}

	private bool ShouldRun()
	{
		if (m_shouldRun || ForceRun)
		{
			return true;
		}
		float num = m_aiActor.DistanceToTarget;
		if (m_aiActor.PlayerTarget is PlayerController && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(m_aiActor.PlayerTarget as PlayerController);
			if ((bool)otherPlayer && (bool)otherPlayer.healthHaver && otherPlayer.healthHaver.IsAlive)
			{
				float b = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, otherPlayer.specRigidbody.GetUnitCenter(ColliderType.HitBox));
				num = Mathf.Min(num, b);
			}
		}
		if (num < TooCloseDistance && (!TooCloseLOS || m_aiActor.HasLineOfSightToTarget))
		{
			return true;
		}
		if (num < CloseDistance && m_closeTimer <= 0f)
		{
			return true;
		}
		return false;
	}
}
