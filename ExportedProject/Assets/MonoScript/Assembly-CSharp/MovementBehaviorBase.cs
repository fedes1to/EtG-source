using Dungeonator;
using Pathfinding;
using UnityEngine;

public abstract class MovementBehaviorBase : BehaviorBase
{
	private static float FleePathInterval = 0.25f;

	private BehaviorSpeculator m_behaviorSpeculator;

	private tk2dSpriteAnimator m_extantFearVFX;

	private float m_fleeRepathTimer;

	private bool m_isFleeing;

	public virtual float DesiredCombatDistance
	{
		get
		{
			return -1f;
		}
	}

	public virtual bool AllowFearRunState
	{
		get
		{
			return false;
		}
	}

	public override void Start()
	{
		base.Start();
		m_behaviorSpeculator = m_aiActor.behaviorSpeculator;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_fleeRepathTimer);
		UpdateFearVFX();
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (ShouldFleePlayer())
		{
			if (!m_isFleeing && m_behaviorSpeculator.IsInterruptable)
			{
				m_behaviorSpeculator.Interrupt();
			}
			m_isFleeing = true;
			UpdateFearVFX();
			FleePlayerData fleeData = m_behaviorSpeculator.FleePlayerData;
			if (m_fleeRepathTimer <= 0f)
			{
				Vector2 pointOfFear = fleeData.Player.CenterPosition;
				CellValidator cellValidator = (IntVector2 p) => Vector2.Distance(p.ToCenterVector2(), pointOfFear) > fleeData.StopDistance;
				CellTypes cellTypes = m_aiActor.PathableTiles;
				if (m_aiActor.DistanceToTarget < fleeData.DeathDistance)
				{
					cellTypes |= CellTypes.PIT;
				}
				IntVector2? nearestAvailableCell = m_aiActor.ParentRoom.GetNearestAvailableCell(m_aiActor.specRigidbody.UnitCenter, m_aiActor.Clearance, cellTypes, false, cellValidator);
				if (nearestAvailableCell.HasValue)
				{
					AIActor aiActor = m_aiActor;
					Vector2 targetPosition = nearestAvailableCell.Value.ToCenterVector2();
					CellTypes? overridePathableTiles = cellTypes;
					aiActor.PathfindToPosition(targetPosition, null, true, null, null, overridePathableTiles);
					m_fleeRepathTimer = FleePathInterval;
				}
				else
				{
					m_aiActor.ClearPath();
				}
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		m_isFleeing = false;
		UpdateFearVFX();
		return base.Update();
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		m_isFleeing = false;
		if (m_extantFearVFX != null)
		{
			SpawnManager.Despawn(m_extantFearVFX.gameObject);
			m_extantFearVFX = null;
		}
	}

	private bool ShouldFleePlayer()
	{
		if (m_behaviorSpeculator == null)
		{
			return false;
		}
		FleePlayerData fleePlayerData = m_behaviorSpeculator.FleePlayerData;
		if (fleePlayerData == null || !fleePlayerData.Player)
		{
			return false;
		}
		float num = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, fleePlayerData.Player.CenterPosition);
		return (m_isFleeing && num < fleePlayerData.StopDistance) || num < fleePlayerData.StartDistance;
	}

	protected virtual void UpdateFearVFX()
	{
		if (!m_isFleeing && m_extantFearVFX != null)
		{
			if (m_extantFearVFX.IsPlaying("fear_face_vfx"))
			{
				m_extantFearVFX.Play("fear_face_vfx_out");
			}
			else if (!m_extantFearVFX.Playing)
			{
				SpawnManager.Despawn(m_extantFearVFX.gameObject);
				m_extantFearVFX = null;
			}
		}
		else if (m_isFleeing && m_extantFearVFX == null)
		{
			m_extantFearVFX = m_aiActor.PlayEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_Fear") as GameObject, (m_aiActor.sprite.WorldTopCenter - m_aiActor.CenterPosition).WithX(0f), true, true).GetComponent<tk2dSpriteAnimator>();
		}
		else if (m_isFleeing && m_extantFearVFX != null)
		{
			if (!m_extantFearVFX.IsPlaying("fear_face_vfx"))
			{
				m_extantFearVFX.Play("fear_face_vfx");
			}
			m_extantFearVFX.transform.position = m_aiActor.sprite.WorldTopCenter.ToVector3ZUp(m_extantFearVFX.transform.position.z);
		}
	}
}
