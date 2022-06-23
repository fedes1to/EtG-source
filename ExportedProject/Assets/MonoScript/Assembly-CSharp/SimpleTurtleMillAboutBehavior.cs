using Dungeonator;
using UnityEngine;

public class SimpleTurtleMillAboutBehavior : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	public float TargetInterval = 3f;

	public float MillRadius = 5f;

	private Vector2 m_currentTargetPosition;

	private float m_repathTimer;

	private float m_newPositionTimer;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		DecrementTimer(ref m_newPositionTimer);
	}

	private Vector2 GetNewTargetPosition(PlayerController owner)
	{
		Vector2? vector = null;
		int num = 30;
		while (!vector.HasValue && num > 0)
		{
			num--;
			Vector2 vector2 = Random.insideUnitCircle.normalized * Random.Range(0.5f, 1f);
			vector = owner.specRigidbody.HitboxPixelCollider.UnitCenter + vector2 * MillRadius;
			vector = ((!vector.HasValue) ? null : new Vector2?(vector.GetValueOrDefault() + owner.specRigidbody.Velocity.normalized * Random.Range(0f, MillRadius * 1.5f)));
			CellData cell = vector.Value.GetCell();
			if (cell == null || cell.type != CellType.FLOOR || !cell.IsPassable)
			{
				vector = null;
			}
		}
		if (!vector.HasValue)
		{
			return owner.specRigidbody.HitboxPixelCollider.UnitBottomCenter;
		}
		return vector.Value;
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
		PlayerController playerController = GameManager.Instance.PrimaryPlayer;
		if ((bool)m_aiActor && (bool)m_aiActor.CompanionOwner)
		{
			playerController = m_aiActor.CompanionOwner;
		}
		m_aiActor.MovementSpeed = m_aiActor.BaseMovementSpeed;
		if (!playerController || !playerController.IsInCombat)
		{
			return BehaviorResult.Continue;
		}
		float num = Vector2.Distance(playerController.CenterPosition, m_currentTargetPosition);
		float num2 = Vector2.Distance(m_aiActor.CenterPosition, m_currentTargetPosition);
		if (m_newPositionTimer <= 0f || num > MillRadius * 1.75f || num2 <= 0.25f)
		{
			m_aiActor.ClearPath();
			m_currentTargetPosition = GetNewTargetPosition(playerController);
			m_newPositionTimer = TargetInterval;
		}
		else if (num2 > 30f)
		{
			m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
		}
		m_aiActor.MovementSpeed = Mathf.Lerp(m_aiActor.BaseMovementSpeed, m_aiActor.BaseMovementSpeed * 2f, Mathf.Clamp01(num2 / 30f));
		if (m_repathTimer <= 0f && !playerController.IsOverPitAtAll && !playerController.IsInMinecart)
		{
			m_repathTimer = PathInterval;
			m_aiActor.FallingProhibited = false;
			m_aiActor.PathfindToPosition(m_currentTargetPosition);
			if (m_aiActor.Path != null && m_aiActor.Path.InaccurateLength > 50f)
			{
				m_aiActor.ClearPath();
				m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
			}
			else if (m_aiActor.Path != null && !m_aiActor.Path.WillReachFinalGoal)
			{
				m_aiActor.CompanionWarp(m_aiActor.CompanionOwner.CenterPosition);
			}
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
