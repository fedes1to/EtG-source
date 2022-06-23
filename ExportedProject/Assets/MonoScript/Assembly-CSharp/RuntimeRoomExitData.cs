using System;

public class RuntimeRoomExitData
{
	[NonSerialized]
	public PrototypeRoomExit referencedExit;

	[NonSerialized]
	public int additionalExitLength;

	[NonSerialized]
	public bool jointedExit;

	[NonSerialized]
	public RuntimeRoomExitData linkedExit;

	[NonSerialized]
	public bool oneWayDoor;

	[NonSerialized]
	public bool isLockedDoor;

	[NonSerialized]
	public bool isCriticalPath;

	[NonSerialized]
	public bool isWarpWingStart;

	[NonSerialized]
	public DungeonFlowNode.ForcedDoorType forcedDoorType;

	[NonSerialized]
	public WarpWingPortalController warpWingPortal;

	public int TotalExitLength
	{
		get
		{
			return additionalExitLength + referencedExit.exitLength;
		}
	}

	public IntVector2 HalfExitAttachPoint
	{
		get
		{
			return referencedExit.GetHalfExitAttachPoint(TotalExitLength);
		}
	}

	public IntVector2 ExitOrigin
	{
		get
		{
			return referencedExit.GetExitOrigin(TotalExitLength);
		}
	}

	public RuntimeRoomExitData(PrototypeRoomExit exit)
	{
		referencedExit = exit;
	}
}
