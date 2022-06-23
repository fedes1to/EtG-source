using System.Collections.Generic;
using UnityEngine;

public class RobotRoomTrapSquareFeature : RobotRoomFeature
{
	public override bool CanContainOtherFeature()
	{
		return true;
	}

	public override int RequiredInsetForOtherFeature()
	{
		return 4;
	}

	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (!idea.UseFloorFlameTraps && !idea.UseFloorPitTraps && !idea.UseFloorSpikeTraps)
		{
			return false;
		}
		if (dim.x <= 6 || dim.y <= 6)
		{
			return false;
		}
		return true;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		List<int> list = new List<int>();
		if (idea.UseFloorPitTraps)
		{
			list.Add(0);
		}
		if (idea.UseFloorSpikeTraps)
		{
			list.Add(1);
		}
		if (idea.UseFloorFlameTraps)
		{
			list.Add(2);
		}
		int num = list[Random.Range(0, list.Count)];
		DungeonPlaceableBehaviour dungeonPlaceableBehaviour = null;
		switch (num)
		{
		case 0:
			dungeonPlaceableBehaviour = RobotDave.GetPitTrap();
			break;
		case 1:
			dungeonPlaceableBehaviour = RobotDave.GetSpikesTrap();
			break;
		case 2:
			dungeonPlaceableBehaviour = RobotDave.GetFloorFlameTrap();
			break;
		}
		int width = dungeonPlaceableBehaviour.GetWidth();
		int num2 = LocalBasePosition.x + 2;
		int num3 = LocalBasePosition.y + 2;
		int num4 = LocalBasePosition.x + LocalDimensions.x - 2;
		int num5 = LocalBasePosition.y + LocalDimensions.y - 2;
		if ((num4 - num2) % width != 0)
		{
			num4--;
		}
		if ((num5 - num3) % width != 0)
		{
			num5--;
		}
		for (int i = num2; i < num4; i += width)
		{
			for (int j = num3; j < num5; j += width)
			{
				if (i == num2 || i == num4 - width || j == num3 || j == num5 - width)
				{
					IntVector2 position = new IntVector2(i, j);
					PlaceObject(dungeonPlaceableBehaviour, room, position, targetObjectLayer);
				}
			}
		}
	}
}
