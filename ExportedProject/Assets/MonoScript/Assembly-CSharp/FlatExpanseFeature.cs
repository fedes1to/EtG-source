using Dungeonator;

public class FlatExpanseFeature : RobotRoomFeature
{
	public override bool CanContainOtherFeature()
	{
		return true;
	}

	public override int RequiredInsetForOtherFeature()
	{
		return 2;
	}

	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		return true;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		for (int i = LocalBasePosition.x; i < LocalBasePosition.x + LocalDimensions.x; i++)
		{
			for (int j = LocalBasePosition.y; j < LocalBasePosition.y + LocalDimensions.y; j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(i, j);
				prototypeDungeonRoomCellData.state = CellType.FLOOR;
			}
		}
	}
}
