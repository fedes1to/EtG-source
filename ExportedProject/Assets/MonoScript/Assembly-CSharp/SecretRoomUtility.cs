using System.Collections.Generic;
using Dungeonator;
using tk2dRuntime.TileMap;
using UnityEngine;

public static class SecretRoomUtility
{
	internal class IntVector2WithIndexEqualityComparer : IEqualityComparer<IntVector2WithIndex>
	{
		public bool Equals(IntVector2WithIndex v1, IntVector2WithIndex v2)
		{
			if (v1.position == v2.position)
			{
				return true;
			}
			return false;
		}

		public int GetHashCode(IntVector2WithIndex v1)
		{
			return v1.position.GetHashCode();
		}
	}

	internal class IntVector2WithIndex
	{
		public IntVector2 position;

		public int index;

		public float zOffset;

		public Color[] meshColor;

		public int facewallID;

		public int sidewallID;

		public IntVector2WithIndex(IntVector2 vec, int i)
		{
			position = vec;
			index = i;
			meshColor = new Color[4]
			{
				Color.black,
				Color.black,
				Color.black,
				Color.black
			};
		}

		public IntVector2WithIndex(IntVector2 vec, int i, Color c)
		{
			position = vec;
			index = i;
			meshColor = new Color[4] { c, c, c, c };
		}

		public IntVector2WithIndex(IntVector2 vec, int i, Color bottom, Color top)
		{
			position = vec;
			index = i;
			meshColor = new Color[4] { bottom, bottom, top, top };
		}

		public Vector3 GetOffset()
		{
			return new Vector3(0f, 0f, zOffset);
		}
	}

	public static Dictionary<TilesetIndexMetadata.TilesetFlagType, List<Tuple<int, TilesetIndexMetadata>>> metadataLookupTableRef;

	public static bool FloorHasComplexPuzzle;

	private static bool IsSolid(CellData cell)
	{
		if (cell.type == CellType.WALL || cell.isSecretRoomCell)
		{
			return true;
		}
		return false;
	}

	private static bool IsFaceWallHigher(int x, int y, CellData[][] t)
	{
		if (IsSolid(t[x][y]) && IsSolid(t[x][y - 1]) && !IsSolid(t[x][y - 2]))
		{
			return true;
		}
		return false;
	}

	private static bool IsFaceWallLower(int x, int y, CellData[][] t)
	{
		if (IsSolid(t[x][y]) && !IsSolid(t[x][y - 1]))
		{
			return true;
		}
		return false;
	}

	public static void ClearPerLevelData()
	{
		FloorHasComplexPuzzle = false;
	}

	public static int GetIndexFromTupleArray(CellData current, List<Tuple<int, TilesetIndexMetadata>> list, int roomTypeIndex, float forcedRand = -1f)
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
		if (forcedRand >= 0f)
		{
			num2 = forcedRand * num;
		}
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

	private static bool IsSecretDoorTopBorder(CellData cellToCheck, DungeonData data)
	{
		if (cellToCheck.isSecretRoomCell)
		{
			if (data.cellData[cellToCheck.position.x][cellToCheck.position.y - 1].type == CellType.FLOOR && !data.cellData[cellToCheck.position.x][cellToCheck.position.y - 1].isSecretRoomCell)
			{
				return true;
			}
			if (data.cellData[cellToCheck.position.x][cellToCheck.position.y - 2].type == CellType.FLOOR && !data.cellData[cellToCheck.position.x][cellToCheck.position.y - 2].isSecretRoomCell)
			{
				return true;
			}
		}
		return false;
	}

	private static int GetIndexGivenCell(IntVector2 position, List<IntVector2> cellRepresentation, DungeonData data, out int facewall, out int sidewall)
	{
		facewall = 0;
		sidewall = 0;
		TileIndexGrid borderGridForCellPosition = GetBorderGridForCellPosition(position, data);
		CellData cellData = data.cellData[position.x][position.y];
		List<CellData> cellNeighbors = data.GetCellNeighbors(cellData);
		if (cellNeighbors[1].type == CellType.FLOOR && !cellNeighbors[1].isSecretRoomCell)
		{
			sidewall = 1;
		}
		if (cellNeighbors[3].type == CellType.FLOOR && !cellNeighbors[3].isSecretRoomCell)
		{
			sidewall = -1;
		}
		if (cellNeighbors[2].type == CellType.FLOOR && !cellNeighbors[2].isSecretRoomCell)
		{
			facewall = 1;
			return GetIndexFromTupleArray(cellData, metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER], cellData.cellVisualData.roomVisualTypeIndex);
		}
		if (data.cellData[cellData.position.x][cellData.position.y - 2].type == CellType.FLOOR && !data.cellData[cellData.position.x][cellData.position.y - 2].isSecretRoomCell)
		{
			facewall = 2;
			return GetIndexFromTupleArray(cellData, metadataLookupTableRef[TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER], cellData.cellVisualData.roomVisualTypeIndex);
		}
		bool[] array = new bool[4];
		for (int i = 0; i < 4; i++)
		{
			bool flag = IsFaceWallHigher(cellNeighbors[i].position.x, cellNeighbors[i].position.y, data.cellData) || IsFaceWallLower(cellNeighbors[i].position.x, cellNeighbors[i].position.y, data.cellData);
			bool flag2 = cellNeighbors[i].type != CellType.WALL && !cellNeighbors[i].isSecretRoomCell && IsSolid(data.cellData[cellNeighbors[i].position.x][cellNeighbors[i].position.y - 1]);
			if ((cellNeighbors[i].type != CellType.WALL || flag) && !cellNeighbors[i].isSecretRoomCell && !flag2)
			{
				array[i] = true;
			}
			if (IsSecretDoorTopBorder(cellNeighbors[i], data))
			{
				array[i] = true;
				facewall = 3;
			}
			if (array[i] && ((cellData.type != CellType.WALL && !cellData.IsTopWall()) || cellData.IsAnyFaceWall()))
			{
				facewall = 3;
			}
		}
		return borderGridForCellPosition.GetIndexGivenSides(array[0], array[1], array[2], array[3]);
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

	private static void BuildRoomCoverExitIndices(RoomHandler room, tk2dTileMap tileMap, DungeonData dungeonData, List<IntVector2> cellRepresentation, HashSet<IntVector2WithIndex> ceilingCells, HashSet<IntVector2WithIndex> borderCells, HashSet<IntVector2WithIndex> facewallCells)
	{
		for (int i = 0; i < room.area.instanceUsedExits.Count; i++)
		{
			PrototypeRoomExit prototypeRoomExit = room.area.instanceUsedExits[i];
			RuntimeRoomExitData runtimeRoomExitData = room.area.exitToLocalDataMap[prototypeRoomExit];
			PrototypeRoomExit exitConnectedToRoom = room.connectedRoomsByExit[prototypeRoomExit].GetExitConnectedToRoom(room);
			RuntimeRoomExitData runtimeRoomExitData2 = room.connectedRoomsByExit[prototypeRoomExit].area.exitToLocalDataMap[exitConnectedToRoom];
			int num = runtimeRoomExitData.TotalExitLength + runtimeRoomExitData2.TotalExitLength - 1;
			if (prototypeRoomExit.exitDirection == DungeonData.Direction.NORTH)
			{
				num += 2;
			}
			for (int j = 0; j < prototypeRoomExit.containedCells.Count; j++)
			{
				for (int k = 0; k < num; k++)
				{
					IntVector2 intVector = prototypeRoomExit.containedCells[j].ToIntVector2() + room.area.basePosition - IntVector2.One;
					List<IntVector2> list = new List<IntVector2>();
					if (prototypeRoomExit.exitDirection == DungeonData.Direction.NORTH)
					{
						intVector += IntVector2.Up * k;
						list.Add(intVector + IntVector2.Left);
						list.Add(intVector + IntVector2.Right);
					}
					else if (prototypeRoomExit.exitDirection == DungeonData.Direction.SOUTH)
					{
						intVector += IntVector2.Down * k;
						if (k < num - 2)
						{
							list.Add(intVector + IntVector2.Left);
							list.Add(intVector + IntVector2.Right);
						}
					}
					else if (prototypeRoomExit.exitDirection == DungeonData.Direction.EAST)
					{
						intVector += IntVector2.Right * k;
						list.Add(intVector + IntVector2.Up);
						list.Add(intVector + IntVector2.Up * 2);
						list.Add(intVector + IntVector2.Up * 3);
					}
					else
					{
						intVector += IntVector2.Left * k;
						list.Add(intVector + IntVector2.Up);
						list.Add(intVector + IntVector2.Up * 2);
						list.Add(intVector + IntVector2.Up * 3);
					}
					list.Add(intVector);
					for (int l = 0; l < list.Count; l++)
					{
						int facewall = 0;
						int sidewall = 0;
						int indexGivenCell = GetIndexGivenCell(list[l], cellRepresentation, dungeonData, out facewall, out sidewall);
						TileIndexGrid borderGridForCellPosition = GetBorderGridForCellPosition(list[l], dungeonData);
						if (facewall > 0)
						{
							IntVector2WithIndex intVector2WithIndex = new IntVector2WithIndex(list[l], indexGivenCell);
							intVector2WithIndex.facewallID = facewall;
							intVector2WithIndex.sidewallID = sidewall;
							switch (facewall)
							{
							case 1:
								intVector2WithIndex.meshColor = new Color[4]
								{
									new Color(0f, 0f, 1f),
									new Color(0f, 0f, 1f),
									new Color(0f, 0.5f, 1f),
									new Color(0f, 0.5f, 1f)
								};
								facewallCells.Add(intVector2WithIndex);
								break;
							case 2:
								intVector2WithIndex.meshColor = new Color[4]
								{
									new Color(0f, 0.5f, 1f),
									new Color(0f, 0.5f, 1f),
									new Color(0f, 1f, 1f),
									new Color(0f, 1f, 1f)
								};
								facewallCells.Add(intVector2WithIndex);
								break;
							case 3:
							{
								if (!borderGridForCellPosition.centerIndices.indices.Contains(intVector2WithIndex.index))
								{
									facewallCells.Add(intVector2WithIndex);
								}
								IntVector2WithIndex intVector2WithIndex2 = new IntVector2WithIndex(list[l], borderGridForCellPosition.centerIndices.GetIndexByWeight());
								intVector2WithIndex2.zOffset = 1.5f;
								ceilingCells.Add(intVector2WithIndex2);
								break;
							}
							}
						}
						else if (borderGridForCellPosition.centerIndices.indices.Contains(indexGivenCell))
						{
							IntVector2WithIndex intVector2WithIndex3 = new IntVector2WithIndex(list[l], indexGivenCell);
							intVector2WithIndex3.sidewallID = sidewall;
							ceilingCells.Add(intVector2WithIndex3);
						}
						else
						{
							IntVector2WithIndex intVector2WithIndex4 = new IntVector2WithIndex(list[l], indexGivenCell);
							intVector2WithIndex4.sidewallID = sidewall;
							intVector2WithIndex4.zOffset += 1f;
							borderCells.Add(intVector2WithIndex4);
							IntVector2WithIndex intVector2WithIndex5 = new IntVector2WithIndex(list[l], borderGridForCellPosition.centerIndices.GetIndexByWeight());
							intVector2WithIndex5.sidewallID = sidewall;
							intVector2WithIndex5.zOffset += 0.75f;
							ceilingCells.Add(intVector2WithIndex5);
						}
					}
				}
			}
		}
	}

	private static Mesh BuildAOMesh(HashSet<IntVector2WithIndex> facewallIndices, out Material material)
	{
		material = null;
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector2> list3 = new List<Vector2>();
		List<Color> list4 = new List<Color>();
		tk2dSpriteCollectionData dungeonCollection = GameManager.Instance.Dungeon.tileIndices.dungeonCollection;
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		foreach (IntVector2WithIndex facewallIndex in facewallIndices)
		{
			if (hashSet.Contains(facewallIndex.position))
			{
				continue;
			}
			hashSet.Add(facewallIndex.position);
			int num = -1;
			Vector3 zero = Vector3.zero;
			if (facewallIndex.facewallID == 1)
			{
				num = BuilderUtil.GetTileFromRawTile(GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorTileIndex);
			}
			else if (facewallIndex.sidewallID == 1)
			{
				num = BuilderUtil.GetTileFromRawTile(GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallLeft);
				zero += Vector3.right + Vector3.down + Vector3.forward;
			}
			else if (facewallIndex.sidewallID == -1)
			{
				num = BuilderUtil.GetTileFromRawTile(GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOFloorWallRight);
			}
			if (num == -1)
			{
				continue;
			}
			tk2dSpriteDefinition tk2dSpriteDefinition2 = dungeonCollection.spriteDefinitions[num];
			if (material == null)
			{
				material = tk2dSpriteDefinition2.material;
			}
			int count = list.Count;
			Vector3 vector = facewallIndex.position.ToVector3(facewallIndex.position.y - 1) + zero;
			vector.y -= 1f;
			Vector3[] array = tk2dSpriteDefinition2.ConstructExpensivePositions();
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector2 = array[i];
				vector2 = vector2.WithZ(vector2.y);
				if (facewallIndex.facewallID == 1)
				{
					vector2.z += 2f;
				}
				list.Add(vector + vector2 + facewallIndex.GetOffset());
				list3.Add(tk2dSpriteDefinition2.uvs[i]);
				list4.Add(facewallIndex.meshColor[i % 4]);
			}
			for (int j = 0; j < tk2dSpriteDefinition2.indices.Length; j++)
			{
				list2.Add(count + tk2dSpriteDefinition2.indices[j]);
			}
			if (facewallIndex.facewallID != 1)
			{
				continue;
			}
			int tileFromRawTile = BuilderUtil.GetTileFromRawTile(GameManager.Instance.Dungeon.tileIndices.aoTileIndices.AOBottomWallBaseTileIndex);
			tk2dSpriteDefinition2 = dungeonCollection.spriteDefinitions[tileFromRawTile];
			count = list.Count;
			vector = facewallIndex.position.ToVector3(facewallIndex.position.y);
			array = tk2dSpriteDefinition2.ConstructExpensivePositions();
			for (int k = 0; k < array.Length; k++)
			{
				Vector3 vector3 = array[k];
				vector3 = vector3.WithZ(0f - vector3.y);
				if (facewallIndex.facewallID == 1)
				{
					vector3.z += 2f;
				}
				list.Add(vector + vector3 + facewallIndex.GetOffset());
				list3.Add(tk2dSpriteDefinition2.uvs[k]);
				list4.Add(facewallIndex.meshColor[k % 4]);
			}
			for (int l = 0; l < tk2dSpriteDefinition2.indices.Length; l++)
			{
				list2.Add(count + tk2dSpriteDefinition2.indices[l]);
			}
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.uv = list3.ToArray();
		mesh.colors = list4.ToArray();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		return mesh;
	}

	private static Mesh BuildTargetMesh(HashSet<IntVector2WithIndex> cellIndices, out Material material, bool facewall = false)
	{
		material = null;
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector2> list3 = new List<Vector2>();
		List<Color> list4 = new List<Color>();
		List<Vector3> list5 = new List<Vector3>();
		tk2dSpriteCollectionData dungeonCollection = GameManager.Instance.Dungeon.tileIndices.dungeonCollection;
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		foreach (IntVector2WithIndex cellIndex in cellIndices)
		{
			if (hashSet.Contains(cellIndex.position))
			{
				continue;
			}
			hashSet.Add(cellIndex.position);
			int tileFromRawTile = BuilderUtil.GetTileFromRawTile(cellIndex.index);
			tk2dSpriteDefinition tk2dSpriteDefinition2 = dungeonCollection.spriteDefinitions[tileFromRawTile];
			if (material == null)
			{
				material = tk2dSpriteDefinition2.material;
			}
			int count = list.Count;
			Vector3 vector = cellIndex.position.ToVector3(cellIndex.position.y);
			Vector3[] array = tk2dSpriteDefinition2.ConstructExpensivePositions();
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector2 = array[i];
				if (facewall)
				{
					if (cellIndex.facewallID > 2)
					{
						vector2 = vector2.WithZ(vector2.y);
						vector2.z -= 2.25f;
					}
					else
					{
						vector2 = vector2.WithZ(0f - vector2.y);
						if (cellIndex.facewallID == 1)
						{
							vector2.z += 2f;
						}
					}
				}
				else
				{
					vector2 = vector2.WithZ(vector2.y);
				}
				list.Add(vector + vector2 + cellIndex.GetOffset());
				list5.Add(Vector3.back);
				list3.Add(tk2dSpriteDefinition2.uvs[i]);
				list4.Add(cellIndex.meshColor[i % 4]);
			}
			for (int j = 0; j < tk2dSpriteDefinition2.indices.Length; j++)
			{
				list2.Add(count + tk2dSpriteDefinition2.indices[j]);
			}
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.normals = list5.ToArray();
		mesh.uv = list3.ToArray();
		mesh.colors = list4.ToArray();
		mesh.RecalculateBounds();
		return mesh;
	}

	private static GameObject CreateObjectForMesh(Mesh meshTarget, Material materialTarget, float zHeight, Transform parentObject, bool ao = false)
	{
		GameObject gameObject = new GameObject("Secret Room Mesh");
		gameObject.transform.position = new Vector3(0f, 0f, zHeight);
		gameObject.transform.parent = parentObject;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshFilter.sharedMesh = meshTarget;
		meshRenderer.sharedMaterial = materialTarget;
		DepthLookupManager.ProcessRenderer(meshRenderer, DepthLookupManager.GungeonSortingLayer.FOREGROUND);
		if (!ao)
		{
			gameObject.layer = LayerMask.NameToLayer("ShadowCaster");
		}
		return gameObject;
	}

	public static List<SecretRoomExitData> BuildRoomExitColliders(RoomHandler room)
	{
		List<SecretRoomExitData> list = new List<SecretRoomExitData>();
		if (!room.area.IsProceduralRoom)
		{
			for (int i = 0; i < room.area.instanceUsedExits.Count; i++)
			{
				if (room.area.exitToLocalDataMap[room.area.instanceUsedExits[i]].oneWayDoor)
				{
					continue;
				}
				RuntimeExitDefinition runtimeExitDefinition = room.exitDefinitionsByExit[room.area.exitToLocalDataMap[room.area.instanceUsedExits[i]]];
				if (Dungeon.IsGenerating || runtimeExitDefinition.downstreamRoom == room || !(runtimeExitDefinition.downstreamRoom.area.prototypeRoom != null) || runtimeExitDefinition.downstreamRoom.area.prototypeRoom.category != PrototypeDungeonRoom.RoomCategory.SECRET)
				{
					GameObject gameObject = new GameObject("secret exit collider");
					SpeculativeRigidbody speculativeRigidbody = gameObject.AddComponent<SpeculativeRigidbody>();
					gameObject.AddComponent<PersistentVFXManagerBehaviour>();
					speculativeRigidbody.CollideWithTileMap = false;
					speculativeRigidbody.CollideWithOthers = true;
					speculativeRigidbody.PixelColliders = new List<PixelCollider>();
					PrototypeRoomExit prototypeRoomExit = room.area.instanceUsedExits[i];
					RuntimeRoomExitData runtimeRoomExitData = room.area.exitToLocalDataMap[prototypeRoomExit];
					RoomHandler roomHandler = room.connectedRoomsByExit[prototypeRoomExit];
					PrototypeRoomExit exitConnectedToRoom = roomHandler.GetExitConnectedToRoom(room);
					int num = roomHandler.area.exitToLocalDataMap[exitConnectedToRoom].TotalExitLength - 1;
					IntVector2 intVector = IntVector2.Zero;
					IntVector2 intVector2 = IntVector2.Zero;
					int num2 = 0;
					if (prototypeRoomExit.exitDirection == DungeonData.Direction.NORTH)
					{
						intVector = room.area.basePosition + runtimeRoomExitData.ExitOrigin + IntVector2.Left + num * IntVector2.Up;
						intVector2 = new IntVector2(2, 1);
						num2 = 8;
					}
					else if (prototypeRoomExit.exitDirection == DungeonData.Direction.EAST)
					{
						intVector = room.area.basePosition + runtimeRoomExitData.ExitOrigin + IntVector2.NegOne + num * IntVector2.Right;
						intVector2 = new IntVector2(1, 4);
					}
					else if (prototypeRoomExit.exitDirection == DungeonData.Direction.WEST)
					{
						intVector = room.area.basePosition + runtimeRoomExitData.ExitOrigin + IntVector2.NegOne + num * IntVector2.Left;
						intVector2 = new IntVector2(1, 4);
					}
					else if (prototypeRoomExit.exitDirection == DungeonData.Direction.SOUTH)
					{
						intVector = room.area.basePosition + runtimeRoomExitData.ExitOrigin + IntVector2.NegOne + num * IntVector2.Down;
						PixelCollider pixelCollider = new PixelCollider();
						pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
						pixelCollider.CollisionLayer = CollisionLayer.LowObstacle;
						pixelCollider.ManualOffsetX = 0;
						pixelCollider.ManualOffsetY = -16;
						pixelCollider.ManualWidth = 32;
						pixelCollider.ManualHeight = 16;
						speculativeRigidbody.PixelColliders.Add(pixelCollider);
						intVector += IntVector2.Up;
						intVector2 = new IntVector2(2, 1);
					}
					gameObject.transform.position = intVector.ToVector3();
					PixelCollider pixelCollider2 = new PixelCollider();
					pixelCollider2.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
					pixelCollider2.CollisionLayer = CollisionLayer.HighObstacle;
					pixelCollider2.ManualWidth = 16 * intVector2.x;
					pixelCollider2.ManualHeight = 16 * intVector2.y + num2;
					speculativeRigidbody.PixelColliders.Add(pixelCollider2);
					speculativeRigidbody.ForceRegenerate();
					list.Add(new SecretRoomExitData(gameObject, prototypeRoomExit.exitDirection));
				}
			}
		}
		else
		{
			Debug.LogError("no support for secret procedural rooms yet.");
		}
		return list;
	}
}
