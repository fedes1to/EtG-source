using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BulletBro/SeekTargetBehavior")]
public class BulletBroSeekTargetBehavior : MovementBehaviorBase
{
	public bool StopWhenInRange = true;

	public float CustomRange = -1f;

	public float PathInterval = 0.25f;

	private float m_repathTimer;

	private AIActor m_otherBro;

	public override float DesiredCombatDistance
	{
		get
		{
			return CustomRange;
		}
	}

	public override void Start()
	{
		base.Start();
		BroController otherBro = BroController.GetOtherBro(m_aiActor.gameObject);
		if ((bool)otherBro)
		{
			m_otherBro = otherBro.aiActor;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	public override BehaviorResult Update()
	{
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		if (targetRigidbody != null)
		{
			float desiredCombatDistance = m_aiActor.DesiredCombatDistance;
			if (StopWhenInRange && m_aiActor.DistanceToTarget <= desiredCombatDistance)
			{
				m_aiActor.ClearPath();
				return BehaviorResult.Continue;
			}
			if (m_repathTimer <= 0f)
			{
				Vector2 targetPosition;
				if (!m_otherBro)
				{
					targetPosition = targetRigidbody.UnitCenter;
				}
				else
				{
					Vector2 unitCenter = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
					Vector2 unitCenter2 = m_aiActor.specRigidbody.UnitCenter;
					Vector2 unitCenter3 = m_otherBro.specRigidbody.UnitCenter;
					float num = (unitCenter2 - unitCenter).ToAngle();
					float num2 = (unitCenter3 - unitCenter).ToAngle();
					float num3 = (num + num2) / 2f;
					float angle = ((!(BraveMathCollege.ClampAngle180(num - num3) > 0f)) ? (num3 - 90f) : (num3 + 90f));
					targetPosition = unitCenter + BraveMathCollege.DegreesToVector(angle) * DesiredCombatDistance;
				}
				m_aiActor.PathfindToPosition(targetPosition);
				m_repathTimer = PathInterval;
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		return BehaviorResult.Continue;
	}
}
