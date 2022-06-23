using Dungeonator;
using UnityEngine;

public class RobotRoomTablesFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (dim.x < 7 && dim.y < 7)
		{
			return false;
		}
		return true;
	}

	public override bool CanContainOtherFeature()
	{
		return true;
	}

	public override int RequiredInsetForOtherFeature()
	{
		return 5;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		bool flag = LocalDimensions.x < LocalDimensions.y;
		if (Mathf.Abs(1f - (float)LocalDimensions.x / ((float)LocalDimensions.y * 1f)) < 0.25f)
		{
			flag = Random.value < 0.5f;
		}
		if (flag)
		{
			DungeonPlaceable horizontalTable = RobotDave.GetHorizontalTable();
			for (int i = LocalBasePosition.x + 3; i < LocalBasePosition.x + LocalDimensions.x - 3; i += 4)
			{
				IntVector2 position = new IntVector2(i, LocalBasePosition.y + 3);
				IntVector2 position2 = new IntVector2(i, LocalBasePosition.y + LocalDimensions.y - 4);
				PlaceObject(horizontalTable, room, position, targetObjectLayer);
				PlaceObject(horizontalTable, room, position2, targetObjectLayer);
			}
		}
		else
		{
			DungeonPlaceable verticalTable = RobotDave.GetVerticalTable();
			for (int j = LocalBasePosition.y + 3; j < LocalBasePosition.y + LocalDimensions.y - 3; j += 4)
			{
				IntVector2 position3 = new IntVector2(LocalBasePosition.x + 3, j);
				IntVector2 position4 = new IntVector2(LocalBasePosition.x + LocalDimensions.x - 4, j);
				PlaceObject(verticalTable, room, position3, targetObjectLayer);
				PlaceObject(verticalTable, room, position4, targetObjectLayer);
			}
		}
	}
}
