using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class TargetPlayerBehavior : TargetBehaviorBase
{
	public float Radius = 10f;

	public bool LineOfSight = true;

	public bool ObjectPermanence = true;

	public float SearchInterval = 0.25f;

	public bool PauseOnTargetSwitch;

	[InspectorShowIf("PauseOnTargetSwitch")]
	public float PauseTime = 0.25f;

	private const float PLAYER_REFRESH_TIMER = 1f;

	private float m_losTimer;

	private float m_coopRefreshSearchTimer;

	private float m_prevDistToTarget;

	private PlayerController m_previousPlayer;

	private SpeculativeRigidbody m_specRigidbody;

	private BehaviorSpeculator m_behaviorSpeculator;

	public override void Start()
	{
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_losTimer);
		DecrementTimer(ref m_coopRefreshSearchTimer);
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
		if ((bool)m_behaviorSpeculator.PlayerTarget)
		{
			if (m_behaviorSpeculator.PlayerTarget.IsFalling)
			{
				m_behaviorSpeculator.PlayerTarget = null;
				if ((bool)m_aiActor)
				{
					m_aiActor.ClearPath();
				}
				return BehaviorResult.SkipRemainingClassBehaviors;
			}
			if ((bool)m_behaviorSpeculator.PlayerTarget.healthHaver && (m_behaviorSpeculator.PlayerTarget.healthHaver.IsDead || m_behaviorSpeculator.PlayerTarget.healthHaver.PreventAllDamage))
			{
				m_behaviorSpeculator.PlayerTarget = null;
				if ((bool)m_aiActor)
				{
					m_aiActor.ClearPath();
				}
				return BehaviorResult.SkipRemainingClassBehaviors;
			}
		}
		else
		{
			m_behaviorSpeculator.PlayerTarget = null;
		}
		if (!ObjectPermanence)
		{
			m_behaviorSpeculator.PlayerTarget = null;
		}
		if (m_behaviorSpeculator.PlayerTarget != null && m_behaviorSpeculator.PlayerTarget.IsStealthed)
		{
			m_behaviorSpeculator.PlayerTarget = null;
		}
		if (GameManager.Instance.AllPlayers.Length > 1 && m_coopRefreshSearchTimer <= 0f)
		{
			m_behaviorSpeculator.PlayerTarget = null;
		}
		if (m_behaviorSpeculator.PlayerTarget is AIActor)
		{
			float num = Vector2.Distance(m_specRigidbody.UnitCenter, m_behaviorSpeculator.PlayerTarget.specRigidbody.UnitCenter);
			if (m_prevDistToTarget + 3f < num)
			{
				m_behaviorSpeculator.PlayerTarget = null;
			}
			m_prevDistToTarget = num;
			if ((bool)m_aiActor && !m_aiActor.IsNormalEnemy && (bool)m_aiActor.CompanionOwner && m_behaviorSpeculator.PlayerTarget is AIActor && m_behaviorSpeculator.PlayerTarget.GetAbsoluteParentRoom() != m_aiActor.CompanionOwner.CurrentRoom)
			{
				m_behaviorSpeculator.PlayerTarget = null;
			}
		}
		if (m_behaviorSpeculator.PlayerTarget != null)
		{
			return BehaviorResult.Continue;
		}
		PlayerController playerController = GameManager.Instance.GetActivePlayerClosestToPoint(m_specRigidbody.UnitCenter);
		if ((bool)m_aiActor && m_aiActor.SuppressTargetSwitch)
		{
			playerController = m_previousPlayer;
		}
		if (!m_aiActor || (m_aiActor.CanTargetPlayers && !m_aiActor.CanTargetEnemies))
		{
			if (playerController == null)
			{
				return BehaviorResult.Continue;
			}
			m_behaviorSpeculator.PlayerTarget = playerController;
			if (GameManager.Instance.AllPlayers.Length > 1)
			{
				m_coopRefreshSearchTimer = 1f;
			}
		}
		else if (m_aiActor.CanTargetEnemies && !m_aiActor.CanTargetPlayers)
		{
			List<AIActor> activeEnemies = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(m_aiActor.GridPosition).GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null && activeEnemies.Count > 0)
			{
				AIActor aIActor = null;
				float num2 = -1f;
				if (!m_aiActor || m_aiActor.IsNormalEnemy || !m_aiActor.CompanionOwner || !m_aiActor.CompanionOwner.IsStealthed)
				{
					for (int i = 0; i < activeEnemies.Count; i++)
					{
						AIActor aIActor2 = activeEnemies[i];
						if ((bool)aIActor2 && aIActor2.IsNormalEnemy && !aIActor2.IsGone && !aIActor2.IsHarmlessEnemy && !(aIActor2 == m_aiActor) && (!aIActor2.healthHaver || !aIActor2.healthHaver.PreventAllDamage))
						{
							float num3 = Vector2.Distance(m_specRigidbody.UnitCenter, aIActor2.specRigidbody.UnitCenter);
							if (aIActor == null || num3 < num2)
							{
								aIActor = aIActor2;
								num2 = num3;
							}
						}
					}
				}
				if ((bool)aIActor)
				{
					m_behaviorSpeculator.PlayerTarget = aIActor;
					m_prevDistToTarget = num2;
				}
			}
		}
		else if (m_aiActor.CanTargetEnemies && !m_aiActor.CanTargetPlayers)
		{
		}
		if (m_aiShooter != null && m_behaviorSpeculator.PlayerTarget != null)
		{
			m_aiShooter.AimAtPoint(m_behaviorSpeculator.PlayerTarget.CenterPosition);
		}
		if ((bool)m_aiActor && PauseOnTargetSwitch && m_aiActor.HasBeenEngaged && (bool)m_previousPlayer && (bool)playerController && m_previousPlayer != playerController)
		{
			m_aiActor.behaviorSpeculator.AttackCooldown = Mathf.Max(m_aiActor.behaviorSpeculator.AttackCooldown, PauseTime);
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		m_previousPlayer = playerController;
		if ((bool)m_aiActor && !m_aiActor.HasBeenEngaged)
		{
			m_aiActor.HasBeenEngaged = true;
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		m_specRigidbody = gameObject.GetComponent<SpeculativeRigidbody>();
		m_behaviorSpeculator = gameObject.GetComponent<BehaviorSpeculator>();
	}
}
