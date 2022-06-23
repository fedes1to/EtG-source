using Dungeonator;
using UnityEngine;

public class RobotRoomInnerPitFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (dim.x <= 7 || dim.y <= 7)
		{
			return false;
		}
		return idea.CanIncludePits;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		int num = Random.Range(4, LocalDimensions.x / 2);
		int num2 = Random.Range(4, LocalDimensions.y / 2);
		int num3 = (LocalDimensions.x - num) / 2 + LocalBasePosition.x;
		int num4 = (LocalDimensions.y - num2) / 2 + LocalBasePosition.y;
		for (int i = num3; i < num3 + num; i++)
		{
			for (int j = num4; j < num4 + num2; j++)
			{
				room.ForceGetCellDataAtPoint(i, j).state = CellType.PIT;
			}
		}
		room.RedefineAllPitEntries();
	}
}
