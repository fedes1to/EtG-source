using Dungeonator;
using Pathfinding;
using UnityEngine;

public class CursePotChallengeModifier : ChallengeModifier
{
	public DungeonPlaceable CursePot;

	public int RoughAreaPerCursePot = 50;

	private void Start()
	{
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		int b = currentRoom.CellsWithoutExits.Count / RoughAreaPerCursePot;
		b = Mathf.Max(1, b);
		CellValidator cellValidator = delegate(IntVector2 pos)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				if (Vector2.Distance(GameManager.Instance.AllPlayers[j].CenterPosition, pos.ToCenterVector2()) < 8f)
				{
					return false;
				}
			}
			return true;
		};
		for (int i = 0; i < b; i++)
		{
			IntVector2? randomAvailableCell = currentRoom.GetRandomAvailableCell(IntVector2.One, CellTypes.FLOOR, false, cellValidator);
			if (randomAvailableCell.HasValue)
			{
				CellData cellData = GameManager.Instance.Dungeon.data[randomAvailableCell.Value];
				if (cellData.parentRoom == currentRoom && cellData.type == CellType.FLOOR && !cellData.isOccupied && !cellData.containsTrap && !cellData.isOccludedByTopWall)
				{
					cellData.containsTrap = true;
					CursePot.InstantiateObject(currentRoom, cellData.position - currentRoom.area.basePosition);
				}
			}
		}
	}
}
