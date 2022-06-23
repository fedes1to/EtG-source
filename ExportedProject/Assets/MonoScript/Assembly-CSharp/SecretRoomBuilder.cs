using System.Collections.Generic;
using Dungeonator;
using tk2dRuntime.TileMap;
using UnityEngine;

public class SecretRoomBuilder
{
	private const float CEILING_HEIGHT_OFFSET = -3.01f;

	private const float BORDER_HEIGHT_OFFSET = -3.02f;

	private static HashSet<IntVector2> GetRoomCeilingCells(RoomHandler room)
	{
		List<IntVector2> cellRepresentationIncFacewalls = room.area.prototypeRoom.GetCellRepresentationIncFacewalls();
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		List<IntVector2> list = new List<IntVector2>(IntVector2.CardinalsAndOrdinals);
		foreach (IntVector2 item in cellRepresentationIncFacewalls)
		{
			hashSet.Add(room.area.basePosition + item);
			foreach (IntVector2 item2 in list)
			{
				hashSet.Add(room.area.basePosition + item + item2);
			}
		}
		list.Add(IntVector2.Up * 2);
		list.Add(IntVector2.Up * 3);
		list.Add(IntVector2.Up * 2 + IntVector2.Right);
		list.Add(IntVector2.Up * 3 + IntVector2.Right);
		list.Add(IntVector2.Up * 2 + IntVector2.Left);
		list.Add(IntVector2.Up * 3 + IntVector2.Left);
		foreach (PrototypeRoomExit instanceUsedExit in room.area.instanceUsedExits)
		{
			RuntimeExitDefinition runtimeExitDefinition = room.exitDefinitionsByExit[room.area.exitToLocalDataMap[instanceUsedExit]];
			if (room.area.exitToLocalDataMap[instanceUsedExit].oneWayDoor)
			{
				continue;
			}
			DungeonData.Direction direction = ((runtimeExitDefinition.upstreamRoom != room) ? runtimeExitDefinition.downstreamExit.referencedExit.exitDirection : runtimeExitDefinition.upstreamExit.referencedExit.exitDirection);
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(direction);
			HashSet<IntVector2> cellsForRoom = runtimeExitDefinition.GetCellsForRoom(room);
			bool flag = !Dungeon.IsGenerating && runtimeExitDefinition.upstreamRoom.area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.SECRET && runtimeExitDefinition.downstreamRoom.area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.SECRET;
			if (flag)
			{
				int num = int.MaxValue;
				foreach (IntVector2 item3 in cellsForRoom)
				{
					num = Mathf.Min(num, item3.y);
				}
				foreach (IntVector2 item4 in runtimeExitDefinition.GetCellsForOtherRoom(room))
				{
					if (num - item4.y <= 4)
					{
						cellsForRoom.Add(item4);
					}
				}
			}
			foreach (IntVector2 item5 in cellsForRoom)
			{
				hashSet.Add(item5);
				foreach (IntVector2 item6 in list)
				{
					if ((item6.x != 0 && item6.x == intVector2FromDirection.x) || (item6.y != 0 && item6.y == intVector2FromDirection.y))
					{
						if (flag)
						{
							if (room == runtimeExitDefinition.upstreamRoom)
							{
								BraveUtility.DrawDebugSquare(item5.ToVector2() + item6.ToVector2(), Color.yellow, 1000f);
							}
							else if (room == runtimeExitDefinition.downstreamRoom)
							{
								BraveUtility.DrawDebugSquare(item5.ToVector2() + item6.ToVector2() + new Vector2(0.1f, 0.1f), item5.ToVector2() + item6.ToVector2() + new Vector2(0.9f, 0.9f), Color.cyan, 1000f);
							}
						}
					}
					else
					{
						hashSet.Add(item5 + item6);
					}
				}
			}
			if (direction == DungeonData.Direction.SOUTH)
			{
				continue;
			}
			RoomHandler r = ((runtimeExitDefinition.upstreamRoom != room) ? runtimeExitDefinition.upstreamRoom : runtimeExitDefinition.downstreamRoom);
			foreach (IntVector2 item7 in runtimeExitDefinition.GetCellsForRoom(r))
			{
				hashSet.Add(item7);
				foreach (IntVector2 item8 in list)
				{
					if ((item8.x == 0 || item8.x != intVector2FromDirection.x) && (item8.y == 0 || Mathf.Sign(item8.y) != (float)intVector2FromDirection.y))
					{
						hashSet.Add(item7 + item8);
					}
				}
			}
		}
		return hashSet;
	}

	private static bool IsFaceWallHigher(int x, int y, DungeonData data, HashSet<IntVector2> cells)
	{
		if (cells.Contains(new IntVector2(x, y)))
		{
			return false;
		}
		if ((data.cellData[x][y].type == CellType.WALL || data.cellData[x][y].isSecretRoomCell) && data.cellData[x][y - 2].type != CellType.WALL && !data.cellData[x][y - 2].isSecretRoomCell)
		{
			return true;
		}
		return false;
	}

	private static bool IsTopWall(int x, int y, DungeonData data, HashSet<IntVector2> cells)
	{
		if (data.cellData[x][y].type != CellType.WALL && (data.cellData[x][y - 1].type == CellType.WALL || cells.Contains(new IntVector2(x, y - 1))) && !cells.Contains(new IntVector2(x, y + 1)))
		{
			return true;
		}
		return false;
	}

	private static bool IsWall(int x, int y, DungeonData data, HashSet<IntVector2> cells)
	{
		if (cells.Contains(new IntVector2(x, y)))
		{
			return true;
		}
		if (data[x, y].type == CellType.WALL)
		{
			return true;
		}
		return false;
	}

	private static bool IsTopWallOrSecret(int x, int y, DungeonData data, HashSet<IntVector2> cells)
	{
		return data[x, y].type != CellType.WALL && !data[x, y].isSecretRoomCell && IsWallOrSecret(x, y - 1, data, cells);
	}

	private static bool IsWallOrSecret(int x, int y, DungeonData data, HashSet<IntVector2> cells)
	{
		return data[x, y].type == CellType.WALL || data[x, y].isSecretRoomCell || cells.Contains(new IntVector2(x, y));
	}

	private static bool IsFaceWallHigherOrSecret(int x, int y, DungeonData data, HashSet<IntVector2> cells)
	{
		return IsFaceWallHigher(x, y, data, cells);
	}

	public static int GetIndexFromTupleArray(CellData current, List<Tuple<int, TilesetIndexMetadata>> list, int roomTypeIndex)
	{
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			Tuple<int, TilesetIndexMetadata> tuple = list[i];
			if (!tuple.Second.usesAnimSequence && (tuple.Second.dungeonRoomSubType == roomTypeIndex || tuple.Second.secondRoomSubType == roomTypeIndex || tuple.Second.thirdRoomSubType == roomTypeIndex))
			{
				num += tuple.Second.weight;
			}
		}
		float num2 = current.UniqueHash * num;
		for (int j = 0; j < list.Count; j++)
		{
			Tuple<int, TilesetIndexMetadata> tuple2 = list[j];
			if (!tuple2.Second.usesAnimSequence && (tuple2.Second.dungeonRoomSubType == roomTypeIndex || tuple2.Second.secondRoomSubType == roomTypeIndex || tuple2.Second.thirdRoomSubType == roomTypeIndex))
			{
				num2 -= tuple2.Second.weight;
				if (num2 <= 0f)
				{
					return tuple2.First;
				}
			}
		}
		return list[0].First;
	}

	private static TileIndexGrid GetBorderGridForCellPosition(IntVector2 position, DungeonData data)
	{
		TileIndexGrid roomCeilingBorderGrid = GameManager.Instance.Dungeon.roomMaterialDefinitions[data.cellData[position.x][position.y].cellVisualData.roomVisualTypeIndex].roomCeilingBorderGrid;
		if (roomCeilingBorderGrid == null)
		{
			roomCeilingBorderGrid = GameManager.Instance.Dungeon.roomMaterialDefinitions[0].roomCeilingBorderGrid;
		}
		return roomCeilingBorderGrid;
	}

	private static void AddCeilingTileAtPosition(IntVector2 position, TileIndexGrid indexGrid, List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Color> colors, out Material ceilingMaterial, tk2dSpriteCollectionData spriteData)
	{
		int indexByWeight = indexGrid.centerIndices.GetIndexByWeight();
		int tileFromRawTile = BuilderUtil.GetTileFromRawTile(indexByWeight);
		tk2dSpriteDefinition tk2dSpriteDefinition2 = spriteData.spriteDefinitions[tileFromRawTile];
		ceilingMaterial = tk2dSpriteDefinition2.material;
		int count = verts.Count;
		Vector3 vector = position.ToVector3((float)position.y - 2.4f);
		Vector3[] array = tk2dSpriteDefinition2.ConstructExpensivePositions();
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector2 = array[i].WithZ(array[i].y);
			verts.Add(vector + vector2);
			uvs.Add(tk2dSpriteDefinition2.uvs[i]);
			colors.Add(Color.black);
		}
		for (int j = 0; j < tk2dSpriteDefinition2.indices.Length; j++)
		{
			tris.Add(count + tk2dSpriteDefinition2.indices[j]);
		}
	}

	private static void AddTileAtPosition(IntVector2 position, int index, List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Color> colors, out Material targetMaterial, tk2dSpriteCollectionData spriteData, float zOffset, bool tilted, Color topColor, Color bottomColor)
	{
		int tileFromRawTile = BuilderUtil.GetTileFromRawTile(index);
		tk2dSpriteDefinition tk2dSpriteDefinition2 = spriteData.spriteDefinitions[tileFromRawTile];
		targetMaterial = tk2dSpriteDefinition2.material;
		int count = verts.Count;
		Vector3 vector = position.ToVector3((float)position.y + zOffset);
		Vector3[] array = tk2dSpriteDefinition2.ConstructExpensivePositions();
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector2 = ((!tilted) ? array[i].WithZ(array[i].y) : array[i].WithZ(0f - array[i].y));
			verts.Add(vector + vector2);
			uvs.Add(tk2dSpriteDefinition2.uvs[i]);
		}
		colors.Add(bottomColor);
		colors.Add(bottomColor);
		colors.Add(topColor);
		colors.Add(topColor);
		for (int j = 0; j < tk2dSpriteDefinition2.indices.Length; j++)
		{
			tris.Add(count + tk2dSpriteDefinition2.indices[j]);
		}
	}

	private static void AddTileAtPosition(IntVector2 position, int index, List<Vector3> verts, List<int> tris, List<Vector2> uvs, List<Color> colors, ref Material targetMaterial, tk2dSpriteCollectionData spriteData, float zOffset, bool tilted = false)
	{
		int tileFromRawTile = BuilderUtil.GetTileFromRawTile(index);
		if (tileFromRawTile < 0 || tileFromRawTile >= spriteData.spriteDefinitions.Length)
		{
			Debug.Log(tileFromRawTile + " index is out of bounds in SecretRoomBuilder, of indices: " + spriteData.spriteDefinitions.Length);
			return;
		}
		tk2dSpriteDefinition tk2dSpriteDefinition2 = spriteData.spriteDefinitions[tileFromRawTile];
		targetMaterial = tk2dSpriteDefinition2.material;
		int count = verts.Count;
		Vector3 vector = position.ToVector3((float)position.y + zOffset);
		Vector3[] array = tk2dSpriteDefinition2.ConstructExpensivePositions();
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector2 = ((!tilted) ? array[i].WithZ(array[i].y) : array[i].WithZ(0f - array[i].y));
			verts.Add(vector + vector2);
			uvs.Add(tk2dSpriteDefinition2.uvs[i]);
			colors.Add(Color.black);
		}
		for (int j = 0; j < tk2dSpriteDefinition2.indices.Length; j++)
		{
			tris.Add(count + tk2dSpriteDefinition2.indices[j]);
		}
	}

	private static GameObject GenerateRoomDoorMesh(RuntimeExitDefinition exit, RoomHandler room, DungeonData dungeonData)
	{
		DungeonData.Direction directionFromRoom = exit.GetDirectionFromRoom(room);
		IntVector2 intVector = ((exit.upstreamRoom != room) ? exit.GetUpstreamBasePosition() : exit.GetDownstreamBasePosition());
		DungeonData.Direction exitDirection = directionFromRoom;
		IntVector2 exitBasePosition = intVector;
		return GenerateWallMesh(exitDirection, exitBasePosition, "secret room door object", dungeonData);
	}

	public static GameObject GenerateWallMesh(DungeonData.Direction exitDirection, IntVector2 exitBasePosition, string objectName = "secret room door object", DungeonData dungeonData = null, bool abridged = false)
	{
		if (dungeonData == null)
		{
			dungeonData = GameManager.Instance.Dungeon.data;
		}
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		List<int> list5 = new List<int>();
		List<Vector2> list6 = new List<Vector2>();
		List<Color> list7 = new List<Color>();
		Material ceilingMaterial = null;
		Material targetMaterial = null;
		Material targetMaterial2 = null;
		Material targetMaterial3 = null;
		tk2dSpriteCollectionData dungeonCollection = GameManager.Instance.Dungeon.tileIndices.dungeonCollection;
		TileIndexGrid borderGridForCellPosition = GetBorderGridForCellPosition(exitBasePosition, dungeonData);
		CellData cellData = dungeonData[exitBasePosition];
		int num = -1;
		switch (exitDirection)
		{
		case DungeonData.Direction.NORTH:
			AddCeilingTileAtPosition(exitBasePosition, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			AddCeilingTileAtPosition(exitBasePosition + IntVector2.Right, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			AddTileAtPosition(exitBasePosition, borderGridForCellPosition.leftCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			AddTileAtPosition(exitBasePosition + IntVector2.Right, borderGridForCellPosition.rightCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition + IntVector2.Down, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, -0.4f, true, new Color(0f, 1f, 1f), new Color(0f, 0.5f, 1f));
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition + IntVector2.Down + IntVector2.Right, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, -0.4f, true, new Color(0f, 1f, 1f), new Color(0f, 0.5f, 1f));
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition + IntVector2.Down * 2, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, 1.6f, true, new Color(0f, 0.5f, 1f), new Color(0f, 0f, 1f));
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition + IntVector2.Down * 2 + IntVector2.Right, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, 1.6f, true, new Color(0f, 0.5f, 1f), new Color(0f, 0f, 1f));
			break;
		case DungeonData.Direction.EAST:
		{
			if (!abridged)
			{
				AddCeilingTileAtPosition(exitBasePosition + IntVector2.Down, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			}
			if (!abridged)
			{
				AddCeilingTileAtPosition(exitBasePosition + IntVector2.Zero, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			}
			AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up * 2, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			if (!abridged)
			{
				AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up * 3, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			}
			AddTileAtPosition(exitBasePosition + IntVector2.Up, borderGridForCellPosition.bottomCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			AddTileAtPosition(exitBasePosition + IntVector2.Up * 2, borderGridForCellPosition.verticalIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			if (!abridged)
			{
				AddTileAtPosition(exitBasePosition + IntVector2.Up * 3, borderGridForCellPosition.topCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			}
			Color color3 = new Color(0f, 0f, 1f, 0f);
			AddTileAtPosition(exitBasePosition + IntVector2.Down + IntVector2.Right, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallLeft, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color3, color3);
			AddTileAtPosition(exitBasePosition + IntVector2.Right, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallLeft, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color3, color3);
			if (!abridged)
			{
				AddTileAtPosition(exitBasePosition + IntVector2.Up + IntVector2.Right, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallLeft, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color3, color3);
			}
			break;
		}
		case DungeonData.Direction.SOUTH:
		{
			AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up * 2, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up * 2 + IntVector2.Right, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			AddTileAtPosition(exitBasePosition + IntVector2.Up * 2, borderGridForCellPosition.leftCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			AddTileAtPosition(exitBasePosition + IntVector2.Up * 2 + IntVector2.Right, borderGridForCellPosition.rightCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition + IntVector2.Up, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, -0.4f, true, new Color(0f, 1f, 1f), new Color(0f, 0.5f, 1f));
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition + IntVector2.Up + IntVector2.Right, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, -0.4f, true, new Color(0f, 1f, 1f), new Color(0f, 0.5f, 1f));
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, 1.6f, true, new Color(0f, 0.5f, 1f), new Color(0f, 0f, 1f));
			num = GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER], cellData.cellVisualData.roomVisualTypeIndex);
			AddTileAtPosition(exitBasePosition + IntVector2.Right, num, list, list4, list6, list7, out targetMaterial2, dungeonCollection, 1.6f, true, new Color(0f, 0.5f, 1f), new Color(0f, 0f, 1f));
			Color color2 = new Color(0f, 0f, 1f, 0f);
			AddTileAtPosition(exitBasePosition, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOBottomWallBaseTileIndex, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color2, color2);
			AddTileAtPosition(exitBasePosition + IntVector2.Right, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOBottomWallBaseTileIndex, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color2, color2);
			AddTileAtPosition(exitBasePosition + IntVector2.Down, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorTileIndex, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, false, color2, color2);
			AddTileAtPosition(exitBasePosition + IntVector2.Down + IntVector2.Right, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorTileIndex, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, false, color2, color2);
			break;
		}
		case DungeonData.Direction.WEST:
		{
			if (!abridged)
			{
				AddCeilingTileAtPosition(exitBasePosition + IntVector2.Down, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			}
			if (!abridged)
			{
				AddCeilingTileAtPosition(exitBasePosition + IntVector2.Zero, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			}
			AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up * 2, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			if (!abridged)
			{
				AddCeilingTileAtPosition(exitBasePosition + IntVector2.Up * 3, borderGridForCellPosition, list, list2, list6, list7, out ceilingMaterial, dungeonCollection);
			}
			AddTileAtPosition(exitBasePosition + IntVector2.Up, borderGridForCellPosition.bottomCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			AddTileAtPosition(exitBasePosition + IntVector2.Up * 2, borderGridForCellPosition.verticalIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			if (!abridged)
			{
				AddTileAtPosition(exitBasePosition + IntVector2.Up * 3, borderGridForCellPosition.topCapIndices.GetIndexByWeight(), list, list3, list6, list7, ref targetMaterial, dungeonCollection, -2.45f);
			}
			Color color = new Color(0f, 0f, 1f, 0f);
			AddTileAtPosition(exitBasePosition + IntVector2.Down + IntVector2.Left, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallRight, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color, color);
			AddTileAtPosition(exitBasePosition + IntVector2.Left, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallRight, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color, color);
			if (!abridged)
			{
				AddTileAtPosition(exitBasePosition + IntVector2.Up + IntVector2.Left, GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallRight, list, list5, list6, list7, out targetMaterial3, dungeonCollection, 1.55f, true, color, color);
			}
			break;
		}
		}
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		for (int i = 0; i < list.Count; i++)
		{
			vector = Vector3.Min(vector, list[i]);
		}
		vector.x = Mathf.FloorToInt(vector.x);
		vector.y = Mathf.FloorToInt(vector.y);
		vector.z = Mathf.FloorToInt(vector.z);
		for (int j = 0; j < list.Count; j++)
		{
			list[j] -= vector;
		}
		mesh.vertices = list.ToArray();
		mesh.uv = list6.ToArray();
		mesh.colors = list7.ToArray();
		mesh.subMeshCount = 4;
		mesh.SetTriangles(list2.ToArray(), 0);
		mesh.SetTriangles(list3.ToArray(), 1);
		mesh.SetTriangles(list4.ToArray(), 2);
		mesh.SetTriangles(list5.ToArray(), 3);
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		GameObject gameObject = new GameObject(objectName);
		gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Critical"));
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		gameObject.transform.position = vector;
		meshFilter.mesh = mesh;
		meshRenderer.materials = new Material[4] { ceilingMaterial, targetMaterial, targetMaterial2, targetMaterial3 };
		gameObject.SetLayerRecursively(LayerMask.NameToLayer("ShadowCaster"));
		return gameObject;
	}

	public static GameObject GenerateRoomCeilingMesh(HashSet<IntVector2> cells, string objectName = "secret room ceiling object", DungeonData dungeonData = null, bool mimicCheck = false)
	{
		if (dungeonData == null)
		{
			dungeonData = GameManager.Instance.Dungeon.data;
		}
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<Vector2> list4 = new List<Vector2>();
		Material material = null;
		Material material2 = null;
		tk2dSpriteCollectionData dungeonCollection = GameManager.Instance.Dungeon.tileIndices.dungeonCollection;
		Vector3 vector = new Vector3(0f, 0f, -3.01f);
		Vector3 vector2 = new Vector3(0f, 0f, -3.02f);
		foreach (IntVector2 cell in cells)
		{
			TileIndexGrid borderGridForCellPosition = GetBorderGridForCellPosition(cell, dungeonData);
			int indexByWeight = borderGridForCellPosition.centerIndices.GetIndexByWeight();
			int tileFromRawTile = BuilderUtil.GetTileFromRawTile(indexByWeight);
			tk2dSpriteDefinition tk2dSpriteDefinition2 = dungeonCollection.spriteDefinitions[tileFromRawTile];
			if (material == null)
			{
				material = tk2dSpriteDefinition2.material;
			}
			int count = list.Count;
			Vector3 vector3 = cell.ToVector3(cell.y);
			Vector3[] array = tk2dSpriteDefinition2.ConstructExpensivePositions();
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector4 = array[i].WithZ(array[i].y);
				list.Add(vector3 + vector4 + vector);
				list4.Add(tk2dSpriteDefinition2.uvs[i]);
			}
			for (int j = 0; j < tk2dSpriteDefinition2.indices.Length; j++)
			{
				list2.Add(count + tk2dSpriteDefinition2.indices[j]);
			}
			int x = cell.x;
			int y = cell.y;
			bool flag = IsTopWall(x, y, dungeonData, cells);
			bool flag2 = IsTopWall(x + 1, y, dungeonData, cells) && !IsTopWall(x, y, dungeonData, cells) && (IsWall(x, y + 1, dungeonData, cells) || IsTopWall(x, y + 1, dungeonData, cells));
			bool flag3 = (!IsWall(x + 1, y, dungeonData, cells) && !IsTopWall(x + 1, y, dungeonData, cells)) || IsFaceWallHigher(x + 1, y, dungeonData, cells);
			bool flag4 = y > 3 && IsFaceWallHigher(x + 1, y - 1, dungeonData, cells) && !IsFaceWallHigher(x, y - 1, dungeonData, cells);
			bool flag5 = y > 3 && IsFaceWallHigher(x, y - 1, dungeonData, cells);
			bool flag6 = y > 3 && IsFaceWallHigher(x - 1, y - 1, dungeonData, cells) && !IsFaceWallHigher(x, y - 1, dungeonData, cells);
			bool flag7 = (!IsWall(x - 1, y, dungeonData, cells) && !IsTopWall(x - 1, y, dungeonData, cells)) || IsFaceWallHigher(x - 1, y, dungeonData, cells);
			bool flag8 = IsTopWall(x - 1, y, dungeonData, cells) && !IsTopWall(x, y, dungeonData, cells) && (IsWall(x, y + 1, dungeonData, cells) || IsTopWall(x, y + 1, dungeonData, cells));
			if (mimicCheck)
			{
				flag = IsTopWallOrSecret(x, y, dungeonData, cells);
				flag2 = IsTopWallOrSecret(x + 1, y, dungeonData, cells) && !IsTopWallOrSecret(x, y, dungeonData, cells) && (IsWallOrSecret(x, y + 1, dungeonData, cells) || IsTopWallOrSecret(x, y + 1, dungeonData, cells));
				flag3 = (!IsWallOrSecret(x + 1, y, dungeonData, cells) && !IsTopWallOrSecret(x + 1, y, dungeonData, cells)) || IsFaceWallHigherOrSecret(x + 1, y, dungeonData, cells);
				flag4 = y > 3 && IsFaceWallHigherOrSecret(x + 1, y - 1, dungeonData, cells) && !IsFaceWallHigherOrSecret(x, y - 1, dungeonData, cells);
				flag5 = y > 3 && IsFaceWallHigherOrSecret(x, y - 1, dungeonData, cells);
				flag6 = y > 3 && IsFaceWallHigherOrSecret(x - 1, y - 1, dungeonData, cells) && !IsFaceWallHigherOrSecret(x, y - 1, dungeonData, cells);
				flag7 = (!IsWallOrSecret(x - 1, y, dungeonData, cells) && !IsTopWallOrSecret(x - 1, y, dungeonData, cells)) || IsFaceWallHigherOrSecret(x - 1, y, dungeonData, cells);
				flag8 = IsTopWallOrSecret(x - 1, y, dungeonData, cells) && !IsTopWallOrSecret(x, y, dungeonData, cells) && (IsWallOrSecret(x, y + 1, dungeonData, cells) || IsTopWallOrSecret(x, y + 1, dungeonData, cells));
			}
			if (!flag && !flag2 && !flag3 && !flag4 && !flag5 && !flag6 && !flag7 && !flag8)
			{
				continue;
			}
			int rawTile = borderGridForCellPosition.GetIndexGivenSides(flag, flag2, flag3, flag4, flag5, flag6, flag7, flag8);
			if (borderGridForCellPosition.UsesRatChunkBorders)
			{
				bool flag9 = y > 3;
				if (flag9)
				{
					flag9 = !dungeonData[x, y - 1].HasFloorNeighbor(dungeonData, false, true);
				}
				TileIndexGrid.RatChunkResult result = TileIndexGrid.RatChunkResult.NONE;
				rawTile = borderGridForCellPosition.GetRatChunkIndexGivenSides(flag, flag2, flag3, flag4, flag5, flag6, flag7, flag8, flag9, out result);
			}
			int tileFromRawTile2 = BuilderUtil.GetTileFromRawTile(rawTile);
			tk2dSpriteDefinition tk2dSpriteDefinition3 = dungeonCollection.spriteDefinitions[tileFromRawTile2];
			if (material2 == null)
			{
				material2 = tk2dSpriteDefinition3.material;
			}
			int count2 = list.Count;
			Vector3 vector5 = cell.ToVector3(cell.y);
			Vector3[] array2 = tk2dSpriteDefinition3.ConstructExpensivePositions();
			for (int k = 0; k < array2.Length; k++)
			{
				Vector3 vector6 = array2[k].WithZ(array2[k].y);
				list.Add(vector5 + vector6 + vector2);
				list4.Add(tk2dSpriteDefinition3.uvs[k]);
			}
			for (int l = 0; l < tk2dSpriteDefinition3.indices.Length; l++)
			{
				list3.Add(count2 + tk2dSpriteDefinition3.indices[l]);
			}
		}
		Vector3 vector7 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		for (int m = 0; m < list.Count; m++)
		{
			vector7 = Vector3.Min(vector7, list[m]);
		}
		for (int n = 0; n < list.Count; n++)
		{
			list[n] -= vector7;
		}
		mesh.vertices = list.ToArray();
		mesh.uv = list4.ToArray();
		mesh.subMeshCount = 2;
		mesh.SetTriangles(list2.ToArray(), 0);
		mesh.SetTriangles(list3.ToArray(), 1);
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		GameObject gameObject = new GameObject(objectName);
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		gameObject.transform.position = vector7;
		meshFilter.mesh = mesh;
		meshRenderer.materials = new Material[2] { material, material2 };
		return gameObject;
	}

	private static HashSet<IntVector2> CorrectForDoubledSecretRoomness(RoomHandler room, DungeonData data)
	{
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		if (room.area.instanceUsedExits.Count == 1)
		{
			RuntimeExitDefinition runtimeExitDefinition = room.exitDefinitionsByExit[room.area.exitToLocalDataMap[room.area.instanceUsedExits[0]]];
			if (runtimeExitDefinition.downstreamRoom == room && runtimeExitDefinition.upstreamRoom.area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.SECRET)
			{
				List<IntVector2> cellRepresentationIncFacewalls = runtimeExitDefinition.upstreamRoom.area.prototypeRoom.GetCellRepresentationIncFacewalls();
				List<IntVector2> list = new List<IntVector2>(IntVector2.CardinalsAndOrdinals);
				foreach (IntVector2 item in cellRepresentationIncFacewalls)
				{
					hashSet.Add(runtimeExitDefinition.upstreamRoom.area.basePosition + item);
					foreach (IntVector2 item2 in list)
					{
						hashSet.Add(runtimeExitDefinition.upstreamRoom.area.basePosition + item + item2);
					}
				}
			}
		}
		List<IntVector2> list2 = new List<IntVector2>();
		foreach (IntVector2 item3 in hashSet)
		{
			if (data[item3] != null && data[item3].isSecretRoomCell && !data[item3].isExitCell)
			{
				data[item3].isSecretRoomCell = false;
			}
			else
			{
				list2.Add(item3);
			}
		}
		foreach (IntVector2 item4 in list2)
		{
			hashSet.Remove(item4);
		}
		return hashSet;
	}

	public static GameObject BuildRoomCover(RoomHandler room, tk2dTileMap tileMap, DungeonData dungeonData)
	{
		HashSet<IntVector2> hashSet = null;
		if (!Dungeon.IsGenerating)
		{
			hashSet = CorrectForDoubledSecretRoomness(room, dungeonData);
		}
		HashSet<IntVector2> roomCeilingCells = GetRoomCeilingCells(room);
		GameObject gameObject = GenerateRoomCeilingMesh(roomCeilingCells, "secret room ceiling object", dungeonData);
		List<SecretRoomDoorBeer> list = new List<SecretRoomDoorBeer>();
		for (int i = 0; i < room.area.instanceUsedExits.Count; i++)
		{
			PrototypeRoomExit key = room.area.instanceUsedExits[i];
			if (!room.area.exitToLocalDataMap[key].oneWayDoor)
			{
				RuntimeExitDefinition runtimeExitDefinition = room.exitDefinitionsByExit[room.area.exitToLocalDataMap[key]];
				if (Dungeon.IsGenerating || runtimeExitDefinition.downstreamRoom == room || !(runtimeExitDefinition.downstreamRoom.area.prototypeRoom != null) || runtimeExitDefinition.downstreamRoom.area.prototypeRoom.category != PrototypeDungeonRoom.RoomCategory.SECRET)
				{
					GameObject gameObject2 = GenerateRoomDoorMesh(runtimeExitDefinition, room, dungeonData);
					SecretRoomDoorBeer secretRoomDoorBeer = gameObject2.AddComponent<SecretRoomDoorBeer>();
					secretRoomDoorBeer.exitDef = room.exitDefinitionsByExit[room.area.exitToLocalDataMap[key]];
					secretRoomDoorBeer.linkedRoom = room.connectedRoomsByExit[key];
					list.Add(secretRoomDoorBeer);
				}
			}
		}
		GameObject gameObject3 = new GameObject("Secret Room");
		gameObject3.transform.position = gameObject.transform.position;
		gameObject.transform.parent = gameObject3.transform;
		SecretRoomManager secretRoomManager = gameObject3.AddComponent<SecretRoomManager>();
		List<IntVector2> ceilingCellList = new List<IntVector2>(roomCeilingCells);
		secretRoomManager.InitializeCells(ceilingCellList);
		List<SecretRoomExitData> list2 = SecretRoomUtility.BuildRoomExitColliders(room);
		for (int j = 0; j < list2.Count; j++)
		{
			list[j].collider = list2[j];
		}
		secretRoomManager.ceilingRenderer = gameObject.GetComponent<Renderer>();
		for (int k = 0; k < list.Count; k++)
		{
			secretRoomManager.doorObjects.Add(list[k]);
		}
		secretRoomManager.room = room;
		room.secretRoomManager = secretRoomManager;
		string roomName = room.GetRoomName();
		if (!string.IsNullOrEmpty(roomName) && roomName.Contains("SewersEntrance"))
		{
			secretRoomManager.revealStyle = SecretRoomManager.SecretRoomRevealStyle.FireplacePuzzle;
			secretRoomManager.InitializeForStyle();
		}
		else if (SecretRoomUtility.FloorHasComplexPuzzle || GameManager.Instance.Dungeon.SecretRoomComplexTriggers.Count == 0)
		{
			secretRoomManager.InitializeForStyle();
		}
		else
		{
			SecretRoomUtility.FloorHasComplexPuzzle = true;
			secretRoomManager.revealStyle = SecretRoomManager.SecretRoomRevealStyle.ComplexPuzzle;
			secretRoomManager.InitializeForStyle();
		}
		if (hashSet != null && hashSet.Count > 0)
		{
			foreach (IntVector2 item in hashSet)
			{
				dungeonData[item].isSecretRoomCell = true;
			}
		}
		return null;
	}
}
