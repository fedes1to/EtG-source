using System.Collections.Generic;

namespace Dungeonator
{
	public class FlowNodeBuildData
	{
		public DungeonFlowNode node;

		public bool usesOverrideCategory;

		public PrototypeDungeonRoom.RoomCategory overrideCategory;

		public RoomHandler room;

		public bool unbuilt = true;

		public PrototypeRoomExit sourceExit;

		public PrototypeRoomExit roomEntrance;

		public RoomHandler sourceRoom;

		public List<FlowNodeBuildData> childBuildData;

		public FlowNodeBuildData(DungeonFlowNode n)
		{
			node = n;
		}

		public void MarkExits()
		{
			room.area.instanceUsedExits.Add(roomEntrance);
			sourceRoom.area.instanceUsedExits.Add(sourceExit);
			room.parentRoom = sourceRoom;
			room.connectedRooms.Add(sourceRoom);
			room.connectedRoomsByExit.Add(roomEntrance, sourceRoom);
			sourceRoom.childRooms.Add(room);
			sourceRoom.connectedRooms.Add(room);
			sourceRoom.connectedRoomsByExit.Add(sourceExit, room);
		}

		public void UnmarkExits()
		{
			room.area.instanceUsedExits.Remove(roomEntrance);
			sourceRoom.area.instanceUsedExits.Remove(sourceExit);
			room.parentRoom = null;
			room.connectedRooms.Remove(sourceRoom);
			room.connectedRoomsByExit.Remove(roomEntrance);
			sourceRoom.childRooms.Remove(room);
			sourceRoom.connectedRooms.Remove(room);
			sourceRoom.connectedRoomsByExit.Remove(sourceExit);
		}
	}
}
