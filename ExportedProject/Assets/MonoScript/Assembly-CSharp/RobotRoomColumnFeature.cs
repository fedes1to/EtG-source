using Dungeonator;
using UnityEngine;

public class RobotRoomColumnFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (dim.x < 8 || dim.y < 8)
		{
			return false;
		}
		return true;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		for (int i = LocalBasePosition.x; i < LocalBasePosition.x + LocalDimensions.x; i++)
		{
			for (int j = LocalBasePosition.y; j < LocalBasePosition.y + LocalDimensions.y; j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(i, j);
				prototypeDungeonRoomCellData.state = CellType.FLOOR;
			}
		}
		int num = Random.Range(3, LocalDimensions.x / 2 - 1);
		int num2 = Random.Range(3, LocalDimensions.y / 2 - 1);
		IntVector2 intVector = LocalBasePosition + new IntVector2(num, num2);
		IntVector2 dimensions = LocalDimensions - new IntVector2(2 * num, 2 * num2);
		for (int k = intVector.x; k < intVector.x + dimensions.x; k++)
		{
			for (int l = intVector.y; l < intVector.y + dimensions.y; l++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData2 = room.ForceGetCellDataAtPoint(k, l);
				prototypeDungeonRoomCellData2.state = CellType.WALL;
			}
		}
		if (idea.UseWallSawblades)
		{
			SerializedPath item = GenerateRectanglePathInset(intVector, dimensions);
			room.paths.Add(item);
			DungeonPlaceableBehaviour sawbladePrefab = RobotDave.GetSawbladePrefab();
			PrototypePlacedObjectData prototypePlacedObjectData = PlaceObject(sawbladePrefab, room, intVector, targetObjectLayer);
			prototypePlacedObjectData.assignedPathIDx = room.paths.Count - 1;
		}
	}
}
