using Dungeonator;
using UnityEngine;

public class GoopPlaceable : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public GoopDefinition goop;

	[DwarfConfigurable]
	public float radius = 1f;

	private RoomHandler m_room;

	protected override void OnDestroy()
	{
		if (m_room != null)
		{
			m_room.Entered -= PlayerEntered;
		}
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		m_room.Entered += PlayerEntered;
	}

	public void PlayerEntered(PlayerController playerController)
	{
		DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goop);
		goopManagerForGoopType.AddGoopCircle(base.transform.position.XY() + new Vector2(0.5f, 0.5f), radius);
		if (m_room != null)
		{
			m_room.Entered -= PlayerEntered;
		}
	}
}
