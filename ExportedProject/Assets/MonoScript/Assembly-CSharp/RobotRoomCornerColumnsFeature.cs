using Dungeonator;
using UnityEngine;

public class RobotRoomCornerColumnsFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (Application.isPlaying)
		{
			return false;
		}
		if (numFeatures >= 4)
		{
			return false;
		}
		if (dim.x < 8 || dim.y < 8)
		{
			return false;
		}
		return true;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		int num = 2;
		IntVector2 localBasePosition = LocalBasePosition;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				room.ForceGetCellDataAtPoint(localBasePosition.x + i, localBasePosition.y + j).state = CellType.WALL;
			}
		}
		localBasePosition = LocalBasePosition + new IntVector2(LocalDimensions.x - num, 0);
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				room.ForceGetCellDataAtPoint(localBasePosition.x + k, localBasePosition.y + l).state = CellType.WALL;
			}
		}
		localBasePosition = LocalBasePosition + new IntVector2(LocalDimensions.x - num, LocalDimensions.y - num);
		for (int m = 0; m < num; m++)
		{
			for (int n = 0; n < num; n++)
			{
				room.ForceGetCellDataAtPoint(localBasePosition.x + m, localBasePosition.y + n).state = CellType.WALL;
			}
		}
		localBasePosition = LocalBasePosition + new IntVector2(0, LocalDimensions.y - num);
		for (int num2 = 0; num2 < num; num2++)
		{
			for (int num3 = 0; num3 < num; num3++)
			{
				room.ForceGetCellDataAtPoint(localBasePosition.x + num2, localBasePosition.y + num3).state = CellType.WALL;
			}
		}
	}
}
