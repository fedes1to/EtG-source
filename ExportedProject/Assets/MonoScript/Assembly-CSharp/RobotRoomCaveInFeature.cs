using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RobotRoomCaveInFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (!idea.UseCaveIns)
		{
			return false;
		}
		if (dim.x < 5 || dim.y < 5)
		{
			return false;
		}
		return true;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		int x = LocalDimensions.x / 2 - 1;
		int y = LocalDimensions.y / 2 - 1;
		IntVector2 position = LocalBasePosition + new IntVector2(x, y);
		DungeonPlaceableBehaviour caveInPrefab = RobotDave.GetCaveInPrefab();
		PrototypePlacedObjectData prototypePlacedObjectData = PlaceObject(caveInPrefab, room, position, targetObjectLayer);
		IntVector2[] array = new IntVector2[8]
		{
			new IntVector2(1, 1),
			new IntVector2(x, 1),
			new IntVector2(LocalDimensions.x - 2, 1),
			new IntVector2(1, y),
			new IntVector2(LocalDimensions.x - 2, y),
			new IntVector2(1, LocalDimensions.y - 2),
			new IntVector2(x, LocalDimensions.y - 2),
			new IntVector2(LocalDimensions.x - 2, LocalDimensions.y - 2)
		};
		IntVector2 item = LocalBasePosition + array[Random.Range(0, 8)];
		List<IntVector2> list = new List<IntVector2>();
		list.Add(item);
		PrototypeEventTriggerArea item2 = room.AddEventTriggerArea(list);
		int item3 = room.eventTriggerAreas.IndexOf(item2);
		List<int> list2 = new List<int>();
		list2.Add(item3);
		prototypePlacedObjectData.linkedTriggerAreaIDs = list2;
	}
}
