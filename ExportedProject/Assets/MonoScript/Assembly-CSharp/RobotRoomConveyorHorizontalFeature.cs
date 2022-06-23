using UnityEngine;

public class RobotRoomConveyorHorizontalFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (dim.x < 3 || dim.y < 3)
		{
			return false;
		}
		if (!idea.UseFloorConveyorBelts)
		{
			return false;
		}
		return true;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		DungeonPlaceableBehaviour horizontalConveyorPrefab = RobotDave.GetHorizontalConveyorPrefab();
		PrototypePlacedObjectData prototypePlacedObjectData = PlaceObject(horizontalConveyorPrefab, room, LocalBasePosition, targetObjectLayer);
		PrototypePlacedObjectFieldData prototypePlacedObjectFieldData = new PrototypePlacedObjectFieldData();
		prototypePlacedObjectFieldData.fieldName = "ConveyorWidth";
		prototypePlacedObjectFieldData.fieldType = PrototypePlacedObjectFieldData.FieldType.FLOAT;
		prototypePlacedObjectFieldData.floatValue = LocalDimensions.x;
		PrototypePlacedObjectFieldData prototypePlacedObjectFieldData2 = new PrototypePlacedObjectFieldData();
		prototypePlacedObjectFieldData2.fieldName = "ConveyorHeight";
		prototypePlacedObjectFieldData2.fieldType = PrototypePlacedObjectFieldData.FieldType.FLOAT;
		prototypePlacedObjectFieldData2.floatValue = LocalDimensions.y;
		PrototypePlacedObjectFieldData prototypePlacedObjectFieldData3 = new PrototypePlacedObjectFieldData();
		prototypePlacedObjectFieldData3.fieldName = "VelocityX";
		prototypePlacedObjectFieldData3.fieldType = PrototypePlacedObjectFieldData.FieldType.FLOAT;
		prototypePlacedObjectFieldData3.floatValue = ((!(Random.value > 0.5f)) ? (-4) : 4);
		PrototypePlacedObjectFieldData prototypePlacedObjectFieldData4 = new PrototypePlacedObjectFieldData();
		prototypePlacedObjectFieldData4.fieldName = "VelocityY";
		prototypePlacedObjectFieldData4.fieldType = PrototypePlacedObjectFieldData.FieldType.FLOAT;
		prototypePlacedObjectFieldData4.floatValue = 0f;
		prototypePlacedObjectData.fieldData.Add(prototypePlacedObjectFieldData);
		prototypePlacedObjectData.fieldData.Add(prototypePlacedObjectFieldData2);
		prototypePlacedObjectData.fieldData.Add(prototypePlacedObjectFieldData3);
		prototypePlacedObjectData.fieldData.Add(prototypePlacedObjectFieldData4);
	}
}
