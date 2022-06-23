using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class FlameTrapChallengeModifier : ChallengeModifier
{
	public DungeonPlaceable FlameTrap;

	public float ChanceToTrap = 0.2f;

	public float TrapChanceDecrementPerFloor = 0.005f;

	private static List<BasicTrapController> m_activeTraps = new List<BasicTrapController>();

	private void Start()
	{
		RoomHandler currentRoom = GameManager.Instance.BestActivePlayer.CurrentRoom;
		float num = 0f;
		switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
		{
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			num = TrapChanceDecrementPerFloor;
			break;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			num = TrapChanceDecrementPerFloor * 2f;
			break;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			num = TrapChanceDecrementPerFloor * 3f;
			break;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			num = TrapChanceDecrementPerFloor * 4f;
			break;
		case GlobalDungeonData.ValidTilesets.HELLGEON:
			num = TrapChanceDecrementPerFloor * 4f;
			break;
		}
		Vector2 centerPosition = GameManager.Instance.BestActivePlayer.CenterPosition;
		for (int i = 0; i < currentRoom.area.dimensions.x; i++)
		{
			for (int j = 0; j < currentRoom.area.dimensions.y; j++)
			{
				IntVector2 intVector = currentRoom.area.basePosition + new IntVector2(i, j);
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
				{
					CellData cellData = GameManager.Instance.Dungeon.data[intVector];
					if (!(Vector2.Distance(cellData.position.ToCenterVector2(), centerPosition) < 5f) && cellData.parentRoom == currentRoom && cellData.type == CellType.FLOOR && !cellData.containsTrap && !cellData.isOccupied && !cellData.isOccludedByTopWall && !cellData.PreventRewardSpawn && Random.value < ChanceToTrap - num)
					{
						GameObject gameObject = FlameTrap.InstantiateObject(currentRoom, cellData.position - currentRoom.area.basePosition);
						m_activeTraps.Add(gameObject.GetComponent<BasicTrapController>());
						Exploder.DoRadialMinorBreakableBreak(cellData.position.ToCenterVector3(cellData.position.y), 1f);
						cellData.containsTrap = true;
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < m_activeTraps.Count; i++)
		{
			if ((bool)m_activeTraps[i])
			{
				m_activeTraps[i].triggerMethod = BasicTrapController.TriggerMethod.Script;
			}
		}
		m_activeTraps.Clear();
	}
}
