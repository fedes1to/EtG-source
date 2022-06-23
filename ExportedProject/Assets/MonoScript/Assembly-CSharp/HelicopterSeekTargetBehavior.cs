using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Helicopter/SeekTargetBehavior")]
public class HelicopterSeekTargetBehavior : MovementBehaviorBase
{
	public Vector2 minPoint;

	public Vector2 maxPoint;

	public Vector2 period;

	public float MaxSpeed = 6f;

	private float m_timer;

	private bool m_isMoving;

	public override void Start()
	{
		base.Start();
		m_updateEveryFrame = true;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_aiActor.OverridePathVelocity = null;
	}

	public override BehaviorResult Update()
	{
		m_timer += m_deltaTime;
		Vector2 unitBottomLeft = m_aiActor.ParentRoom.area.UnitBottomLeft;
		float num = 0f;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if (playerController.healthHaver.IsAlive)
			{
				num = Mathf.Max(num, playerController.specRigidbody.UnitCenter.y);
			}
		}
		num = Mathf.Max(0f, num - unitBottomLeft.y);
		Vector2 vector = unitBottomLeft + new Vector2(Mathf.SmoothStep(minPoint.x, maxPoint.x, Mathf.PingPong(m_timer, period.x) / period.x), Mathf.SmoothStep(minPoint.y, maxPoint.y, Mathf.PingPong(m_timer, period.y) / period.y) + num);
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 vector2 = vector - unitCenter;
		Vector2 value;
		if (m_deltaTime > 0f && vector2.magnitude > 0f)
		{
			value = vector2 / BraveTime.DeltaTime;
			if (MaxSpeed >= 0f && value.magnitude > MaxSpeed)
			{
				value = MaxSpeed * value.normalized;
			}
		}
		else
		{
			value = Vector2.zero;
		}
		m_isMoving = true;
		m_aiActor.OverridePathVelocity = value;
		return BehaviorResult.Continue;
	}
}
