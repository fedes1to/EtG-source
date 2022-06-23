using System.Collections.Generic;
using UnityEngine;

public class RobotRoomTrapPlusFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (!idea.UseFloorFlameTraps && !idea.UseFloorPitTraps && !idea.UseFloorSpikeTraps)
		{
			return false;
		}
		if (numFeatures != 1)
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
		if (LocalDimensions.x % width == 0)
		{
			int y = LocalBasePosition.y + Mathf.FloorToInt((float)LocalDimensions.y / 2f) - (width - 1);
			for (int i = 0; i < LocalDimensions.x; i += width)
			{
				PlaceObject(dungeonPlaceableBehaviour, room, new IntVector2(LocalBasePosition.x + i, y), targetObjectLayer);
			}
		}
		if (LocalDimensions.y % width == 0)
		{
			int x = LocalBasePosition.x + Mathf.FloorToInt((float)LocalDimensions.x / 2f) - (width - 1);
			for (int j = 0; j < LocalDimensions.y; j += width)
			{
				PlaceObject(dungeonPlaceableBehaviour, room, new IntVector2(x, LocalBasePosition.y + j), targetObjectLayer);
			}
		}
	}
}
