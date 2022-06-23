using UnityEngine;

public class CircleRoomBehavior : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	public float Radius = 3f;

	public float Direction = 1f;

	private float m_repathTimer;

	private Vector2 m_center;

	public override void Start()
	{
		base.Start();
		m_center = m_aiActor.ParentRoom.area.UnitCenter;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	public override BehaviorResult Update()
	{
		if (m_repathTimer <= 0f)
		{
			float num = (m_aiActor.specRigidbody.UnitCenter - m_center).ToAngle();
			float num2 = PathInterval * 2f * m_aiActor.MovementSpeed;
			float angle = num + Direction * (num2 / Radius) * 57.29578f;
			Vector2 vector = m_center + BraveMathCollege.DegreesToVector(angle, Radius);
			m_aiActor.PathfindToPosition(vector, vector);
			m_repathTimer = PathInterval;
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
