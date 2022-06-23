using Dungeonator;
using UnityEngine;

public class CopMovementBehavior : MovementBehaviorBase
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

	private bool m_hasIdled;

	private bool m_isCatchingUp;

	private float m_catchUpTime;

	private int m_sequentialPathFails;

	private float m_repathTimer;

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	private void CatchUpMovementModifier(ref Vector2 voluntaryVel, ref Vector2 involuntaryVel)
	{
		m_catchUpTime += m_aiActor.LocalDeltaTime;
		voluntaryVel = voluntaryVel.normalized * Mathf.Lerp(CatchUpSpeed, CatchUpMaxSpeed, m_catchUpTime / CatchUpAccelTime);
	}

	public override BehaviorResult Update()
	{
		m_aiActor.DustUpInterval = Mathf.Lerp(0.5f, 0.125f, m_aiActor.specRigidbody.Velocity.magnitude / CatchUpSpeed);
		PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
		if (!primaryPlayer || primaryPlayer.CurrentRoom == null)
		{
			m_aiActor.ClearPath();
			return BehaviorResult.Continue;
		}
		if (!primaryPlayer.IsStealthed && primaryPlayer.CurrentRoom.IsSealed && m_aiActor.transform.position.GetAbsoluteRoom() == primaryPlayer.CurrentRoom && DisableInCombat)
		{
			IntVector2 intVector = m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
			if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector) && !GameManager.Instance.Dungeon.data[intVector].isExitCell)
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
		float num = Vector2.Distance(primaryPlayer.CenterPosition, m_aiActor.CenterPosition);
		if (num <= IdealRadius)
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
			if (!m_hasIdled && !m_aiAnimator.IsPlaying(CatchUpOutAnimation) && IdleAnimations.Length > 0)
			{
				m_hasIdled = true;
				m_aiAnimator.PlayUntilCancelled(IdleAnimations[Random.Range(0, IdleAnimations.Length)]);
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		if (num > 30f)
		{
			m_sequentialPathFails = 0;
			m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
		}
		else
		{
			m_hasIdled = false;
			if (!m_isCatchingUp && num > CatchUpRadius)
			{
				m_isCatchingUp = true;
				m_catchUpTime = 0f;
				if (!string.IsNullOrEmpty(CatchUpAnimation))
				{
					m_aiAnimator.PlayUntilFinished(CatchUpAnimation);
				}
				else
				{
					m_aiAnimator.EndAnimation();
				}
				m_aiActor.MovementModifiers += CatchUpMovementModifier;
			}
			else if (!m_isCatchingUp && num < CatchUpRadius)
			{
				m_aiAnimator.EndAnimation();
			}
			if (m_repathTimer <= 0f && (bool)primaryPlayer && (bool)primaryPlayer.specRigidbody && !primaryPlayer.IsInMinecart)
			{
				m_repathTimer = PathInterval;
				m_aiActor.PathfindToPosition(primaryPlayer.specRigidbody.UnitCenter);
				if (m_aiActor.Path != null && m_aiActor.Path.InaccurateLength > 50f)
				{
					m_aiActor.ClearPath();
					m_sequentialPathFails = 0;
					m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
				}
				else if (m_aiActor.Path != null && !m_aiActor.Path.WillReachFinalGoal)
				{
					m_sequentialPathFails++;
					IntVector2 key = m_aiActor.CompanionOwner.CenterPosition.ToIntVector2(VectorConversions.Floor);
					CellData cellData = GameManager.Instance.Dungeon.data[key];
					if (m_sequentialPathFails > 3 && cellData != null && cellData.IsPassable)
					{
						m_sequentialPathFails = 0;
						m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
					}
				}
				else
				{
					m_sequentialPathFails = 0;
				}
			}
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
