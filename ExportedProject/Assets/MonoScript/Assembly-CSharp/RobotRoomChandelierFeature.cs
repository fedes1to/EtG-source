using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RobotRoomChandelierFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (!idea.UseChandeliers)
		{
			return false;
		}
		if (dim.x < 5 || dim.y < 5)
		{
			return false;
		}
		if (isInternal)
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
		DungeonPlaceableBehaviour chandelierPrefab = RobotDave.GetChandelierPrefab();
		PrototypePlacedObjectData prototypePlacedObjectData = PlaceObject(chandelierPrefab, room, position, targetObjectLayer);
		IntVector2 intVector = new IntVector2(Random.Range(0, LocalDimensions.x), LocalDimensions.y - 1);
		IntVector2 item = LocalBasePosition + intVector;
		List<IntVector2> list = new List<IntVector2>();
		list.Add(item);
		PrototypeEventTriggerArea item2 = room.AddEventTriggerArea(list);
		int item3 = room.eventTriggerAreas.IndexOf(item2);
		List<int> list2 = new List<int>();
		list2.Add(item3);
		prototypePlacedObjectData.linkedTriggerAreaIDs = list2;
	}
}
