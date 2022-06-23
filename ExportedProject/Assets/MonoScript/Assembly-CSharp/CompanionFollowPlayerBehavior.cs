using System;
using Dungeonator;
using UnityEngine;

public class CompanionFollowPlayerBehavior : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	public bool DisableInCombat = true;

	public float IdealRadius = 3f;

	public float CatchUpRadius = 7f;

	public float CatchUpAccelTime = 5f;

	public float CatchUpSpeed = 7f;

	public float CatchUpMaxSpeed = 10f;

	public string CatchUpAnimation;

	public string CatchUpOutAnimation;

	public string[] IdleAnimations;

	public bool CanRollOverPits;

	public string RollAnimation = "roll";

	private bool m_isCatchingUp;

	private float m_catchUpTime;

	[NonSerialized]
	public bool TemporarilyDisabled;

	protected bool m_triedToPathOverPit;

	protected bool m_wasOverPit;

	protected bool m_groundRolling;

	private int m_sequentialPathFails;

	private float m_idleTimer = 2f;

	private float m_repathTimer;

	private CompanionController m_companionController;

	public override void Start()
	{
		base.Start();
		m_companionController = m_gameObject.GetComponent<CompanionController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	private void CatchUpMovementModifier(ref Vector2 voluntaryVel, ref Vector2 involuntaryVel)
	{
		if (DisableInCombat)
		{
			PlayerController playerController = GameManager.Instance.PrimaryPlayer;
			if ((bool)m_aiActor && (bool)m_aiActor.CompanionOwner)
			{
				playerController = m_aiActor.CompanionOwner;
			}
			if ((bool)playerController && playerController.IsInCombat && Vector2.Distance(playerController.CenterPosition, m_aiActor.CenterPosition) < CatchUpRadius)
			{
				m_isCatchingUp = false;
				if (!string.IsNullOrEmpty(CatchUpOutAnimation))
				{
					m_aiAnimator.PlayUntilFinished(CatchUpOutAnimation);
				}
				m_aiActor.MovementModifiers -= CatchUpMovementModifier;
				return;
			}
		}
		m_catchUpTime += m_aiActor.LocalDeltaTime;
		voluntaryVel = voluntaryVel.normalized * Mathf.Lerp(CatchUpSpeed, CatchUpMaxSpeed, m_catchUpTime / CatchUpAccelTime);
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (!m_aiAnimator.IsPlaying(RollAnimation))
		{
			return ContinuousBehaviorResult.Finished;
		}
		if (m_aiAnimator.CurrentClipProgress > 0.7f)
		{
			m_aiActor.FallingProhibited = false;
			m_aiActor.BehaviorVelocity = m_aiActor.BehaviorVelocity.normalized * 2f;
		}
		return base.ContinuousUpdate();
	}

	public override void EndContinuousUpdate()
	{
		m_updateEveryFrame = false;
		m_triedToPathOverPit = false;
		m_groundRolling = false;
		m_aiActor.FallingProhibited = false;
		m_aiActor.BehaviorOverridesVelocity = false;
		base.EndContinuousUpdate();
	}

	public override BehaviorResult Update()
	{
		if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel)
		{
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			m_aiActor.ClearPath();
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		if (TemporarilyDisabled)
		{
			return BehaviorResult.Continue;
		}
		DecrementTimer(ref m_idleTimer);
		m_aiActor.DustUpInterval = Mathf.Lerp(0.5f, 0.125f, m_aiActor.specRigidbody.Velocity.magnitude / CatchUpSpeed);
		PlayerController playerController = GameManager.Instance.PrimaryPlayer;
		if ((bool)m_aiActor && (bool)m_aiActor.CompanionOwner)
		{
			playerController = m_aiActor.CompanionOwner;
		}
		if (CanRollOverPits && m_triedToPathOverPit)
		{
			if (m_aiActor.IsOverPitAtAll && !m_wasOverPit)
			{
				Debug.Log("running continuous");
				m_aiActor.FallingProhibited = true;
				m_aiAnimator.PlayUntilFinished(RollAnimation);
				Vector2 normalized = m_aiActor.specRigidbody.Velocity.normalized;
				m_aiActor.BehaviorOverridesVelocity = true;
				m_aiActor.BehaviorVelocity = normalized * 7f;
				m_aiActor.ClearPath();
				m_updateEveryFrame = true;
				return BehaviorResult.RunContinuous;
			}
			m_wasOverPit = m_aiActor.IsOverPitAtAll;
		}
		IntVector2 intVector = m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
		CellData cellData = GameManager.Instance.Dungeon.data[intVector];
		if (cellData != null && cellData.IsPlayerInaccessible)
		{
			if (m_repathTimer <= 0f)
			{
				m_repathTimer = PathInterval;
				RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector);
				if (absoluteRoomFromPosition != null)
				{
					IntVector2? nearestAvailableCell = absoluteRoomFromPosition.GetNearestAvailableCell(intVector.ToCenterVector2(), m_aiActor.Clearance, m_aiActor.PathableTiles, false, (IntVector2 pos) => (!GameManager.Instance.Dungeon.data[pos].IsPlayerInaccessible) ? true : false);
					if (nearestAvailableCell.HasValue)
					{
						m_aiActor.PathfindToPosition(nearestAvailableCell.Value.ToCenterVector2());
					}
				}
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		if (!playerController)
		{
			return BehaviorResult.Continue;
		}
		if (!playerController.IsStealthed && playerController.CurrentRoom != null && playerController.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.All) && (bool)m_aiActor.TargetRigidbody && m_aiActor.transform.position.GetAbsoluteRoom() == playerController.CurrentRoom && DisableInCombat)
		{
			IntVector2 intVector2 = ((!m_aiActor.specRigidbody) ? m_aiActor.transform.position.IntXY(VectorConversions.Floor) : m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
			if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector2) && !GameManager.Instance.Dungeon.data[intVector2].isExitCell)
			{
				if (m_isCatchingUp)
				{
					m_isCatchingUp = false;
					if (!string.IsNullOrEmpty(CatchUpOutAnimation))
					{
						m_aiAnimator.PlayUntilFinished(CatchUpOutAnimation);
					}
					m_aiActor.MovementModifiers -= CatchUpMovementModifier;
				}
				return BehaviorResult.Continue;
			}
		}
		bool flag = false;
		if ((bool)m_companionController && m_companionController.IsBeingPet)
		{
			flag = true;
		}
		float num = Vector2.Distance(playerController.CenterPosition, m_aiActor.CenterPosition);
		if (num <= IdealRadius && !flag)
		{
			m_aiActor.ClearPath();
			if (m_isCatchingUp)
			{
				m_isCatchingUp = false;
				if (!string.IsNullOrEmpty(CatchUpOutAnimation))
				{
					m_aiAnimator.PlayUntilFinished(CatchUpOutAnimation);
				}
				m_aiActor.MovementModifiers -= CatchUpMovementModifier;
			}
			if (m_idleTimer <= 0f && IdleAnimations != null && IdleAnimations.Length > 0)
			{
				m_aiAnimator.PlayUntilFinished(IdleAnimations[UnityEngine.Random.Range(0, IdleAnimations.Length)]);
				m_idleTimer = UnityEngine.Random.Range(3, 10);
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		if (num > 30f)
		{
			m_sequentialPathFails = 0;
			m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
		}
		else if (!m_isCatchingUp && num > CatchUpRadius)
		{
			m_isCatchingUp = true;
			m_catchUpTime = 0f;
			if (!string.IsNullOrEmpty(CatchUpAnimation))
			{
				m_aiAnimator.PlayUntilFinished(CatchUpAnimation);
			}
			m_aiActor.MovementModifiers += CatchUpMovementModifier;
		}
		m_idleTimer = Mathf.Max(m_idleTimer, 2f);
		if (m_repathTimer <= 0f && !playerController.IsOverPitAtAll && !playerController.IsInMinecart)
		{
			m_repathTimer = PathInterval;
			m_triedToPathOverPit = false;
			m_aiActor.FallingProhibited = false;
			if (flag)
			{
				Vector2 vector = m_companionController.m_pettingDoer.specRigidbody.UnitCenter + m_companionController.m_petOffset;
				if (Vector2.Distance(vector, m_aiActor.specRigidbody.UnitCenter) < 0.08f)
				{
					m_aiActor.ClearPath();
				}
				else
				{
					m_aiActor.PathfindToPosition(vector, vector);
				}
			}
			else
			{
				m_aiActor.PathfindToPosition(playerController.specRigidbody.UnitCenter);
			}
			if (m_aiActor.Path != null && m_aiActor.Path.InaccurateLength > 50f)
			{
				m_aiActor.ClearPath();
				m_sequentialPathFails = 0;
				m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
			}
			else if (m_aiActor.Path != null && !m_aiActor.Path.WillReachFinalGoal)
			{
				bool flag2 = false;
				if (CanRollOverPits)
				{
					m_aiActor.PathableTiles |= CellTypes.PIT;
					m_aiActor.PathfindToPosition(playerController.specRigidbody.UnitCenter);
					m_aiActor.PathableTiles &= ~CellTypes.PIT;
					if (m_aiActor.Path != null && m_aiActor.Path.WillReachFinalGoal)
					{
						m_triedToPathOverPit = true;
						m_aiActor.FallingProhibited = true;
						flag2 = true;
					}
				}
				if (!flag2)
				{
					m_sequentialPathFails++;
					IntVector2 key = m_aiActor.CompanionOwner.CenterPosition.ToIntVector2(VectorConversions.Floor);
					CellData cellData2 = GameManager.Instance.Dungeon.data[key];
					if (m_sequentialPathFails > 3 && cellData2 != null && cellData2.IsPassable)
					{
						m_sequentialPathFails = 0;
						m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
					}
				}
			}
			else
			{
				m_sequentialPathFails = 0;
			}
		}
		if (!m_aiShooter || (bool)m_aiShooter.EquippedGun)
		{
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
