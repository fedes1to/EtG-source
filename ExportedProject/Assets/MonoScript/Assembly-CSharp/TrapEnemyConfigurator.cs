using Dungeonator;

public class TrapEnemyConfigurator : BraveBehaviour
{
	private bool m_isActive;

	private RoomHandler m_parentRoom;

	private void Start()
	{
		m_parentRoom = base.transform.position.GetAbsoluteRoom();
		m_parentRoom.Entered += Activate;
	}

	private void Activate(PlayerController p)
	{
		if (!m_isActive)
		{
			m_isActive = true;
			base.behaviorSpeculator.enabled = true;
		}
	}

	private void Update()
	{
		if (m_isActive && !GameManager.Instance.IsAnyPlayerInRoom(m_parentRoom))
		{
			m_isActive = false;
			base.behaviorSpeculator.enabled = false;
		}
	}
}
