public class RobotRoomRollingLogsVerticalFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (!idea.UseRollingLogsVertical)
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
		DungeonPlaceableBehaviour rollingLogVertical = RobotDave.GetRollingLogVertical();
		SerializedPath item = GenerateVerticalPath(LocalBasePosition, new IntVector2(LocalDimensions.x, LocalDimensions.y - (rollingLogVertical.GetHeight() - 1)));
		room.paths.Add(item);
		PrototypePlacedObjectData prototypePlacedObjectData = PlaceObject(rollingLogVertical, room, LocalBasePosition, targetObjectLayer);
		prototypePlacedObjectData.assignedPathIDx = room.paths.Count - 1;
		PrototypePlacedObjectFieldData prototypePlacedObjectFieldData = new PrototypePlacedObjectFieldData();
		prototypePlacedObjectFieldData.fieldName = "NumTiles";
		prototypePlacedObjectFieldData.fieldType = PrototypePlacedObjectFieldData.FieldType.FLOAT;
		prototypePlacedObjectFieldData.floatValue = LocalDimensions.x;
		prototypePlacedObjectData.fieldData.Add(prototypePlacedObjectFieldData);
	}
}
