using System;
using System.Collections;
using Dungeonator;

public class ArtfulDodgerCameraManipulator : DungeonPlaceableBehaviour, IEventTriggerable, IPlaceConfigurable
{
	[DwarfConfigurable]
	public float OverrideZoomScale = 0.75f;

	[NonSerialized]
	public bool Active;

	private bool m_triggered;

	private bool m_triggeredFrame;

	private ArtfulDodgerRoomController m_dodgerRoom;

	protected RoomHandler m_room;

	private IEnumerator Start()
	{
		yield return null;
		m_dodgerRoom = m_room.GetComponentsAbsoluteInRoom<ArtfulDodgerRoomController>()[0];
		m_dodgerRoom.RegisterCameraZone(this);
	}

	public void Trigger(int index)
	{
		if (!m_dodgerRoom.Completed && Active)
		{
			m_triggeredFrame = true;
		}
	}

	public void LateUpdate()
	{
		if (!m_triggeredFrame)
		{
			if (m_triggered)
			{
				m_triggered = false;
				Minimap.Instance.TemporarilyPreventMinimap = false;
				GameManager.Instance.MainCameraController.SetManualControl(false);
				GameManager.Instance.MainCameraController.OverrideZoomScale = 1f;
			}
		}
		else if (!m_triggered)
		{
			m_triggered = true;
			Minimap.Instance.TemporarilyPreventMinimap = true;
			GameManager.Instance.MainCameraController.OverridePosition = base.transform.position.XY();
			GameManager.Instance.MainCameraController.SetManualControl(true);
			GameManager.Instance.MainCameraController.OverrideZoomScale = OverrideZoomScale;
		}
		m_triggeredFrame = false;
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
