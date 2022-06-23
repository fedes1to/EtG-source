public class RobotRoomRollingLogsHorizontalFeature : RobotRoomFeature
{
	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (!idea.UseRollingLogsHorizontal)
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
		DungeonPlaceableBehaviour rollingLogHorizontal = RobotDave.GetRollingLogHorizontal();
		SerializedPath item = GenerateHorizontalPath(LocalBasePosition, new IntVector2(LocalDimensions.x - (rollingLogHorizontal.GetWidth() - 1), LocalDimensions.y));
		room.paths.Add(item);
		PrototypePlacedObjectData prototypePlacedObjectData = PlaceObject(rollingLogHorizontal, room, LocalBasePosition, targetObjectLayer);
		prototypePlacedObjectData.assignedPathIDx = room.paths.Count - 1;
		PrototypePlacedObjectFieldData prototypePlacedObjectFieldData = new PrototypePlacedObjectFieldData();
		prototypePlacedObjectFieldData.fieldName = "NumTiles";
		prototypePlacedObjectFieldData.fieldType = PrototypePlacedObjectFieldData.FieldType.FLOAT;
		prototypePlacedObjectFieldData.floatValue = LocalDimensions.y;
		prototypePlacedObjectData.fieldData.Add(prototypePlacedObjectFieldData);
	}
}
