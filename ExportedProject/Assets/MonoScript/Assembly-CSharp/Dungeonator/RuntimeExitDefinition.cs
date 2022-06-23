using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	public class RuntimeExitDefinition
	{
		public RuntimeRoomExitData upstreamExit;

		public RuntimeRoomExitData downstreamExit;

		private const int SUBDOOR_CORRIDOR_THRESHOLD = 7;

		public RoomHandler upstreamRoom;

		public RoomHandler downstreamRoom;

		public DungeonDoorController linkedDoor;

		public bool containsLight;

		public bool isCriticalPath;

		public HashSet<IntVector2> ExitOccluderCells;

		public HashSet<IntVector2> IntermediaryCells;

		protected HashSet<IntVector2> m_upstreamCells;

		protected HashSet<IntVector2> m_downstreamCells;

		private bool m_westgeonProcessed;

		public RoomHandler.VisibilityStatus Visibility
		{
			get
			{
				if (IsVisibleFromRoom(GameManager.Instance.PrimaryPlayer.CurrentRoom) || ExitOccluderCells.Contains(GameManager.Instance.PrimaryPlayer.transform.position.IntXY(VectorConversions.Floor)))
				{
					return RoomHandler.VisibilityStatus.CURRENT;
				}
				return RoomHandler.VisibilityStatus.OBSCURED;
			}
		}

		public RuntimeExitDefinition(RuntimeRoomExitData uExit, RuntimeRoomExitData dExit, RoomHandler upstream, RoomHandler downstream)
		{
			if (upstream.distanceFromEntrance <= downstream.distanceFromEntrance || downstream.area.IsProceduralRoom)
			{
				upstreamExit = uExit;
				downstreamExit = dExit;
				upstreamRoom = upstream;
				downstreamRoom = downstream;
			}
			else
			{
				upstreamExit = dExit;
				downstreamExit = uExit;
				upstreamRoom = downstream;
				downstreamRoom = upstream;
			}
			if (upstream.IsOnCriticalPath && downstream.IsOnCriticalPath)
			{
				isCriticalPath = true;
				if (uExit != null)
				{
					uExit.isCriticalPath = true;
				}
				if (dExit != null)
				{
					dExit.isCriticalPath = true;
				}
			}
			CalculateCellData();
			if (isCriticalPath)
			{
				if (upstreamExit != null && upstreamExit.referencedExit != null)
				{
					BraveUtility.DrawDebugSquare((upstreamExit.referencedExit.GetExitOrigin(0) - IntVector2.One + upstreamRoom.area.basePosition + -3 * DungeonData.GetIntVector2FromDirection(upstreamExit.referencedExit.exitDirection)).ToVector2(), Color.red, 1000f);
				}
				if (downstreamExit != null && downstreamExit.referencedExit != null)
				{
					BraveUtility.DrawDebugSquare((downstreamExit.referencedExit.GetExitOrigin(0) - IntVector2.One + downstreamRoom.area.basePosition + -3 * DungeonData.GetIntVector2FromDirection(downstreamExit.referencedExit.exitDirection)).ToVector2(), Color.blue, 1000f);
				}
			}
		}

		public bool ContainsPosition(IntVector2 position)
		{
			return (m_upstreamCells != null && m_upstreamCells.Contains(position)) || (m_downstreamCells != null && m_downstreamCells.Contains(position));
		}

		public void RemovePosition(IntVector2 position)
		{
			if (m_upstreamCells != null)
			{
				m_upstreamCells.Remove(position);
			}
			if (m_downstreamCells != null)
			{
				m_downstreamCells.Remove(position);
			}
			if (IntermediaryCells != null)
			{
				IntermediaryCells.Remove(position);
			}
		}

		public IntVector2 GetLinearMidpoint(RoomHandler baseRoom)
		{
			if (baseRoom == upstreamRoom)
			{
				if (upstreamExit.jointedExit || downstreamExit == null)
				{
					return upstreamExit.ExitOrigin - IntVector2.One;
				}
				int totalExitLength = (upstreamExit.TotalExitLength + downstreamExit.TotalExitLength) / 2;
				return upstreamExit.referencedExit.GetExitOrigin(totalExitLength) - IntVector2.One;
			}
			if (baseRoom == downstreamRoom)
			{
				if (downstreamExit.jointedExit || upstreamExit == null)
				{
					return downstreamExit.ExitOrigin - IntVector2.One;
				}
				int totalExitLength2 = (upstreamExit.TotalExitLength + downstreamExit.TotalExitLength) / 2;
				return downstreamExit.referencedExit.GetExitOrigin(totalExitLength2) - IntVector2.One;
			}
			Debug.LogError("SHOULD NEVER OCCUR. LIGHTING PLACEMENT ERROR.");
			return IntVector2.Zero;
		}

		public DungeonData.Direction GetDirectionFromRoom(RoomHandler sourceRoom)
		{
			if (sourceRoom == upstreamRoom)
			{
				if (upstreamExit == null || upstreamExit.referencedExit == null)
				{
					return (DungeonData.Direction)((int)(downstreamExit.referencedExit.exitDirection + 4) % 8);
				}
				return upstreamExit.referencedExit.exitDirection;
			}
			if (sourceRoom == downstreamRoom)
			{
				if (downstreamExit == null || downstreamExit.referencedExit == null)
				{
					return (DungeonData.Direction)((int)(upstreamExit.referencedExit.exitDirection + 4) % 8);
				}
				return downstreamExit.referencedExit.exitDirection;
			}
			Debug.LogError("This should never happen.");
			return (DungeonData.Direction)(-1);
		}

		public void GetExitLine(RoomHandler sourceRoom, out Vector2 p1, out Vector2 p2)
		{
			p1 = sourceRoom.GetCellAdjacentToExit(this).ToVector2();
			switch (GetDirectionFromRoom(sourceRoom))
			{
			case DungeonData.Direction.NORTH:
				p1 += new Vector2(0f, 1f);
				p2 = p1 + new Vector2(2f, 0f);
				break;
			case DungeonData.Direction.EAST:
				p1 += new Vector2(1f, 0f);
				p2 = p1 + new Vector2(0f, 2f);
				break;
			case DungeonData.Direction.SOUTH:
				p2 = p1 + new Vector2(2f, 0f);
				break;
			case DungeonData.Direction.WEST:
				p2 = p1 + new Vector2(0f, 2f);
				break;
			default:
				Debug.LogError("This should never happen.");
				p2 = Vector2.zero;
				break;
			}
		}

		public HashSet<IntVector2> GetCellsForRoom(RoomHandler r)
		{
			if (r == upstreamRoom)
			{
				return m_upstreamCells;
			}
			if (r == downstreamRoom)
			{
				return m_downstreamCells;
			}
			return null;
		}

		public HashSet<IntVector2> GetCellsForOtherRoom(RoomHandler r)
		{
			if (r == upstreamRoom)
			{
				return m_downstreamCells;
			}
			if (r == downstreamRoom)
			{
				return m_upstreamCells;
			}
			return null;
		}

		private void PlaceExitDecorables(RoomHandler targetRoom, RuntimeRoomExitData targetExit, RoomHandler otherRoom)
		{
			if (otherRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SPECIAL && otherRoom.area.prototypeRoom.subCategorySpecial == PrototypeDungeonRoom.RoomSpecialSubCategory.WEIRD_SHOP)
			{
				otherRoom.OptionalDoorTopDecorable = ResourceCache.Acquire("Global Prefabs/Purple_Lantern") as GameObject;
			}
			GameObject gameObject = ((!(otherRoom.area.prototypeRoom != null) || !(otherRoom.area.prototypeRoom.doorTopDecorable != null)) ? otherRoom.OptionalDoorTopDecorable : otherRoom.area.prototypeRoom.doorTopDecorable);
			if (targetExit != null && targetExit.referencedExit != null && targetRoom != null && otherRoom != null && !targetRoom.IsSecretRoom && !otherRoom.IsSecretRoom && otherRoom.area.prototypeRoom != null && gameObject != null)
			{
				IntVector2 intVector = targetExit.referencedExit.GetExitOrigin(0) - IntVector2.One + targetRoom.area.basePosition + -3 * DungeonData.GetIntVector2FromDirection(targetExit.referencedExit.exitDirection);
				Vector2 vector = intVector.ToVector2();
				Vector2 vector2 = intVector.ToVector2();
				switch (targetExit.referencedExit.exitDirection)
				{
				case DungeonData.Direction.NORTH:
					vector += new Vector2(-1.5f, 3.5f);
					vector2 += new Vector2(2.5f, 3.5f);
					break;
				case DungeonData.Direction.EAST:
					vector += new Vector2(1.5f, -0.5f);
					vector2 += new Vector2(1.5f, 4.5f);
					break;
				case DungeonData.Direction.SOUTH:
					vector += new Vector2(-1.5f, 0.5f);
					vector2 += new Vector2(2.5f, 0.5f);
					break;
				case DungeonData.Direction.WEST:
					vector += new Vector2(-1.5f, -0.5f);
					vector2 += new Vector2(-1.5f, 4.5f);
					break;
				}
				if (Random.value < 0.4f)
				{
					Object.Instantiate(gameObject, vector.ToVector3ZUp() + gameObject.transform.position, Quaternion.identity);
					return;
				}
				if (Random.value < 0.8f)
				{
					Object.Instantiate(gameObject, vector2.ToVector3ZUp() + gameObject.transform.position, Quaternion.identity);
					return;
				}
				Object.Instantiate(gameObject, vector.ToVector3ZUp() + gameObject.transform.position, Quaternion.identity);
				Object.Instantiate(gameObject, vector2.ToVector3ZUp() + gameObject.transform.position, Quaternion.identity);
			}
		}

		public void ProcessWestgeonData()
		{
			if (m_westgeonProcessed)
			{
				return;
			}
			m_westgeonProcessed = true;
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON)
			{
				if (upstreamExit != null && upstreamExit.referencedExit != null && upstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH && upstreamRoom.RoomVisualSubtype == 0 && downstreamRoom.RoomVisualSubtype != 0)
				{
					IntVector2 exitConnection = upstreamExit.referencedExit.GetExitOrigin(0) - IntVector2.One + upstreamRoom.area.basePosition + -2 * DungeonData.GetIntVector2FromDirection(upstreamExit.referencedExit.exitDirection);
					ProcessWestgeonSection(exitConnection, downstreamRoom);
				}
				if (downstreamExit != null && downstreamExit.referencedExit != null && downstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH && downstreamRoom.RoomVisualSubtype == 0 && upstreamRoom.RoomVisualSubtype != 0)
				{
					IntVector2 exitConnection2 = downstreamExit.referencedExit.GetExitOrigin(0) - IntVector2.One + downstreamRoom.area.basePosition + -2 * DungeonData.GetIntVector2FromDirection(downstreamExit.referencedExit.exitDirection);
					ProcessWestgeonSection(exitConnection2, upstreamRoom);
				}
			}
		}

		private void ProcessWestgeonSection(IntVector2 exitConnection, RoomHandler inheritRoom)
		{
			IntVector2 intVector = exitConnection + IntVector2.Left;
			IntVector2 intVector2 = exitConnection + IntVector2.Right * 2;
			IntVector2 intVector3 = intVector;
			IntVector2 intVector4 = intVector2;
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			int num = -1;
			while (cellData != null && cellData.IsLowerFaceWall())
			{
				intVector3 = cellData.position;
				cellData.cellVisualData.IsFacewallForInteriorTransition = true;
				cellData.cellVisualData.InteriorTransitionIndex = inheritRoom.RoomVisualSubtype;
				num = inheritRoom.RoomVisualSubtype;
				CellData cellData2 = GameManager.Instance.Dungeon.data[cellData.position + IntVector2.Up];
				while (cellData2 != null && (cellData2.IsUpperFacewall() || cellData2.type == CellType.WALL) && (cellData2.nearestRoom == upstreamRoom || cellData2.nearestRoom == downstreamRoom))
				{
					cellData2.cellVisualData.IsFacewallForInteriorTransition = true;
					cellData2.cellVisualData.InteriorTransitionIndex = inheritRoom.RoomVisualSubtype;
					if (!GameManager.Instance.Dungeon.data.CheckInBounds(cellData2.position + IntVector2.Up))
					{
						break;
					}
					cellData2 = GameManager.Instance.Dungeon.data[cellData2.position + IntVector2.Up];
				}
				cellData = GameManager.Instance.Dungeon.data[cellData.position + IntVector2.Left];
			}
			cellData = GameManager.Instance.Dungeon.data[intVector2];
			while (cellData != null && cellData.IsLowerFaceWall())
			{
				intVector4 = cellData.position;
				cellData.cellVisualData.IsFacewallForInteriorTransition = true;
				cellData.cellVisualData.InteriorTransitionIndex = inheritRoom.RoomVisualSubtype;
				num = inheritRoom.RoomVisualSubtype;
				CellData cellData3 = GameManager.Instance.Dungeon.data[cellData.position + IntVector2.Up];
				while (cellData3 != null && (cellData3.IsUpperFacewall() || cellData3.type == CellType.WALL) && (cellData3.nearestRoom == upstreamRoom || cellData3.nearestRoom == downstreamRoom))
				{
					cellData3.cellVisualData.IsFacewallForInteriorTransition = true;
					cellData3.cellVisualData.InteriorTransitionIndex = inheritRoom.RoomVisualSubtype;
					if (!GameManager.Instance.Dungeon.data.CheckInBounds(cellData3.position + IntVector2.Up))
					{
						break;
					}
					cellData3 = GameManager.Instance.Dungeon.data[cellData3.position + IntVector2.Up];
				}
				cellData = GameManager.Instance.Dungeon.data[cellData.position + IntVector2.Right];
			}
			if (num != -1)
			{
				intVector3 += IntVector2.Down;
				intVector4 += IntVector2.Down;
				for (int i = intVector3.x; i <= intVector4.x; i++)
				{
					GameManager.Instance.Dungeon.data[i, intVector4.y].cellVisualData.IsFacewallForInteriorTransition = true;
					GameManager.Instance.Dungeon.data[i, intVector4.y].cellVisualData.InteriorTransitionIndex = num;
				}
			}
		}

		private bool CheckRowIsFloor(int minX, int maxX, int iy)
		{
			for (int i = minX; i <= maxX; i++)
			{
				if (GameManager.Instance.Dungeon.data[i, iy].type != CellType.FLOOR)
				{
					return false;
				}
			}
			return true;
		}

		public IntVector2 GetDownstreamBasePosition()
		{
			if (upstreamRoom.area.IsProceduralRoom)
			{
				return GetUpstreamBasePosition();
			}
			IntVector2 result = IntVector2.Zero;
			switch (upstreamExit.referencedExit.exitDirection)
			{
			case DungeonData.Direction.NORTH:
				result = new IntVector2(int.MaxValue, int.MinValue);
				foreach (IntVector2 downstreamCell in m_downstreamCells)
				{
					result = new IntVector2(Mathf.Min(result.x, downstreamCell.x), Mathf.Max(result.y, downstreamCell.y));
				}
				result += IntVector2.Up;
				break;
			case DungeonData.Direction.EAST:
				result = new IntVector2(int.MinValue, int.MaxValue);
				{
					foreach (IntVector2 downstreamCell2 in m_downstreamCells)
					{
						result = new IntVector2(Mathf.Max(result.x, downstreamCell2.x), Mathf.Min(result.y, downstreamCell2.y));
					}
					return result;
				}
			case DungeonData.Direction.SOUTH:
				result = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 downstreamCell3 in m_downstreamCells)
					{
						result = IntVector2.Min(downstreamCell3, result);
					}
					return result;
				}
			case DungeonData.Direction.WEST:
				result = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 downstreamCell4 in m_downstreamCells)
					{
						result = IntVector2.Min(downstreamCell4, result);
					}
					return result;
				}
			}
			return result;
		}

		public IntVector2 GetUpstreamBasePosition()
		{
			if (downstreamExit == null || downstreamExit.referencedExit == null)
			{
				return GetDownstreamBasePosition();
			}
			IntVector2 result = IntVector2.Zero;
			switch (downstreamExit.referencedExit.exitDirection)
			{
			case DungeonData.Direction.NORTH:
				result = new IntVector2(int.MaxValue, int.MinValue);
				foreach (IntVector2 upstreamCell in m_upstreamCells)
				{
					result = new IntVector2(Mathf.Min(result.x, upstreamCell.x), Mathf.Max(result.y, upstreamCell.y));
				}
				result += IntVector2.Up;
				break;
			case DungeonData.Direction.EAST:
				result = new IntVector2(int.MinValue, int.MaxValue);
				{
					foreach (IntVector2 upstreamCell2 in m_upstreamCells)
					{
						result = new IntVector2(Mathf.Max(result.x, upstreamCell2.x), Mathf.Min(result.y, upstreamCell2.y));
					}
					return result;
				}
			case DungeonData.Direction.SOUTH:
				result = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 upstreamCell3 in m_upstreamCells)
					{
						result = IntVector2.Min(upstreamCell3, result);
					}
					return result;
				}
			case DungeonData.Direction.WEST:
				result = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 upstreamCell4 in m_upstreamCells)
					{
						result = IntVector2.Min(upstreamCell4, result);
					}
					return result;
				}
			}
			return result;
		}

		public IntVector2 GetDownstreamNearDoorPosition()
		{
			if (upstreamRoom.area.IsProceduralRoom)
			{
				return GetUpstreamNearDoorPosition();
			}
			if (downstreamExit == null || downstreamExit.referencedExit == null)
			{
				return GetUpstreamNearDoorPosition();
			}
			IntVector2 zero = IntVector2.Zero;
			switch (downstreamExit.referencedExit.exitDirection)
			{
			case DungeonData.Direction.SOUTH:
				zero = new IntVector2(int.MaxValue, int.MinValue);
				{
					foreach (IntVector2 downstreamCell in m_downstreamCells)
					{
						if (downstreamCell.y > zero.y || (downstreamCell.y == zero.y && downstreamCell.x < zero.x))
						{
							zero = downstreamCell;
						}
					}
					return zero;
				}
			case DungeonData.Direction.WEST:
				zero = new IntVector2(int.MinValue, int.MaxValue);
				{
					foreach (IntVector2 downstreamCell2 in m_downstreamCells)
					{
						if (downstreamCell2.x > zero.x || (downstreamCell2.x == zero.x && downstreamCell2.y < zero.y))
						{
							zero = downstreamCell2;
						}
					}
					return zero;
				}
			case DungeonData.Direction.NORTH:
				zero = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 downstreamCell3 in m_downstreamCells)
					{
						if (downstreamCell3.y < zero.y || (downstreamCell3.y == zero.y && downstreamCell3.x < zero.x))
						{
							zero = downstreamCell3;
						}
					}
					return zero;
				}
			case DungeonData.Direction.EAST:
				zero = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 downstreamCell4 in m_downstreamCells)
					{
						if (downstreamCell4.x < zero.x || (downstreamCell4.x == zero.x && downstreamCell4.y < zero.y))
						{
							zero = downstreamCell4;
						}
					}
					return zero;
				}
			default:
				return zero;
			}
		}

		public IntVector2 GetUpstreamNearDoorPosition()
		{
			if (upstreamExit == null || upstreamExit.referencedExit == null)
			{
				return GetDownstreamNearDoorPosition();
			}
			IntVector2 zero = IntVector2.Zero;
			switch (upstreamExit.referencedExit.exitDirection)
			{
			case DungeonData.Direction.SOUTH:
				zero = new IntVector2(int.MaxValue, int.MinValue);
				{
					foreach (IntVector2 upstreamCell in m_upstreamCells)
					{
						if (upstreamCell.y > zero.y || (upstreamCell.y == zero.y && upstreamCell.x < zero.x))
						{
							zero = upstreamCell;
						}
					}
					return zero;
				}
			case DungeonData.Direction.WEST:
				zero = new IntVector2(int.MinValue, int.MaxValue);
				{
					foreach (IntVector2 upstreamCell2 in m_upstreamCells)
					{
						if (upstreamCell2.x > zero.x || (upstreamCell2.x == zero.x && upstreamCell2.y < zero.y))
						{
							zero = upstreamCell2;
						}
					}
					return zero;
				}
			case DungeonData.Direction.NORTH:
				zero = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 upstreamCell3 in m_upstreamCells)
					{
						if (upstreamCell3.y < zero.y || (upstreamCell3.y == zero.y && upstreamCell3.x < zero.x))
						{
							zero = upstreamCell3;
						}
					}
					return zero;
				}
			case DungeonData.Direction.EAST:
				zero = new IntVector2(int.MaxValue, int.MaxValue);
				{
					foreach (IntVector2 upstreamCell4 in m_upstreamCells)
					{
						if (upstreamCell4.x < zero.x || (upstreamCell4.x == zero.x && upstreamCell4.y < zero.y))
						{
							zero = upstreamCell4;
						}
					}
					return zero;
				}
			default:
				return zero;
			}
		}

		public bool IsVisibleFromRoom(RoomHandler room)
		{
			if (Pixelator.Instance.UseTexturedOcclusion)
			{
				return true;
			}
			if (linkedDoor == null)
			{
				if (room == upstreamRoom)
				{
					if (downstreamRoom != null && downstreamRoom.secretRoomManager != null)
					{
						return downstreamRoom.secretRoomManager.IsOpen;
					}
				}
				else if (room == downstreamRoom && upstreamRoom != null && upstreamRoom.secretRoomManager != null)
				{
					return upstreamRoom.secretRoomManager.IsOpen;
				}
				return room == downstreamRoom;
			}
			if (room == upstreamRoom)
			{
				if (linkedDoor.subsidiaryBlocker == null && linkedDoor.subsidiaryDoor == null)
				{
					if (m_upstreamCells.Contains(GetDoorPositionForExit(upstreamExit, upstreamRoom)))
					{
						return linkedDoor.IsOpenForVisibilityTest;
					}
					return true;
				}
				if (linkedDoor.subsidiaryBlocker != null)
				{
					if (linkedDoor.IsOpenForVisibilityTest && !linkedDoor.subsidiaryBlocker.isSealed)
					{
						return true;
					}
				}
				else if (linkedDoor.subsidiaryDoor != null && linkedDoor.IsOpenForVisibilityTest && linkedDoor.subsidiaryDoor.IsOpenForVisibilityTest)
				{
					return true;
				}
				if (m_upstreamCells.Contains(GetDoorPositionForExit(upstreamExit, upstreamRoom)))
				{
					if (linkedDoor.IsOpenForVisibilityTest)
					{
						return true;
					}
				}
				else if (linkedDoor.subsidiaryBlocker != null)
				{
					if (!linkedDoor.subsidiaryBlocker.isSealed)
					{
						return true;
					}
				}
				else if (linkedDoor.subsidiaryDoor != null && linkedDoor.subsidiaryDoor.IsOpenForVisibilityTest)
				{
					return true;
				}
				return false;
			}
			if (room == downstreamRoom)
			{
				if (linkedDoor.subsidiaryBlocker == null && linkedDoor.subsidiaryDoor == null)
				{
					if (m_downstreamCells.Contains(GetDoorPositionForExit(upstreamExit, upstreamRoom)))
					{
						return linkedDoor.IsOpenForVisibilityTest;
					}
					return true;
				}
				if (linkedDoor.subsidiaryBlocker != null)
				{
					if (linkedDoor.IsOpenForVisibilityTest && !linkedDoor.subsidiaryBlocker.isSealed)
					{
						return true;
					}
				}
				else if (linkedDoor.subsidiaryDoor != null && linkedDoor.IsOpenForVisibilityTest && linkedDoor.subsidiaryDoor.IsOpenForVisibilityTest)
				{
					return true;
				}
				if (m_upstreamCells.Contains(GetDoorPositionForExit(upstreamExit, upstreamRoom)))
				{
					if (linkedDoor.subsidiaryBlocker != null)
					{
						if (!linkedDoor.subsidiaryBlocker.isSealed)
						{
							return true;
						}
					}
					else if (linkedDoor.subsidiaryDoor != null && linkedDoor.subsidiaryDoor.IsOpenForVisibilityTest)
					{
						return true;
					}
				}
				else if (linkedDoor.IsOpenForVisibilityTest)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public void ProcessExitDecorables()
		{
			if ((upstreamRoom == null || (!(upstreamRoom.secretRoomManager != null) && upstreamRoom.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.SECRET)) && (downstreamRoom == null || (!(downstreamRoom.secretRoomManager != null) && downstreamRoom.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.SECRET)))
			{
				PlaceExitDecorables(upstreamRoom, upstreamExit, downstreamRoom);
				PlaceExitDecorables(downstreamRoom, downstreamExit, upstreamRoom);
			}
		}

		public void StampCellVisualTypes(DungeonData dungeonData)
		{
			if (GameManager.Instance.Dungeon.UsesWallWarpWingDoors && ((upstreamExit != null && upstreamExit.isWarpWingStart) || (downstreamExit != null && downstreamExit.isWarpWingStart)))
			{
				GenerateWarpWingPortals();
			}
			foreach (IntVector2 item in GetCellsForRoom(upstreamRoom))
			{
				if (upstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH || upstreamExit.referencedExit.exitDirection == DungeonData.Direction.SOUTH)
				{
					dungeonData[item.x, item.y].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
					dungeonData[item.x - 1, item.y + 2].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
					dungeonData[item.x + 1, item.y + 2].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
					continue;
				}
				dungeonData[item.x, item.y].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
				if (dungeonData[item.x, item.y + 1].type == CellType.WALL)
				{
					dungeonData[item.x, item.y + 1].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
				}
				if (dungeonData[item.x, item.y + 2].type == CellType.WALL)
				{
					dungeonData[item.x, item.y + 2].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
				}
				if (dungeonData[item.x, item.y + 3].type == CellType.WALL)
				{
					dungeonData[item.x, item.y + 3].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
				}
			}
			if (downstreamRoom != null && downstreamExit != null)
			{
				foreach (IntVector2 item2 in GetCellsForRoom(downstreamRoom))
				{
					if (downstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH || downstreamExit.referencedExit.exitDirection == DungeonData.Direction.SOUTH)
					{
						dungeonData[item2.x, item2.y].cellVisualData.roomVisualTypeIndex = downstreamRoom.RoomVisualSubtype;
						dungeonData[item2.x - 1, item2.y + 1].cellVisualData.roomVisualTypeIndex = downstreamRoom.RoomVisualSubtype;
						dungeonData[item2.x + 1, item2.y + 1].cellVisualData.roomVisualTypeIndex = downstreamRoom.RoomVisualSubtype;
						continue;
					}
					dungeonData[item2.x, item2.y].cellVisualData.roomVisualTypeIndex = downstreamRoom.RoomVisualSubtype;
					if (dungeonData[item2.x, item2.y + 1].type == CellType.WALL)
					{
						dungeonData[item2.x, item2.y + 1].cellVisualData.roomVisualTypeIndex = downstreamRoom.RoomVisualSubtype;
					}
					if (dungeonData[item2.x, item2.y + 2].type == CellType.WALL)
					{
						dungeonData[item2.x, item2.y + 2].cellVisualData.roomVisualTypeIndex = downstreamRoom.RoomVisualSubtype;
					}
					if (dungeonData[item2.x, item2.y + 3].type == CellType.WALL)
					{
						dungeonData[item2.x, item2.y + 3].cellVisualData.roomVisualTypeIndex = downstreamRoom.RoomVisualSubtype;
					}
				}
			}
			if (IntermediaryCells == null || IntermediaryCells.Count <= 0)
			{
				return;
			}
			foreach (IntVector2 intermediaryCell in IntermediaryCells)
			{
				dungeonData[intermediaryCell.x, intermediaryCell.y].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
				for (int i = -1; i < 2; i++)
				{
					if (i != 0 && dungeonData[intermediaryCell.x + i, intermediaryCell.y].type != CellType.WALL)
					{
						continue;
					}
					int num = ((!upstreamExit.jointedExit || (upstreamExit.referencedExit.exitDirection != DungeonData.Direction.SOUTH && downstreamExit.referencedExit.exitDirection != DungeonData.Direction.SOUTH)) ? 2 : 0);
					for (int j = num; j < 4; j++)
					{
						if (dungeonData[intermediaryCell.x + i, intermediaryCell.y + j].type == CellType.WALL)
						{
							dungeonData[intermediaryCell.x + i, intermediaryCell.y + j].cellVisualData.roomVisualTypeIndex = upstreamRoom.RoomVisualSubtype;
						}
					}
				}
			}
		}

		protected void CleanupCellDataForWarpWingExits()
		{
			IntVector2 lhs = new IntVector2(int.MaxValue, int.MaxValue);
			IntVector2 lhs2 = new IntVector2(int.MinValue, int.MinValue);
			foreach (IntVector2 upstreamCell in m_upstreamCells)
			{
				lhs = IntVector2.Min(lhs, upstreamCell);
				lhs2 = IntVector2.Max(lhs2, upstreamCell);
			}
			for (int i = lhs.x; i <= lhs2.x; i++)
			{
				for (int j = lhs.y; j <= lhs2.y; j++)
				{
					m_upstreamCells.Add(new IntVector2(i, j));
				}
			}
		}

		protected void CalculateCellData()
		{
			m_upstreamCells = new HashSet<IntVector2>();
			m_downstreamCells = new HashSet<IntVector2>();
			ExitOccluderCells = new HashSet<IntVector2>();
			DungeonData data = GameManager.Instance.Dungeon.data;
			IntVector2 doorPositionForExit = GetDoorPositionForExit(upstreamExit, upstreamRoom);
			bool flag = RequiresSubDoor();
			IntVector2 intVector = IntVector2.Zero;
			if (flag)
			{
				intVector = GetSubDoorPositionForExit(upstreamExit, upstreamRoom);
			}
			if (flag)
			{
				IntermediaryCells = new HashSet<IntVector2>();
			}
			if (upstreamExit != null)
			{
				IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(upstreamExit.referencedExit.exitDirection);
				int num = ((upstreamExit.jointedExit && upstreamExit.referencedExit.exitDirection != DungeonData.Direction.WEST) ? 1 : 0);
				if (downstreamRoom.area.prototypeRoom == null && downstreamRoom.area.IsProceduralRoom && downstreamRoom.area.proceduralCells == null && upstreamExit.referencedExit.exitDirection != DungeonData.Direction.EAST)
				{
					num--;
				}
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				int num2 = 2;
				for (int i = 0; i < upstreamExit.TotalExitLength + num; i++)
				{
					for (int j = 0; j < upstreamExit.referencedExit.containedCells.Count; j++)
					{
						IntVector2 intVector2 = upstreamExit.referencedExit.containedCells[j].ToIntVector2() - IntVector2.One + upstreamRoom.area.basePosition + intVector2FromDirection * i;
						if (intVector2 == doorPositionForExit)
						{
							flag2 = true;
						}
						if (flag)
						{
							if (intVector2 == intVector)
							{
								flag4 = true;
							}
							if (flag4 || flag2)
							{
								IntermediaryCells.Add(intVector2);
							}
						}
						if (flag3)
						{
							m_downstreamCells.Add(intVector2);
						}
						else
						{
							m_upstreamCells.Add(intVector2);
						}
						if (i <= num2 && data.CheckInBoundsAndValid(intVector2))
						{
							data[intVector2].occlusionData.sharedRoomAndExitCell = true;
						}
					}
					if (flag2)
					{
						flag3 = true;
					}
				}
			}
			if (downstreamExit != null)
			{
				IntVector2 intVector2FromDirection2 = DungeonData.GetIntVector2FromDirection(downstreamExit.referencedExit.exitDirection);
				int num3 = ((downstreamExit.jointedExit && downstreamExit.referencedExit.exitDirection != DungeonData.Direction.WEST) ? 1 : 0);
				if (downstreamRoom.area.prototypeRoom == null && downstreamRoom.area.IsProceduralRoom && downstreamRoom.area.proceduralCells == null && upstreamExit.referencedExit.exitDirection != DungeonData.Direction.EAST)
				{
					num3--;
				}
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = false;
				int num4 = 2;
				for (int k = 0; k < downstreamExit.TotalExitLength + num3; k++)
				{
					for (int l = 0; l < downstreamExit.referencedExit.containedCells.Count; l++)
					{
						IntVector2 intVector3 = downstreamExit.referencedExit.containedCells[l].ToIntVector2() - IntVector2.One + downstreamRoom.area.basePosition + intVector2FromDirection2 * k;
						if (intVector3 == doorPositionForExit)
						{
							flag5 = true;
						}
						if (flag)
						{
							if (intVector3 == intVector)
							{
								flag7 = true;
							}
							if (flag7 || flag5)
							{
								IntermediaryCells.Add(intVector3);
							}
						}
						if (flag6)
						{
							m_upstreamCells.Add(intVector3);
						}
						else
						{
							m_downstreamCells.Add(intVector3);
						}
						if (k <= num4)
						{
							if (!data.CheckInBoundsAndValid(intVector3))
							{
								Debug.Log(string.Concat(intVector3, " is out of bounds for ", (upstreamRoom == null) ? "null" : upstreamRoom.GetRoomName(), " | ", (downstreamRoom == null) ? "null" : downstreamRoom.GetRoomName()));
								Debug.Log(string.Concat(upstreamRoom.area.basePosition, "|", downstreamRoom.area.basePosition));
							}
							else
							{
								data[intVector3].occlusionData.sharedRoomAndExitCell = true;
							}
						}
					}
					if (flag5)
					{
						flag6 = true;
					}
				}
			}
			if (downstreamExit != null)
			{
				IntVector2 item = upstreamRoom.area.basePosition + upstreamExit.ExitOrigin - IntVector2.One;
				if (upstreamExit.referencedExit.exitDirection == DungeonData.Direction.EAST || downstreamExit.referencedExit.exitDirection == DungeonData.Direction.EAST)
				{
					item += IntVector2.Right;
				}
				if (upstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH || downstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH)
				{
					item += IntVector2.Up;
				}
				IntVector2 item2 = downstreamRoom.area.basePosition + downstreamExit.ExitOrigin - IntVector2.One;
				if (upstreamExit.referencedExit.exitDirection == DungeonData.Direction.EAST || downstreamExit.referencedExit.exitDirection == DungeonData.Direction.EAST)
				{
					item2 += IntVector2.Right;
				}
				if (upstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH || downstreamExit.referencedExit.exitDirection == DungeonData.Direction.NORTH)
				{
					item2 += IntVector2.Up;
				}
				if (IntermediaryCells == null)
				{
					IntermediaryCells = new HashSet<IntVector2>();
				}
				if (!m_upstreamCells.Contains(item) && !m_downstreamCells.Contains(item) && !IntermediaryCells.Contains(item))
				{
					m_upstreamCells.Add(item);
					IntermediaryCells.Add(item);
				}
				if (!m_upstreamCells.Contains(item2) && !m_downstreamCells.Contains(item2) && !IntermediaryCells.Contains(item2))
				{
					m_downstreamCells.Add(item2);
					IntermediaryCells.Add(item2);
				}
			}
			if ((upstreamRoom != null && !upstreamRoom.area.IsProceduralRoom && upstreamRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET) || (downstreamRoom != null && !downstreamRoom.area.IsProceduralRoom && downstreamRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET))
			{
				CorrectForSecretRoomDoorlessness();
			}
			if ((upstreamExit != null && upstreamExit.isWarpWingStart) || (downstreamExit != null && downstreamExit.isWarpWingStart))
			{
				CleanupCellDataForWarpWingExits();
			}
			if (m_upstreamCells != null)
			{
				foreach (IntVector2 upstreamCell in m_upstreamCells)
				{
					ExitOccluderCells.Add(upstreamCell);
				}
			}
			if (m_downstreamCells != null)
			{
				foreach (IntVector2 downstreamCell in m_downstreamCells)
				{
					ExitOccluderCells.Add(downstreamCell);
				}
			}
			if (IntermediaryCells != null)
			{
				foreach (IntVector2 intermediaryCell in IntermediaryCells)
				{
					ExitOccluderCells.Add(intermediaryCell);
				}
			}
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			foreach (IntVector2 exitOccluderCell in ExitOccluderCells)
			{
				if (ExitOccluderCells.Contains(exitOccluderCell + IntVector2.Right * 2) || ExitOccluderCells.Contains(exitOccluderCell + IntVector2.Left * 2) || (ExitOccluderCells.Contains(exitOccluderCell + IntVector2.Left) && ExitOccluderCells.Contains(exitOccluderCell + IntVector2.Right)))
				{
					hashSet.Add(exitOccluderCell + IntVector2.Up);
					hashSet.Add(exitOccluderCell + IntVector2.Up * 2);
				}
			}
			foreach (IntVector2 item3 in hashSet)
			{
				ExitOccluderCells.Add(item3);
			}
			foreach (IntVector2 exitOccluderCell2 in ExitOccluderCells)
			{
				if (data.CheckInBoundsAndValid(exitOccluderCell2))
				{
					data[exitOccluderCell2].occlusionData.occlusionParentDefintion = this;
				}
			}
		}

		public void CorrectForSecretRoomDoorlessness()
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			IntVector2[] cardinals = IntVector2.Cardinals;
			foreach (IntVector2 upstreamCell in m_upstreamCells)
			{
				if (data.CheckInBoundsAndValid(upstreamCell))
				{
					data[upstreamCell].occlusionData.sharedRoomAndExitCell = true;
					data[upstreamCell].parentRoom = upstreamRoom;
					data[upstreamCell].parentArea = upstreamRoom.area;
				}
			}
			foreach (IntVector2 downstreamCell in m_downstreamCells)
			{
				if (data.CheckInBoundsAndValid(downstreamCell))
				{
					data[downstreamCell].occlusionData.sharedRoomAndExitCell = true;
					data[downstreamCell].parentRoom = upstreamRoom;
					data[downstreamCell].parentArea = upstreamRoom.area;
				}
			}
			foreach (IntVector2 downstreamCell2 in m_downstreamCells)
			{
				for (int i = 0; i < cardinals.Length; i++)
				{
					IntVector2 item = downstreamCell2 + cardinals[i];
					if (!m_downstreamCells.Contains(item) && m_upstreamCells.Contains(item))
					{
						if (IntermediaryCells == null)
						{
							IntermediaryCells = new HashSet<IntVector2>();
						}
						IntermediaryCells.Add(item);
						IntermediaryCells.Add(downstreamCell2);
					}
				}
			}
		}

		protected DungeonData.Direction GetSubsidiaryDoorDirection()
		{
			DungeonData.Direction direction = DungeonData.Direction.NORTH;
			if (upstreamExit.jointedExit)
			{
				if (!upstreamExit.oneWayDoor && (upstreamExit.referencedExit.exitDirection == DungeonData.Direction.EAST || upstreamExit.referencedExit.exitDirection == DungeonData.Direction.WEST))
				{
					return upstreamExit.referencedExit.exitDirection;
				}
				return downstreamExit.referencedExit.exitDirection;
			}
			return downstreamExit.referencedExit.exitDirection;
		}

		protected void GenerateSubsidiaryDoor(DungeonData dungeonData, DungeonPlaceable sourcePlaceable, DungeonDoorController mainDoor, Transform doorParentTransform)
		{
			IntVector2 subDoorPositionForExit = GetSubDoorPositionForExit(upstreamExit, upstreamRoom);
			if (dungeonData.HasDoorAtPosition(subDoorPositionForExit))
			{
				Debug.LogError("Attempting to generate subdoor for position twice.");
				return;
			}
			DungeonData.Direction subsidiaryDoorDirection = GetSubsidiaryDoorDirection();
			IntVector2 location = subDoorPositionForExit - upstreamRoom.area.basePosition;
			GameObject gameObject = sourcePlaceable.InstantiateObjectDirectional(upstreamRoom, location, subsidiaryDoorDirection);
			gameObject.transform.parent = doorParentTransform;
			DungeonDoorController component = gameObject.GetComponent<DungeonDoorController>();
			component.exitDefinition = this;
			mainDoor.subsidiaryDoor = component;
			component.parentDoor = mainDoor;
		}

		public void GenerateStandaloneRoomBlocker(DungeonData dungeonData, Transform parentTransform)
		{
			if (GameManager.Instance.Dungeon.phantomBlockerDoorObjects == null)
			{
				return;
			}
			RuntimeRoomExitData runtimeRoomExitData = upstreamExit;
			int num = runtimeRoomExitData.TotalExitLength + runtimeRoomExitData.linkedExit.TotalExitLength - 3;
			if (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.SOUTH)
			{
				num--;
			}
			IntVector2 intVector = runtimeRoomExitData.referencedExit.GetExitOrigin(num) - IntVector2.One;
			IntVector2 position = intVector + upstreamRoom.area.basePosition;
			if (dungeonData.HasDoorAtPosition(position))
			{
				Debug.LogError("Attempting to generate subdoor for position twice.");
				return;
			}
			DungeonData.Direction exitDirection = upstreamExit.referencedExit.exitDirection;
			GameObject gameObject = GameManager.Instance.Dungeon.phantomBlockerDoorObjects.InstantiateObjectDirectional(upstreamRoom, intVector, exitDirection);
			gameObject.transform.parent = parentTransform;
			DungeonDoorSubsidiaryBlocker component = gameObject.GetComponent<DungeonDoorSubsidiaryBlocker>();
			if (downstreamRoom != null)
			{
				downstreamRoom.standaloneBlockers.Add(component);
			}
			if (upstreamRoom != null)
			{
				upstreamRoom.standaloneBlockers.Add(component);
			}
		}

		public void GenerateSecretRoomBlocker(DungeonData dungeonData, SecretRoomManager secretManager, SecretRoomDoorBeer secretDoor, Transform parentTransform)
		{
			if (!(GameManager.Instance.Dungeon.phantomBlockerDoorObjects == null))
			{
				RuntimeRoomExitData runtimeRoomExitData = ((secretManager.room != upstreamRoom) ? downstreamExit : upstreamExit);
				int num = runtimeRoomExitData.TotalExitLength + runtimeRoomExitData.linkedExit.TotalExitLength - 3;
				if (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.SOUTH)
				{
					num--;
				}
				IntVector2 intVector = runtimeRoomExitData.referencedExit.GetExitOrigin(num) - IntVector2.One;
				IntVector2 position = intVector + secretManager.room.area.basePosition;
				if (dungeonData.HasDoorAtPosition(position))
				{
					Debug.LogError("Attempting to generate subdoor for position twice.");
					return;
				}
				DungeonData.Direction direction = ((secretDoor.exitDef.upstreamRoom != secretManager.room) ? secretDoor.exitDef.downstreamExit.referencedExit.exitDirection : secretDoor.exitDef.upstreamExit.referencedExit.exitDirection);
				GameObject gameObject = GameManager.Instance.Dungeon.phantomBlockerDoorObjects.InstantiateObjectDirectional(secretManager.room, intVector, direction);
				gameObject.transform.parent = parentTransform;
				DungeonDoorSubsidiaryBlocker component = gameObject.GetComponent<DungeonDoorSubsidiaryBlocker>();
				component.ToggleRenderers(false);
				secretDoor.subsidiaryBlocker = component;
			}
		}

		protected void GeneratePhantomDoorBlocker(DungeonData dungeonData, DungeonDoorController mainDoor, Transform doorParentTransform)
		{
			if (!(GameManager.Instance.Dungeon.phantomBlockerDoorObjects == null) && (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.OFFICEGEON || !mainDoor.OneWayDoor))
			{
				IntVector2 subDoorPositionForExit = GetSubDoorPositionForExit(upstreamExit, upstreamRoom);
				if (dungeonData.HasDoorAtPosition(subDoorPositionForExit))
				{
					Debug.LogError("Attempting to generate subdoor for position twice.");
					return;
				}
				DungeonData.Direction direction = DungeonData.Direction.NORTH;
				direction = ((!upstreamExit.jointedExit) ? downstreamExit.referencedExit.exitDirection : ((upstreamExit.referencedExit.exitDirection != DungeonData.Direction.EAST && upstreamExit.referencedExit.exitDirection != DungeonData.Direction.WEST) ? downstreamExit.referencedExit.exitDirection : upstreamExit.referencedExit.exitDirection));
				IntVector2 location = subDoorPositionForExit - upstreamRoom.area.basePosition;
				GameObject gameObject = GameManager.Instance.Dungeon.phantomBlockerDoorObjects.InstantiateObjectDirectional(upstreamRoom, location, direction);
				gameObject.transform.parent = doorParentTransform;
				(mainDoor.subsidiaryBlocker = gameObject.GetComponent<DungeonDoorSubsidiaryBlocker>()).parentDoor = mainDoor;
			}
		}

		private void GenerateWarpWingPortals()
		{
			bool flag = false;
			if (GameManager.Instance.Dungeon.UsesWallWarpWingDoors && upstreamRoom != null && downstreamRoom != null)
			{
				if (m_upstreamCells != null)
				{
					m_upstreamCells.Clear();
				}
				if (m_downstreamCells != null)
				{
					m_downstreamCells.Clear();
				}
				if (IntermediaryCells != null)
				{
					IntermediaryCells.Clear();
				}
				int wallClearanceWidth = GameManager.Instance.Dungeon.WarpWingDoorPrefab.GetComponent<PlacedWallDecorator>().wallClearanceWidth;
				List<TK2DInteriorDecorator.WallExpanse> list = upstreamRoom.GatherExpanses(DungeonData.Direction.NORTH, true, false, true);
				List<TK2DInteriorDecorator.WallExpanse> list2 = downstreamRoom.GatherExpanses(DungeonData.Direction.NORTH, true, false, true);
				Debug.Log(list.Count + "|" + list2.Count + "| req width: " + wallClearanceWidth);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].width < wallClearanceWidth)
					{
						list.RemoveAt(i);
						i--;
					}
				}
				for (int j = 0; j < list2.Count; j++)
				{
					if (list2[j].width < wallClearanceWidth)
					{
						list2.RemoveAt(j);
						j--;
					}
				}
				Debug.Log(list.Count + "|" + list2.Count + "| post cull");
				TK2DInteriorDecorator.WallExpanse? wallExpanse = ((list.Count <= 0) ? null : new TK2DInteriorDecorator.WallExpanse?(list[Random.Range(0, list.Count)]));
				TK2DInteriorDecorator.WallExpanse? wallExpanse2 = ((list2.Count <= 0) ? null : new TK2DInteriorDecorator.WallExpanse?(list2[Random.Range(0, list2.Count)]));
				if (wallExpanse.HasValue && wallExpanse2.HasValue)
				{
					GameObject warpWingDoorPrefab = GameManager.Instance.Dungeon.WarpWingDoorPrefab;
					int num = 0;
					if (wallExpanse.Value.width > wallClearanceWidth)
					{
						num = Mathf.CeilToInt((float)wallExpanse.Value.width / 2f - (float)wallClearanceWidth / 2f);
					}
					int num2 = 0;
					if (wallExpanse2.Value.width > wallClearanceWidth)
					{
						num2 = Mathf.CeilToInt((float)wallExpanse2.Value.width / 2f - (float)wallClearanceWidth / 2f);
					}
					Vector3 vector = wallExpanse.Value.basePosition.ToVector3() + Vector3.right * num + Vector3.up;
					Vector3 vector2 = wallExpanse2.Value.basePosition.ToVector3() + Vector3.right * num2 + Vector3.up;
					WarpPointHandler component = Object.Instantiate(warpWingDoorPrefab, upstreamRoom.area.basePosition.ToVector3() + vector + warpWingDoorPrefab.transform.localPosition, Quaternion.identity).GetComponent<WarpPointHandler>();
					WarpPointHandler component2 = Object.Instantiate(warpWingDoorPrefab, downstreamRoom.area.basePosition.ToVector3() + vector2 + warpWingDoorPrefab.transform.localPosition, Quaternion.identity).GetComponent<WarpPointHandler>();
					PlacedWallDecorator component3 = component.GetComponent<PlacedWallDecorator>();
					if ((bool)component3)
					{
						component3.ConfigureOnPlacement(upstreamRoom);
					}
					PlacedWallDecorator component4 = component2.GetComponent<PlacedWallDecorator>();
					if ((bool)component4)
					{
						component4.ConfigureOnPlacement(downstreamRoom);
					}
					component.GetComponent<PlacedWallDecorator>().ConfigureOnPlacement(upstreamRoom);
					component2.GetComponent<PlacedWallDecorator>().ConfigureOnPlacement(downstreamRoom);
					component.spawnOffset = new Vector2(0f, -0.25f);
					component2.spawnOffset = new Vector2(0f, -0.25f);
					component.SetTarget(component2);
					component2.SetTarget(component);
					flag = true;
				}
			}
			if (!flag)
			{
				GameObject original = (GameObject)BraveResources.Load("Global Prefabs/WarpWing_Portal");
				GameObject gameObject = Object.Instantiate(original);
				GameObject gameObject2 = Object.Instantiate(original);
				WarpWingPortalController component5 = gameObject.GetComponent<WarpWingPortalController>();
				WarpWingPortalController warpWingPortalController = (component5.pairedPortal = gameObject2.GetComponent<WarpWingPortalController>());
				component5.parentRoom = upstreamRoom;
				component5.parentExit = upstreamExit;
				warpWingPortalController.pairedPortal = component5;
				warpWingPortalController.parentRoom = downstreamRoom;
				warpWingPortalController.parentExit = downstreamExit;
				upstreamExit.warpWingPortal = component5;
				downstreamExit.warpWingPortal = warpWingPortalController;
				IntVector2 doorPositionForExit = GetDoorPositionForExit(upstreamExit, upstreamRoom, true);
				IntVector2 doorPositionForExit2 = GetDoorPositionForExit(downstreamExit, downstreamRoom, true);
				doorPositionForExit += DungeonData.GetIntVector2FromDirection(upstreamExit.referencedExit.exitDirection) * 3;
				doorPositionForExit2 += DungeonData.GetIntVector2FromDirection(downstreamExit.referencedExit.exitDirection) * 3;
				component5.transform.position = doorPositionForExit.ToVector3();
				warpWingPortalController.transform.position = doorPositionForExit2.ToVector3();
				RoomHandler.unassignedInteractableObjects.Add(component5);
				RoomHandler.unassignedInteractableObjects.Add(warpWingPortalController);
			}
		}

		public void GenerateDoorsForExit(DungeonData dungeonData, Transform doorParentTransform)
		{
			if (!GameManager.Instance.Dungeon.UsesWallWarpWingDoors && ((upstreamExit != null && upstreamExit.isWarpWingStart) || (downstreamExit != null && downstreamExit.isWarpWingStart)))
			{
				GenerateWarpWingPortals();
			}
			else if ((upstreamExit != null && upstreamExit.isWarpWingStart) || (downstreamExit != null && downstreamExit.isWarpWingStart))
			{
				return;
			}
			bool flag = false;
			if (upstreamRoom != null && upstreamRoom.area.prototypeRoom != null && upstreamRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET)
			{
				flag = true;
			}
			if (downstreamRoom != null && downstreamRoom.area.prototypeRoom != null && downstreamRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET)
			{
				flag = true;
			}
			if (flag && (upstreamExit == null || !upstreamExit.oneWayDoor) && (downstreamExit == null || !downstreamExit.oneWayDoor))
			{
				return;
			}
			if (upstreamRoom.area.PrototypeLostWoodsRoom || downstreamRoom.area.PrototypeLostWoodsRoom)
			{
				GenerateStandaloneRoomBlocker(dungeonData, doorParentTransform);
				flag = true;
				return;
			}
			IntVector2 doorPositionForExit = GetDoorPositionForExit(upstreamExit, upstreamRoom);
			if (dungeonData.HasDoorAtPosition(doorPositionForExit))
			{
				Debug.LogError("Attempting to generate door for position twice.");
				return;
			}
			GameObject gameObject = null;
			DungeonData.Direction direction = DungeonData.Direction.NORTH;
			direction = ((downstreamExit == null) ? upstreamExit.referencedExit.exitDirection : ((upstreamExit == null) ? downstreamExit.referencedExit.exitDirection : ((!upstreamExit.jointedExit) ? upstreamExit.referencedExit.exitDirection : ((upstreamExit.oneWayDoor || (upstreamExit.referencedExit.exitDirection != DungeonData.Direction.EAST && upstreamExit.referencedExit.exitDirection != DungeonData.Direction.WEST)) ? upstreamExit.referencedExit.exitDirection : downstreamExit.referencedExit.exitDirection))));
			DungeonPlaceable dungeonPlaceable = null;
			if (upstreamExit != null && upstreamExit.oneWayDoor)
			{
				IntVector2 location = doorPositionForExit - upstreamRoom.area.basePosition;
				if (direction == DungeonData.Direction.EAST || direction == DungeonData.Direction.WEST)
				{
					location += IntVector2.Down;
				}
				gameObject = GameManager.Instance.Dungeon.oneWayDoorObjects.InstantiateObjectDirectional(upstreamRoom, location, direction);
				dungeonPlaceable = GameManager.Instance.Dungeon.oneWayDoorObjects;
			}
			else if (upstreamExit != null && upstreamExit.isLockedDoor && GameManager.Instance.Dungeon.lockedDoorObjects != null)
			{
				IntVector2 location2 = doorPositionForExit - upstreamRoom.area.basePosition;
				gameObject = GameManager.Instance.Dungeon.lockedDoorObjects.InstantiateObjectDirectional(upstreamRoom, location2, direction);
				dungeonPlaceable = GameManager.Instance.Dungeon.lockedDoorObjects;
			}
			else if (downstreamExit != null && downstreamExit.referencedExit.specifiedDoor != null)
			{
				IntVector2 location3 = doorPositionForExit - downstreamRoom.area.basePosition;
				dungeonPlaceable = downstreamExit.referencedExit.specifiedDoor;
				if (dungeonPlaceable.variantTiers.Count > 0 && dungeonPlaceable.variantTiers[0].nonDatabasePlaceable != null)
				{
					DungeonDoorController component = dungeonPlaceable.variantTiers[0].nonDatabasePlaceable.GetComponent<DungeonDoorController>();
					if (component != null && component.Mode == DungeonDoorController.DungeonDoorMode.FINAL_BOSS_DOOR)
					{
						location3 += IntVector2.Right;
					}
				}
				gameObject = dungeonPlaceable.InstantiateObjectDirectional(downstreamRoom, location3, direction);
			}
			else if (upstreamExit != null && upstreamExit.referencedExit.specifiedDoor != null)
			{
				IntVector2 location4 = doorPositionForExit - upstreamRoom.area.basePosition;
				dungeonPlaceable = upstreamExit.referencedExit.specifiedDoor;
				if (dungeonPlaceable.variantTiers.Count > 0 && dungeonPlaceable.variantTiers[0].nonDatabasePlaceable != null)
				{
					DungeonDoorController component2 = dungeonPlaceable.variantTiers[0].nonDatabasePlaceable.GetComponent<DungeonDoorController>();
					if (component2 != null && component2.Mode == DungeonDoorController.DungeonDoorMode.FINAL_BOSS_DOOR)
					{
						location4 += IntVector2.Right;
					}
				}
				gameObject = dungeonPlaceable.InstantiateObjectDirectional(upstreamRoom, location4, direction);
			}
			else
			{
				if (flag)
				{
					return;
				}
				IntVector2 location5 = doorPositionForExit - upstreamRoom.area.basePosition;
				Dungeon dungeon = GameManager.Instance.Dungeon;
				if (dungeon.alternateDoorObjectsNakatomi != null)
				{
					if (downstreamRoom != null && (downstreamRoom.RoomVisualSubtype == 7 || downstreamRoom.RoomVisualSubtype == 8 || upstreamRoom.RoomVisualSubtype == 7 || upstreamRoom.RoomVisualSubtype == 8))
					{
						gameObject = dungeon.alternateDoorObjectsNakatomi.InstantiateObjectDirectional(upstreamRoom, location5, direction);
						dungeonPlaceable = dungeon.alternateDoorObjectsNakatomi;
					}
					else
					{
						gameObject = dungeon.doorObjects.InstantiateObjectDirectional(upstreamRoom, location5, direction);
						dungeonPlaceable = dungeon.doorObjects;
					}
				}
				else
				{
					gameObject = dungeon.doorObjects.InstantiateObjectDirectional(upstreamRoom, location5, direction);
					dungeonPlaceable = dungeon.doorObjects;
				}
			}
			if (dungeonPlaceable == null)
			{
				return;
			}
			gameObject.transform.parent = doorParentTransform;
			DungeonDoorController component3 = gameObject.GetComponent<DungeonDoorController>();
			if (dungeonPlaceable == GameManager.Instance.Dungeon.lockedDoorObjects && upstreamExit.referencedExit.exitDirection == DungeonData.Direction.EAST)
			{
				component3.FlipLockToOtherSide();
			}
			if ((downstreamExit != null && downstreamExit.oneWayDoor) || (upstreamExit != null && upstreamExit.oneWayDoor))
			{
				component3.OneWayDoor = true;
				GameObject gameObject2 = Object.Instantiate(GameManager.Instance.Dungeon.oneWayDoorPressurePlate);
				Vector3 vector = Vector3.zero;
				if (direction == DungeonData.Direction.WEST || direction == DungeonData.Direction.EAST)
				{
					vector = Vector3.up;
				}
				gameObject2.transform.position = component3.transform.position + (DungeonData.GetIntVector2FromDirection(direction) * 2).ToVector3() + vector;
				PressurePlate component4 = gameObject2.GetComponent<PressurePlate>();
				component3.AssignPressurePlate(component4);
			}
			foreach (IntVector2 upstreamCell in m_upstreamCells)
			{
				dungeonData[upstreamCell].exitDoor = component3;
			}
			foreach (IntVector2 downstreamCell in m_downstreamCells)
			{
				dungeonData[downstreamCell].exitDoor = component3;
			}
			component3.upstreamRoom = upstreamRoom;
			component3.downstreamRoom = downstreamRoom;
			component3.exitDefinition = this;
			upstreamRoom.connectedDoors.Add(component3);
			downstreamRoom.connectedDoors.Add(component3);
			IntVector2 intVector = IntVector2.Zero;
			if (component3.Mode == DungeonDoorController.DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS)
			{
				GeneratePhantomDoorBlocker(dungeonData, component3, doorParentTransform);
			}
			else if (RequiresSubDoor() && !flag)
			{
				DungeonPlaceable sourcePlaceable = ((!(dungeonPlaceable == GameManager.Instance.Dungeon.lockedDoorObjects)) ? dungeonPlaceable : GameManager.Instance.Dungeon.doorObjects);
				intVector = GetSubDoorPositionForExit(upstreamExit, upstreamRoom);
				if (component3.SupportsSubsidiaryDoors)
				{
					GenerateSubsidiaryDoor(dungeonData, sourcePlaceable, component3, doorParentTransform);
				}
				else
				{
					DungeonData.Direction subsidiaryDoorDirection = GetSubsidiaryDoorDirection();
					bool northSouth = subsidiaryDoorDirection == DungeonData.Direction.NORTH || subsidiaryDoorDirection == DungeonData.Direction.SOUTH;
					dungeonData.FakeRegisterDoorFeet(intVector, northSouth);
				}
			}
			dungeonData.RegisterDoor(doorPositionForExit, component3, intVector);
			linkedDoor = component3;
		}

		protected bool RequiresSubDoor()
		{
			if (upstreamExit != null && downstreamExit != null && (upstreamExit.jointedExit || upstreamExit.TotalExitLength + downstreamExit.TotalExitLength > 7))
			{
				return true;
			}
			return false;
		}

		protected IntVector2 GetSubDoorPositionForExit(RuntimeRoomExitData exit, RoomHandler owner)
		{
			if (exit.jointedExit)
			{
				if (!exit.oneWayDoor && (exit.linkedExit == null || exit.referencedExit.exitDirection == DungeonData.Direction.EAST || exit.referencedExit.exitDirection == DungeonData.Direction.WEST))
				{
					IntVector2 intVector = exit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exit.referencedExit.exitDirection);
					return intVector + owner.area.basePosition;
				}
				IntVector2 intVector2 = exit.linkedExit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exit.linkedExit.referencedExit.exitDirection);
				return intVector2 + owner.connectedRoomsByExit[exit.referencedExit].area.basePosition;
			}
			if (exit.linkedExit != null && exit.TotalExitLength + exit.linkedExit.TotalExitLength > 7)
			{
				IntVector2 intVector3 = exit.linkedExit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exit.linkedExit.referencedExit.exitDirection);
				return intVector3 + owner.connectedRoomsByExit[exit.referencedExit].area.basePosition;
			}
			return IntVector2.MaxValue;
		}

		protected IntVector2 GetDoorPositionForExit(RuntimeRoomExitData exit, RoomHandler owner, bool overrideSecretRoomHandling = false)
		{
			if (exit == null)
			{
				Debug.LogError("THIS EXIT ISN'T REAL. IT ISNT REAAAALLLLLLL: " + owner.GetRoomName());
			}
			RoomHandler roomHandler = owner.connectedRoomsByExit[exit.referencedExit];
			if (!overrideSecretRoomHandling)
			{
				if (owner != null && !owner.area.IsProceduralRoom && owner.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET && exit.linkedExit != null && exit.linkedExit.referencedExit != null)
				{
					IntVector2 intVector = exit.linkedExit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exit.linkedExit.referencedExit.exitDirection);
					return intVector + owner.connectedRoomsByExit[exit.referencedExit].area.basePosition;
				}
				if (roomHandler != null && !roomHandler.area.IsProceduralRoom && roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET && exit.linkedExit != null && exit.linkedExit.referencedExit != null)
				{
					IntVector2 intVector2 = exit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exit.referencedExit.exitDirection);
					return intVector2 + owner.area.basePosition;
				}
			}
			if (!exit.oneWayDoor && exit.jointedExit && exit.linkedExit != null && (exit.referencedExit.exitDirection == DungeonData.Direction.EAST || exit.referencedExit.exitDirection == DungeonData.Direction.WEST))
			{
				IntVector2 intVector3 = exit.linkedExit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exit.linkedExit.referencedExit.exitDirection);
				return intVector3 + owner.connectedRoomsByExit[exit.referencedExit].area.basePosition;
			}
			IntVector2 intVector4 = exit.referencedExit.GetExitAttachPoint() - IntVector2.One + DungeonData.GetIntVector2FromDirection(exit.referencedExit.exitDirection);
			return intVector4 + owner.area.basePosition;
		}
	}
}
