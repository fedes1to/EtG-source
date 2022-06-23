using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DemonWall/MovementBehavior")]
public class DemonWallMovementBehavior : MovementBehaviorBase
{
	public float speed = 4f;

	public float sinPeriod = 2f;

	public float sinMagnitude = 1f;

	private DemonWallController m_demonWallController;

	private bool m_initialized;

	private float m_startY;

	private float m_startCameraY;

	private float lowestGoalY = float.MaxValue;

	private float m_timer;

	public override void Start()
	{
		base.Start();
		m_demonWallController = m_aiActor.GetComponent<DemonWallController>();
		m_updateEveryFrame = true;
	}

	public override BehaviorResult Update()
	{
		if (m_deltaTime > 0f && m_demonWallController.IsCameraLocked)
		{
			if (!m_initialized)
			{
				m_startY = m_aiActor.specRigidbody.Position.UnitY;
				m_startCameraY = m_demonWallController.CameraPos.y;
				m_initialized = true;
			}
			m_timer += m_deltaTime;
			float num = m_startY - m_timer * speed;
			float y = m_startCameraY - m_timer * speed;
			num += Mathf.Sin(m_timer / sinPeriod * (float)Math.PI) * sinMagnitude;
			num = (lowestGoalY = Mathf.Min(lowestGoalY, num));
			m_aiActor.BehaviorOverridesVelocity = true;
			if (m_deltaTime > 0f)
			{
				m_aiActor.BehaviorVelocity = new Vector2(0f, (num - m_aiActor.specRigidbody.Position.UnitY) / m_deltaTime);
			}
			m_aiActor.specRigidbody.Velocity = m_aiActor.BehaviorVelocity;
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			Vector2 vector = m_demonWallController.CameraPos.WithY(y);
			float b = (float)m_aiActor.ParentRoom.area.basePosition.y + mainCameraController.Camera.orthographicSize;
			vector.y = Mathf.Max(vector.y, b);
			mainCameraController.OverridePosition = vector;
		}
		return BehaviorResult.Continue;
	}
}
