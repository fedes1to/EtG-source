using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PrototypeRoomObjectLayer
{
	public List<PrototypePlacedObjectData> placedObjects = new List<PrototypePlacedObjectData>();

	public List<Vector2> placedObjectBasePositions = new List<Vector2>();

	public bool layerIsReinforcementLayer;

	public bool shuffle = true;

	public int randomize = 2;

	public bool suppressPlayerChecks;

	public float delayTime = 15f;

	public RoomEventTriggerCondition reinforcementTriggerCondition = RoomEventTriggerCondition.ON_ENEMIES_CLEARED;

	public float probability = 1f;

	public int numberTimesEncounteredRequired;

	public static PrototypeRoomObjectLayer CreateMirror(PrototypeRoomObjectLayer source, IntVector2 roomDimensions)
	{
		PrototypeRoomObjectLayer prototypeRoomObjectLayer = new PrototypeRoomObjectLayer();
		prototypeRoomObjectLayer.placedObjects = new List<PrototypePlacedObjectData>();
		for (int i = 0; i < source.placedObjects.Count; i++)
		{
			prototypeRoomObjectLayer.placedObjects.Add(source.placedObjects[i].CreateMirror(roomDimensions));
		}
		prototypeRoomObjectLayer.placedObjectBasePositions = new List<Vector2>();
		for (int j = 0; j < source.placedObjectBasePositions.Count; j++)
		{
			Vector2 item = source.placedObjectBasePositions[j];
			item.x = (float)roomDimensions.x - (item.x + (float)prototypeRoomObjectLayer.placedObjects[j].GetWidth(true));
			prototypeRoomObjectLayer.placedObjectBasePositions.Add(item);
		}
		prototypeRoomObjectLayer.layerIsReinforcementLayer = source.layerIsReinforcementLayer;
		prototypeRoomObjectLayer.shuffle = source.shuffle;
		prototypeRoomObjectLayer.randomize = source.randomize;
		prototypeRoomObjectLayer.suppressPlayerChecks = source.suppressPlayerChecks;
		prototypeRoomObjectLayer.delayTime = source.delayTime;
		prototypeRoomObjectLayer.reinforcementTriggerCondition = source.reinforcementTriggerCondition;
		prototypeRoomObjectLayer.probability = source.probability;
		prototypeRoomObjectLayer.numberTimesEncounteredRequired = source.numberTimesEncounteredRequired;
		return prototypeRoomObjectLayer;
	}
}
