using Dungeonator;
using UnityEngine;

public class RobotRoomSurroundingPitFeature : RobotRoomFeature
{
	public static bool BeenUsed;

	public override void Use()
	{
		BeenUsed = true;
		base.Use();
	}

	public override bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures)
	{
		if (Application.isPlaying)
		{
			return false;
		}
		if (BeenUsed)
		{
			return false;
		}
		if (isInternal)
		{
			return false;
		}
		return idea.CanIncludePits;
	}

	public override bool CanContainOtherFeature()
	{
		return true;
	}

	public override int RequiredInsetForOtherFeature()
	{
		return 2;
	}

	public override void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer)
	{
		for (int i = LocalBasePosition.x; i < LocalBasePosition.x + LocalDimensions.x; i++)
		{
			for (int j = LocalBasePosition.y; j < LocalBasePosition.y + LocalDimensions.y; j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(i, j);
				prototypeDungeonRoomCellData.state = CellType.PIT;
			}
		}
		room.RedefineAllPitEntries();
		int num = RequiredInsetForOtherFeature();
		for (int k = LocalBasePosition.x + num; k < LocalBasePosition.x + LocalDimensions.x - num; k++)
		{
			for (int l = LocalBasePosition.y + num; l < LocalBasePosition.y + LocalDimensions.y - num; l++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData2 = room.ForceGetCellDataAtPoint(k, l);
				prototypeDungeonRoomCellData2.state = CellType.FLOOR;
			}
		}
	}
}
