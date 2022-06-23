using System;
using System.Collections;
using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;

namespace Dungeonator
{
	public class DungeonData
	{
		public enum Direction
		{
			NORTH,
			NORTHEAST,
			EAST,
			SOUTHEAST,
			SOUTH,
			SOUTHWEST,
			WEST,
			NORTHWEST
		}

		public enum LightGenerationStyle
		{
			STANDARD,
			FORCE_COLOR,
			RAT_HALLWAY
		}

		public CellData[][] cellData;

		private int m_width = -1;

		private int m_height = -1;

		public List<RoomHandler> rooms;

		public Dictionary<IntVector2, DungeonDoorController> doors;

		public RoomHandler Entrance;

		public RoomHandler Exit;

		public tk2dTileMap tilemap;

		private static List<CellData> s_neighborsList = new List<CellData>(8);

		private ParticleSystem m_sizzleSystem;

		public int Width
		{
			get
			{
				if (m_width == -1)
				{
					m_width = cellData.Length;
				}
				return m_width;
			}
		}

		public int Height
		{
			get
			{
				if (m_height == -1)
				{
					m_height = cellData[0].Length;
				}
				return m_height;
			}
		}

		public CellData this[IntVector2 key]
		{
			get
			{
				return cellData[key.x][key.y];
			}
			set
			{
				cellData[key.x][key.y] = value;
			}
		}

		public CellData this[int x, int y]
		{
			get
			{
				return cellData[x][y];
			}
			set
			{
				cellData[x][y] = value;
			}
		}

		public DungeonData(CellData[][] data)
		{
			cellData = data;
		}

		public void ClearCachedCellData()
		{
			m_width = -1;
			m_height = -1;
		}

		public static Direction InvertDirection(Direction inDir)
		{
			switch (inDir)
			{
			case Direction.NORTH:
				return Direction.SOUTH;
			case Direction.NORTHEAST:
				return Direction.SOUTHWEST;
			case Direction.EAST:
				return Direction.WEST;
			case Direction.SOUTHEAST:
				return Direction.NORTHWEST;
			case Direction.SOUTH:
				return Direction.NORTH;
			case Direction.SOUTHWEST:
				return Direction.NORTHEAST;
			case Direction.WEST:
				return Direction.EAST;
			case Direction.NORTHWEST:
				return Direction.SOUTHEAST;
			default:
				return inDir;
			}
		}

		public static Direction GetRandomCardinalDirection()
		{
			float value = UnityEngine.Random.value;
			if (value < 0.25f)
			{
				return Direction.NORTH;
			}
			if (value < 0.5f)
			{
				return Direction.EAST;
			}
			if (value < 0.75f)
			{
				return Direction.SOUTH;
			}
			return Direction.WEST;
		}

		public static Direction GetCardinalFromVector2(Vector2 vec)
		{
			return GetDirectionFromVector2(BraveUtility.GetMajorAxis(vec));
		}

		public static Direction GetDirectionFromInts(int x, int y)
		{
			if (x == 0)
			{
				if (y > 0)
				{
					return Direction.NORTH;
				}
				if (y < 0)
				{
					return Direction.SOUTH;
				}
				return (Direction)(-1);
			}
			if (x < 0)
			{
				if (y > 0)
				{
					return Direction.NORTHWEST;
				}
				if (y < 0)
				{
					return Direction.SOUTHWEST;
				}
				return Direction.WEST;
			}
			if (y > 0)
			{
				return Direction.NORTHEAST;
			}
			if (y < 0)
			{
				return Direction.SOUTHEAST;
			}
			return Direction.EAST;
		}

		public static Direction GetDirectionFromIntVector2(IntVector2 vec)
		{
			return GetDirectionFromInts(vec.x, vec.y);
		}

		public static Direction GetDirectionFromVector2(Vector2 vec)
		{
			if (vec.x == 0f)
			{
				if (vec.y > 0f)
				{
					return Direction.NORTH;
				}
				if (vec.y < 0f)
				{
					return Direction.SOUTH;
				}
				return (Direction)(-1);
			}
			if (vec.x < 0f)
			{
				if (vec.y > 0f)
				{
					return Direction.NORTHWEST;
				}
				if (vec.y < 0f)
				{
					return Direction.SOUTHWEST;
				}
				return Direction.WEST;
			}
			if (vec.y > 0f)
			{
				return Direction.NORTHEAST;
			}
			if (vec.y < 0f)
			{
				return Direction.SOUTHEAST;
			}
			return Direction.EAST;
		}

		public static IntVector2 GetIntVector2FromDirection(Direction dir)
		{
			switch (dir)
			{
			case Direction.NORTH:
				return IntVector2.Up;
			case Direction.NORTHEAST:
				return IntVector2.Up + IntVector2.Right;
			case Direction.EAST:
				return IntVector2.Right;
			case Direction.SOUTHEAST:
				return IntVector2.Right + IntVector2.Down;
			case Direction.SOUTH:
				return IntVector2.Down;
			case Direction.SOUTHWEST:
				return IntVector2.Down + IntVector2.Left;
			case Direction.WEST:
				return IntVector2.Left;
			case Direction.NORTHWEST:
				return IntVector2.Left + IntVector2.Up;
			default:
				return IntVector2.Zero;
			}
		}

		public static Direction GetInverseDirection(Direction dir)
		{
			switch (dir)
			{
			case Direction.NORTH:
				return Direction.SOUTH;
			case Direction.NORTHEAST:
				return Direction.SOUTHWEST;
			case Direction.EAST:
				return Direction.WEST;
			case Direction.SOUTHEAST:
				return Direction.NORTHWEST;
			case Direction.SOUTH:
				return Direction.NORTH;
			case Direction.SOUTHWEST:
				return Direction.NORTHEAST;
			case Direction.WEST:
				return Direction.EAST;
			case Direction.NORTHWEST:
				return Direction.SOUTHEAST;
			default:
				return Direction.SOUTH;
			}
		}

		public static float GetAngleFromDirection(Direction dir)
		{
			switch (dir)
			{
			case Direction.NORTH:
				return 90f;
			case Direction.NORTHEAST:
				return 45f;
			case Direction.EAST:
				return 0f;
			case Direction.SOUTHEAST:
				return 315f;
			case Direction.SOUTH:
				return 270f;
			case Direction.SOUTHWEST:
				return 225f;
			case Direction.WEST:
				return 180f;
			case Direction.NORTHWEST:
				return 135f;
			default:
				return 0f;
			}
		}

		public void InitializeCoreData(List<RoomHandler> r)
		{
			rooms = r;
		}

		private void PreprocessDungeonWings()
		{
			if (Exit == null || Exit.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.EXIT)
			{
				return;
			}
			List<RoomHandler> list = SenseOfDirectionItem.FindPathBetweenNodes(Entrance, Exit, rooms);
			if (list == null)
			{
				return;
			}
			DungeonWingDefinition dungeonWingDefinition = null;
			if (GameManager.Instance.Dungeon.dungeonWingDefinitions.Length > 0)
			{
				dungeonWingDefinition = GameManager.Instance.Dungeon.SelectWingDefinition(true);
			}
			foreach (RoomHandler item in list)
			{
				item.IsOnCriticalPath = true;
				if (dungeonWingDefinition != null)
				{
					item.AssignRoomVisualType(dungeonWingDefinition.includedMaterialIndices.SelectByWeight(), true);
				}
			}
			int num = 0;
			if (dungeonWingDefinition != null)
			{
				dungeonWingDefinition = GameManager.Instance.Dungeon.SelectWingDefinition(false);
			}
			foreach (RoomHandler item2 in list)
			{
				foreach (RoomHandler connectedRoom in item2.connectedRooms)
				{
					if (connectedRoom.IsOnCriticalPath || connectedRoom.DungeonWingID != -1)
					{
						continue;
					}
					connectedRoom.DungeonWingID = num;
					if (dungeonWingDefinition != null)
					{
						connectedRoom.AssignRoomVisualType(dungeonWingDefinition.includedMaterialIndices.SelectByWeight(), true);
					}
					Queue<RoomHandler> queue = new Queue<RoomHandler>();
					queue.Enqueue(connectedRoom);
					while (queue.Count > 0)
					{
						RoomHandler roomHandler = queue.Dequeue();
						foreach (RoomHandler connectedRoom2 in roomHandler.connectedRooms)
						{
							if (!connectedRoom2.IsOnCriticalPath && connectedRoom2.DungeonWingID == -1)
							{
								connectedRoom2.DungeonWingID = num;
								if (dungeonWingDefinition != null)
								{
									connectedRoom2.AssignRoomVisualType(dungeonWingDefinition.includedMaterialIndices.SelectByWeight(), true);
								}
								queue.Enqueue(connectedRoom2);
							}
						}
					}
					num++;
					if (dungeonWingDefinition != null)
					{
						dungeonWingDefinition = GameManager.Instance.Dungeon.SelectWingDefinition(false);
					}
				}
			}
			foreach (RoomHandler room in rooms)
			{
				if (room.IsOnCriticalPath)
				{
					BraveUtility.DrawDebugSquare(room.area.basePosition.ToVector2(), room.area.basePosition.ToVector2() + room.area.dimensions.ToVector2(), Color.cyan, 1000f);
				}
				else
				{
					BraveUtility.DrawDebugSquare(color: new Color(1f - (float)room.DungeonWingID / 7f, 1f - (float)room.DungeonWingID / 7f, (float)room.DungeonWingID / 7f), min: room.area.basePosition.ToVector2(), max: room.area.basePosition.ToVector2() + room.area.dimensions.ToVector2(), duration: 1000f);
				}
			}
		}

		public IEnumerator Apply(TileIndices indices, TilemapDecoSettings decoSettings, tk2dTileMap tilemapRef)
		{
			tilemap = tilemapRef;
			PreprocessDungeonWings();
			foreach (RoomHandler r in rooms)
			{
				r.WriteRoomData(this);
				yield return null;
			}
			HandleTrapAreas();
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
			{
				AddProceduralTeleporters();
			}
			ComputeRoomDistanceData();
			FloodFillDungeonExterior();
			FloodFillDungeonInterior();
			yield return null;
			foreach (RoomHandler room in rooms)
			{
				room.ProcessFeatures();
				if (indices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON)
				{
					for (int i = 0; i < room.connectedRooms.Count; i++)
					{
						room.GetExitDefinitionForConnectedRoom(room.connectedRooms[i]).ProcessWestgeonData();
					}
				}
			}
			CalculatePerRoomOcclusionData();
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT && GameManager.Instance.IsLoadingFirstShortcutFloor)
			{
				RoomHandler entrance = Entrance;
				DungeonPlaceable dungeonPlaceable = BraveResources.Load("Global Prefabs/Merchant_Rat_Placeable", ".asset") as DungeonPlaceable;
				GameObject gObj = dungeonPlaceable.InstantiateObject(entrance, new IntVector2(1, 3));
				IPlayerInteractable[] interfacesInChildren = gObj.GetInterfacesInChildren<IPlayerInteractable>();
				for (int j = 0; j < interfacesInChildren.Length; j++)
				{
					entrance.RegisterInteractable(interfacesInChildren[j]);
				}
			}
			GameManager.Instance.IsLoadingFirstShortcutFloor = false;
			if (GameManager.Instance.Dungeon.decoSettings.generateLights)
			{
				IEnumerator LightTracker = GenerateLights(decoSettings);
				while (LightTracker.MoveNext())
				{
					yield return null;
				}
			}
			GenerateInterestingVisuals(decoSettings);
			SecretRoomUtility.ClearPerLevelData();
			if (GameManager.Instance.Dungeon.debugSettings.DISABLE_SECRET_ROOM_COVERS)
			{
				yield break;
			}
			foreach (RoomHandler room2 in rooms)
			{
				if (room2.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET)
				{
					room2.BuildSecretRoomCover();
				}
			}
		}

		public void PostProcessFeatures()
		{
			foreach (RoomHandler room in rooms)
			{
				room.PostProcessFeatures();
			}
			HandleFloorSpecificCustomization();
		}

		private void HandleFloorSpecificCustomization()
		{
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON)
			{
				return;
			}
			FireplaceController fireplace = UnityEngine.Object.FindObjectOfType<FireplaceController>();
			if (!fireplace)
			{
				return;
			}
			RoomHandler fireplaceRoom = fireplace.transform.position.GetAbsoluteRoom();
			List<MinorBreakable> targetBarrels = new List<MinorBreakable>();
			List<MinorBreakable> allMinorBreakables = StaticReferenceManager.AllMinorBreakables;
			int numToReplace = 2;
			Func<MinorBreakable, bool> func = delegate(MinorBreakable testBarrel)
			{
				int num = -1;
				if (!GameStatsManager.Instance.GetFlag(GungeonFlags.FLAG_ROLLED_BARREL_INTO_FIREPLACE) && testBarrel.transform.position.GetAbsoluteRoom() == fireplaceRoom)
				{
					return false;
				}
				bool flag = testBarrel.CastleReplacedWithWaterDrum;
				if (!flag)
				{
					return false;
				}
				IntVector2 intVector = testBarrel.transform.position.IntXY(VectorConversions.Floor);
				if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector) || GameManager.Instance.Dungeon.data[intVector].HasWallNeighbor())
				{
					return false;
				}
				for (int l = 0; l < targetBarrels.Count; l++)
				{
					if (targetBarrels[l].transform.position.GetAbsoluteRoom() == testBarrel.transform.position.GetAbsoluteRoom())
					{
						flag = false;
					}
					else if (targetBarrels.Count >= numToReplace && Vector2.Distance(fireplace.transform.position, targetBarrels[l].transform.position) > Vector2.Distance(fireplace.transform.position, testBarrel.transform.position))
					{
						num = l;
					}
				}
				if (flag && targetBarrels.Count < numToReplace)
				{
					targetBarrels.Add(testBarrel);
					return true;
				}
				if (flag && num != -1)
				{
					targetBarrels[num] = testBarrel;
					return true;
				}
				return false;
			};
			for (int i = 0; i < allMinorBreakables.Count; i++)
			{
				func(allMinorBreakables[i]);
			}
			DungeonPlaceable dungeonPlaceable = BraveResources.Load("Drum_Water_Castle", ".asset") as DungeonPlaceable;
			for (int j = 0; j < targetBarrels.Count; j++)
			{
				Vector3 vector = targetBarrels[j].transform.position + new Vector3(0.75f, 0f, 0f);
				RoomHandler absoluteRoom = targetBarrels[j].transform.position.GetAbsoluteRoom();
				IntVector2 location = vector.IntXY(VectorConversions.Floor) - absoluteRoom.area.basePosition;
				GameObject gameObject = dungeonPlaceable.InstantiateObject(absoluteRoom, location);
				gameObject.transform.position = gameObject.transform.position;
				KickableObject componentInChildren = gameObject.GetComponentInChildren<KickableObject>();
				if ((bool)componentInChildren)
				{
					componentInChildren.specRigidbody.Reinitialize();
					componentInChildren.rollSpeed = 3f;
					componentInChildren.AllowTopWallTraversal = true;
					absoluteRoom.RegisterInteractable(componentInChildren);
				}
				KickableObject component = targetBarrels[j].GetComponent<KickableObject>();
				if ((bool)component)
				{
					component.ForceDeregister();
				}
				UnityEngine.Object.Destroy(targetBarrels[j].gameObject);
			}
			if (targetBarrels.Count >= numToReplace)
			{
				return;
			}
			for (int k = 0; k < rooms.Count; k++)
			{
				if (rooms[k].IsShop)
				{
					IntVector2 bestRewardLocation = rooms[k].GetBestRewardLocation(IntVector2.One * 2, RoomHandler.RewardLocationStyle.Original, false);
					GameObject gameObject2 = dungeonPlaceable.InstantiateObject(rooms[k], bestRewardLocation - rooms[k].area.basePosition);
					KickableObject componentInChildren2 = gameObject2.GetComponentInChildren<KickableObject>();
					if ((bool)componentInChildren2)
					{
						componentInChildren2.rollSpeed = 3f;
						componentInChildren2.AllowTopWallTraversal = true;
						rooms[k].RegisterInteractable(componentInChildren2);
					}
				}
			}
		}

		private void HandleTrapAreas()
		{
			PathingTrapController[] array = UnityEngine.Object.FindObjectsOfType<PathingTrapController>();
			foreach (PathingTrapController pathingTrapController in array)
			{
				if (!pathingTrapController.specRigidbody)
				{
					return;
				}
				pathingTrapController.specRigidbody.Initialize();
				RoomHandler absoluteRoomFromPosition = GetAbsoluteRoomFromPosition(pathingTrapController.specRigidbody.UnitCenter.ToIntVector2());
				PathMover component = pathingTrapController.GetComponent<PathMover>();
				Vector2 unitDimensions = pathingTrapController.specRigidbody.UnitDimensions;
				ResizableCollider component2 = pathingTrapController.GetComponent<ResizableCollider>();
				if ((bool)component2)
				{
					if (component2.IsHorizontal)
					{
						unitDimensions.x = component2.NumTiles;
					}
					else
					{
						unitDimensions.y = component2.NumTiles;
					}
				}
				Vector2 vector = Vector2Extensions.max;
				Vector2 vector2 = Vector2Extensions.min;
				for (int j = 0; j < component.Path.nodes.Count; j++)
				{
					Vector2 vector3 = absoluteRoomFromPosition.area.basePosition.ToVector2() + component.Path.nodes[j].RoomPosition;
					vector = Vector2.Min(vector, vector3);
					vector2 = Vector2.Max(vector2, vector3 + unitDimensions);
				}
				IntVector2 intVector = vector.ToIntVector2(VectorConversions.Floor);
				IntVector2 intVector2 = vector2.ToIntVector2(VectorConversions.Floor);
				for (int k = intVector.x; k <= intVector2.x; k++)
				{
					for (int l = intVector.y; l <= intVector2.y; l++)
					{
						this[k, l].IsTrapZone = true;
					}
				}
			}
			ProjectileTrapController[] array2 = UnityEngine.Object.FindObjectsOfType<ProjectileTrapController>();
			foreach (ProjectileTrapController projectileTrapController in array2)
			{
				IntVector2 intVector3 = projectileTrapController.shootPoint.position.IntXY(VectorConversions.Floor);
				IntVector2 intVector2FromDirection = GetIntVector2FromDirection(projectileTrapController.shootDirection);
				if (intVector2FromDirection == IntVector2.Zero)
				{
					continue;
				}
				IntVector2 intVector4 = intVector3;
				while (true)
				{
					if (!CheckInBoundsAndValid(intVector4) || isWall(intVector4.x, intVector4.y))
					{
						if (!(intVector3 == intVector4))
						{
							break;
						}
					}
					else
					{
						this[intVector4].IsTrapZone = true;
					}
					intVector4 += intVector2FromDirection;
				}
			}
		}

		private void AddProceduralTeleporters()
		{
			List<List<RoomHandler>> list = new List<List<RoomHandler>>();
			List<RoomHandler> roomsContainingTeleporters = new List<RoomHandler>();
			Func<RoomHandler, bool> func = delegate(RoomHandler r)
			{
				if (r.area.IsProceduralRoom)
				{
					return false;
				}
				if (!r.EverHadEnemies)
				{
					return false;
				}
				if (r.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD || r.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET)
				{
					return false;
				}
				if (r.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS || r.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.EXIT)
				{
					return false;
				}
				if (r.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.NORMAL && r.area.PrototypeRoomNormalSubcategory == PrototypeDungeonRoom.RoomNormalSubCategory.TRAP)
				{
					return false;
				}
				for (int num6 = 0; num6 < r.connectedRooms.Count; num6++)
				{
					if (roomsContainingTeleporters.Contains(r.connectedRooms[num6]))
					{
						return false;
					}
				}
				return true;
			};
			for (int i = 0; i < rooms.Count; i++)
			{
				if (Minimap.Instance.HasTeleporterIcon(rooms[i]))
				{
					roomsContainingTeleporters.Add(rooms[i]);
				}
				if (!roomsContainingTeleporters.Contains(rooms[i]) && rooms[i].connectedRooms.Count >= 4)
				{
					rooms[i].AddProceduralTeleporterToRoom();
					roomsContainingTeleporters.Add(rooms[i]);
				}
				if (rooms[i].IsLoopMember)
				{
					List<RoomHandler> list2 = null;
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j][0].LoopGuid.Equals(rooms[i].LoopGuid))
						{
							list2 = list[j];
							break;
						}
					}
					if (list2 != null)
					{
						list2.Add(rooms[i]);
						continue;
					}
					list2 = new List<RoomHandler>();
					list2.Add(rooms[i]);
					list.Add(list2);
				}
				else if (!roomsContainingTeleporters.Contains(rooms[i]) && rooms[i].connectedRooms.Count == 1)
				{
					if (func(rooms[i]))
					{
						rooms[i].AddProceduralTeleporterToRoom();
						roomsContainingTeleporters.Add(rooms[i]);
					}
					else if (func(rooms[i].connectedRooms[0]))
					{
						rooms[i].connectedRooms[0].AddProceduralTeleporterToRoom();
						roomsContainingTeleporters.Add(rooms[i].connectedRooms[0]);
					}
				}
			}
			Func<RoomHandler, int> func2 = delegate(RoomHandler r)
			{
				int num4 = int.MaxValue;
				for (int n = 0; n < roomsContainingTeleporters.Count; n++)
				{
					int num5 = IntVector2.ManhattanDistance(roomsContainingTeleporters[n].Epicenter, r.Epicenter);
					if (num5 < num4)
					{
						num4 = num5;
					}
				}
				return num4;
			};
			for (int k = 0; k < list.Count; k++)
			{
				List<RoomHandler> list3 = list[k];
				int num = Mathf.Max(1, Mathf.RoundToInt((float)list3.Count / 4f));
				for (int l = 0; l < num; l++)
				{
					RoomHandler roomHandler = null;
					int num2 = int.MinValue;
					for (int m = 0; m < list3.Count; m++)
					{
						if (func(list3[m]))
						{
							int num3 = func2(list3[m]);
							if (list3[m].connectedRooms.Count > 2)
							{
								num3 += 10;
							}
							if (num3 > num2)
							{
								roomHandler = list3[m];
								num2 = num3;
							}
						}
					}
					if (roomHandler != null)
					{
						roomHandler.AddProceduralTeleporterToRoom();
						if (!roomsContainingTeleporters.Contains(roomHandler))
						{
							roomsContainingTeleporters.Add(roomHandler);
						}
					}
				}
			}
		}

		public void PostGenerationCleanup()
		{
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					bool flag = true;
					if (cellData[i][j] != null && cellData[i][j].cellVisualData.IsFeatureCell)
					{
						flag = false;
					}
					if (flag)
					{
						for (int k = -3; k <= 3; k++)
						{
							for (int l = -3; l <= 3; l++)
							{
								if (CheckInBounds(i + k, j + l) && cellData[i + k][j + l] != null && cellData[i + k][j + l].type != CellType.WALL)
								{
									flag = false;
								}
								if (!flag)
								{
									break;
								}
							}
							if (!flag)
							{
								break;
							}
						}
					}
					if (flag)
					{
						cellData[i][j] = null;
					}
					else if (cellData[i][j].type != CellType.WALL)
					{
						bool isNextToWall = cellData[i][j].isNextToWall;
					}
				}
			}
		}

		public RoomHandler GetAbsoluteRoomFromPosition(IntVector2 pos)
		{
			CellData cellData = ((!CheckInBounds(pos)) ? null : this[pos]);
			if (cellData == null)
			{
				float num = float.MaxValue;
				RoomHandler result = null;
				for (int i = 0; i < rooms.Count; i++)
				{
					float num2 = BraveMathCollege.DistToRectangle(pos.ToCenterVector2(), rooms[i].area.basePosition.ToVector2(), rooms[i].area.dimensions.ToVector2());
					if (num2 < num)
					{
						num = num2;
						result = rooms[i];
					}
				}
				return result;
			}
			if (cellData.parentRoom == null)
			{
				return cellData.nearestRoom;
			}
			return cellData.parentRoom;
		}

		public RoomHandler GetRoomFromPosition(IntVector2 pos)
		{
			CellData cellData = this[pos];
			return cellData.parentRoom;
		}

		public CellVisualData.CellFloorType GetFloorTypeFromPosition(IntVector2 pos)
		{
			if (!CheckInBoundsAndValid(pos))
			{
				return CellVisualData.CellFloorType.Stone;
			}
			return this[pos].cellVisualData.floorType;
		}

		public CellType GetCellTypeSafe(IntVector2 pos)
		{
			if (!CheckInBounds(pos))
			{
				return CellType.WALL;
			}
			CellData cellData = this[pos];
			if (cellData == null)
			{
				return CellType.WALL;
			}
			return cellData.type;
		}

		public CellType GetCellTypeSafe(int x, int y)
		{
			if (!CheckInBounds(x, y))
			{
				return CellType.WALL;
			}
			CellData cellData = this[x, y];
			if (cellData == null)
			{
				return CellType.WALL;
			}
			return cellData.type;
		}

		public CellData GetCellSafe(IntVector2 pos)
		{
			return (!CheckInBounds(pos)) ? null : this[pos];
		}

		public CellData GetCellSafe(int x, int y)
		{
			return (!CheckInBounds(x, y)) ? null : this[x, y];
		}

		private static bool CheckCellNeedsAdditionalLight(List<IntVector2> positions, RoomHandler room, CellData currentCell)
		{
			int num = ((!room.area.IsProceduralRoom) ? 10 : 20);
			if (currentCell.isExitCell)
			{
				return false;
			}
			if (currentCell.type == CellType.WALL)
			{
				return false;
			}
			bool flag = true;
			for (int i = 0; i < positions.Count; i++)
			{
				int num2 = IntVector2.ManhattanDistance(positions[i] + room.area.basePosition, currentCell.position);
				if (num2 <= num)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				positions.Add(currentCell.position - room.area.basePosition);
			}
			return flag;
		}

		private void PostprocessLightPositions(List<IntVector2> positions, RoomHandler room)
		{
			CheckCellNeedsAdditionalLight(positions, room, this[room.GetCenterCell()]);
			for (int i = 0; i < room.Cells.Count; i++)
			{
				CellData currentCell = this[room.Cells[i]];
				CheckCellNeedsAdditionalLight(positions, room, currentCell);
			}
		}

		public void ReplicateLighting(CellData sourceCell, CellData targetCell)
		{
			Vector3 position = sourceCell.cellVisualData.lightObject.transform.position - sourceCell.position.ToVector2().ToVector3ZisY() + targetCell.position.ToVector2().ToVector3ZisY();
			GameObject gameObject = UnityEngine.Object.Instantiate(sourceCell.cellVisualData.lightObject, position, Quaternion.identity);
			gameObject.transform.parent = sourceCell.cellVisualData.lightObject.transform.parent;
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
			{
				this[targetCell.position + IntVector2.Down].cellVisualData.containsObjectSpaceStamp = true;
			}
			targetCell.cellVisualData.containsLight = true;
			targetCell.cellVisualData.lightObject = gameObject;
			targetCell.cellVisualData.facewallLightStampData = sourceCell.cellVisualData.facewallLightStampData;
			targetCell.cellVisualData.sidewallLightStampData = sourceCell.cellVisualData.sidewallLightStampData;
		}

		public void GenerateLightsForRoom(TilemapDecoSettings decoSettings, RoomHandler rh, Transform lightParent, LightGenerationStyle style = LightGenerationStyle.STANDARD)
		{
			if (!GameManager.Instance.Dungeon.roomMaterialDefinitions[rh.RoomVisualSubtype].useLighting)
			{
				return;
			}
			bool flag = decoSettings.lightCookies.Length > 0;
			List<IntVector2> list = null;
			List<Tuple<IntVector2, float>> list2 = new List<Tuple<IntVector2, float>>();
			bool flag2 = false;
			int num = 0;
			if (rh.area != null && !rh.area.IsProceduralRoom && !rh.area.prototypeRoom.usesProceduralLighting)
			{
				list = rh.GatherManualLightPositions();
				num = list.Count;
			}
			else
			{
				flag2 = true;
				list = rh.GatherOptimalLightPositions(decoSettings);
				num = list.Count;
				if (rh.area != null && rh.area.prototypeRoom != null)
				{
					PostprocessLightPositions(list, rh);
				}
			}
			if (rh.area.prototypeRoom != null)
			{
				for (int i = 0; i < rh.area.instanceUsedExits.Count; i++)
				{
					RuntimeRoomExitData runtimeRoomExitData = rh.area.exitToLocalDataMap[rh.area.instanceUsedExits[i]];
					RuntimeExitDefinition runtimeExitDefinition = rh.exitDefinitionsByExit[runtimeRoomExitData];
					if (runtimeRoomExitData.TotalExitLength > 4 && !runtimeExitDefinition.containsLight)
					{
						IntVector2 first = ((!runtimeRoomExitData.jointedExit) ? runtimeExitDefinition.GetLinearMidpoint(rh) : (runtimeRoomExitData.ExitOrigin - IntVector2.One));
						list2.Add(new Tuple<IntVector2, float>(first, 0.5f));
						runtimeExitDefinition.containsLight = true;
					}
				}
			}
			GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
			float lightCullingPercentage = decoSettings.lightCullingPercentage;
			if (flag2 && lightCullingPercentage > 0f)
			{
				int num2 = Mathf.FloorToInt((float)list.Count * lightCullingPercentage);
				int num3 = Mathf.FloorToInt((float)list2.Count * lightCullingPercentage);
				if (num2 == 0 && num3 == 0 && list.Count + list2.Count > 4)
				{
					num2 = 1;
				}
				while (num2 > 0 && list.Count > 0)
				{
					list.RemoveAt(UnityEngine.Random.Range(0, list.Count));
					num2--;
				}
				while (num3 > 0 && list2.Count > 0)
				{
					list2.RemoveAt(UnityEngine.Random.Range(0, list2.Count));
					num3--;
				}
			}
			int count = list.Count;
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE && (tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON || tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON || tilesetId == GlobalDungeonData.ValidTilesets.CATACOMBGEON) && (flag2 || rh.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.NORMAL || rh.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.HUB || rh.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.CONNECTOR))
			{
				list.AddRange(rh.GatherPitLighting(decoSettings, list));
			}
			for (int j = 0; j < list.Count + list2.Count; j++)
			{
				IntVector2 negOne = IntVector2.NegOne;
				float num4 = 1f;
				bool flag3 = false;
				if (j < list.Count && j >= count)
				{
					flag3 = true;
					num4 = 0.6f;
				}
				if (j < list.Count)
				{
					negOne = rh.area.basePosition + list[j];
				}
				else
				{
					negOne = rh.area.basePosition + list2[j - list.Count].First;
					num4 = list2[j - list.Count].Second;
				}
				bool flag4 = false;
				if (flag && flag2 && negOne == rh.GetCenterCell())
				{
					flag4 = true;
				}
				IntVector2 intVector = negOne + IntVector2.Up;
				bool flag5 = j >= num;
				bool flag6 = false;
				Vector3 vector = Vector3.zero;
				if (this[negOne + IntVector2.Up].type == CellType.WALL)
				{
					this[intVector].cellVisualData.lightDirection = Direction.NORTH;
					vector = Vector3.down;
				}
				else if (this[negOne + IntVector2.Right].type == CellType.WALL)
				{
					this[intVector].cellVisualData.lightDirection = Direction.EAST;
				}
				else if (this[negOne + IntVector2.Left].type == CellType.WALL)
				{
					this[intVector].cellVisualData.lightDirection = Direction.WEST;
				}
				else if (this[negOne + IntVector2.Down].type == CellType.WALL)
				{
					flag6 = true;
					this[intVector].cellVisualData.lightDirection = Direction.SOUTH;
				}
				else
				{
					this[intVector].cellVisualData.lightDirection = (Direction)(-1);
				}
				int num5 = rh.RoomVisualSubtype;
				float num6 = 0f;
				if (rh.area.prototypeRoom != null)
				{
					PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = ((j >= list.Count) ? rh.area.prototypeRoom.ForceGetCellDataAtPoint(list2[j - list.Count].First.x, list2[j - list.Count].First.y) : rh.area.prototypeRoom.ForceGetCellDataAtPoint(list[j].x, list[j].y));
					if (prototypeDungeonRoomCellData != null && prototypeDungeonRoomCellData.containsManuallyPlacedLight)
					{
						num5 = prototypeDungeonRoomCellData.lightStampIndex;
						num6 = (float)prototypeDungeonRoomCellData.lightPixelsOffsetY / 16f;
					}
				}
				if (num5 < 0 || num5 >= GameManager.Instance.Dungeon.roomMaterialDefinitions.Length)
				{
					num5 = 0;
				}
				DungeonMaterial dungeonMaterial = GameManager.Instance.Dungeon.roomMaterialDefinitions[num5];
				int outIndex = -1;
				GameObject gameObject = null;
				if (style == LightGenerationStyle.FORCE_COLOR || style == LightGenerationStyle.RAT_HALLWAY)
				{
					outIndex = 0;
					gameObject = dungeonMaterial.lightPrefabs.elements[0].gameObject;
				}
				else
				{
					gameObject = dungeonMaterial.lightPrefabs.SelectByWeight(out outIndex);
				}
				if ((!dungeonMaterial.facewallLightStamps[outIndex].CanBeTopWallLight && flag6) || (!dungeonMaterial.facewallLightStamps[outIndex].CanBeCenterLight && flag5))
				{
					if (outIndex >= dungeonMaterial.facewallLightStamps.Count)
					{
						outIndex = 0;
					}
					outIndex = dungeonMaterial.facewallLightStamps[outIndex].FallbackIndex;
					gameObject = dungeonMaterial.lightPrefabs.elements[outIndex].gameObject;
				}
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, intVector.ToVector3(0f), Quaternion.identity);
				gameObject2.transform.parent = lightParent;
				gameObject2.transform.position = intVector.ToCenterVector3((float)intVector.y + decoSettings.lightHeight) + new Vector3(0f, num6, 0f) + vector;
				ShadowSystem componentInChildren = gameObject2.GetComponentInChildren<ShadowSystem>();
				Light componentInChildren2 = gameObject2.GetComponentInChildren<Light>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.intensity *= num4;
				}
				if (style == LightGenerationStyle.FORCE_COLOR || style == LightGenerationStyle.RAT_HALLWAY)
				{
					SceneLightManager component = gameObject2.GetComponent<SceneLightManager>();
					if ((bool)component)
					{
						Color[] array = (component.validColors = new Color[1] { component.validColors[0] });
					}
				}
				if (flag3 && componentInChildren != null)
				{
					if ((bool)componentInChildren2)
					{
						componentInChildren2.range += ((GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CATACOMBGEON) ? 3 : 5);
					}
					componentInChildren.ignoreCustomFloorLight = true;
				}
				if (flag4 && flag && componentInChildren != null)
				{
					componentInChildren.uLightCookie = decoSettings.GetRandomLightCookie();
					componentInChildren.uLightCookieAngle = UnityEngine.Random.Range(0f, 6.28f);
					componentInChildren2.intensity *= 1.5f;
				}
				if (this[intVector].cellVisualData.lightDirection == Direction.NORTH)
				{
					bool flag7 = true;
					for (int k = -2; k < 3; k++)
					{
						if (this[intVector + IntVector2.Right * k].type == CellType.FLOOR)
						{
							flag7 = false;
							break;
						}
					}
					if (flag7 && (bool)componentInChildren)
					{
						GameObject original = (GameObject)BraveResources.Load("Global VFX/Wall_Light_Cookie");
						GameObject gameObject3 = UnityEngine.Object.Instantiate(original);
						Transform transform = gameObject3.transform;
						transform.parent = gameObject2.transform;
						transform.localPosition = Vector3.zero;
						componentInChildren.PersonalCookies.Add(gameObject3.GetComponent<Renderer>());
					}
				}
				CellData cellData = this[intVector + new IntVector2(0, Mathf.RoundToInt(num6))];
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
				{
					this[cellData.position + IntVector2.Down].cellVisualData.containsObjectSpaceStamp = true;
				}
				BraveUtility.DrawDebugSquare(cellData.position.ToVector2(), Color.magenta, 1000f);
				cellData.cellVisualData.containsLight = true;
				cellData.cellVisualData.lightObject = gameObject2;
				LightStampData facewallLightStampData = dungeonMaterial.facewallLightStamps[outIndex];
				LightStampData sidewallLightStampData = dungeonMaterial.sidewallLightStamps[outIndex];
				cellData.cellVisualData.facewallLightStampData = facewallLightStampData;
				cellData.cellVisualData.sidewallLightStampData = sidewallLightStampData;
			}
		}

		private IEnumerator GenerateLights(TilemapDecoSettings decoSettings)
		{
			Transform lightParent = new GameObject("_Lights").transform;
			bool lightCookiesAvailable = decoSettings.lightCookies.Length > 0;
			int rhIterator = 0;
			foreach (RoomHandler rh in rooms)
			{
				rhIterator++;
				GenerateLightsForRoom(decoSettings, rh, lightParent);
				if (rhIterator % 5 == 0)
				{
					yield return null;
				}
			}
		}

		private void GenerateInterestingVisuals(TilemapDecoSettings dungeonDecoSettings)
		{
			List<IntVector2> patchPoints = new List<IntVector2>();
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					if (UnityEngine.Random.value < dungeonDecoSettings.decoPatchFrequency)
					{
						patchPoints.Add(new IntVector2(i, j));
					}
				}
			}
			Func<IntVector2, int> func = delegate(IntVector2 a)
			{
				int num18 = int.MaxValue;
				for (int num19 = 0; num19 < patchPoints.Count; num19++)
				{
					int b = IntVector2.ManhattanDistance(patchPoints[num19], a);
					num18 = Mathf.Min(num18, b);
				}
				return num18;
			};
			for (int k = 0; k < Width; k++)
			{
				for (int l = 0; l < Height; l++)
				{
					CellData cellData = this.cellData[k][l];
					if (cellData == null || (cellData.type == CellType.WALL && !cellData.IsLowerFaceWall()) || cellData.doesDamage)
					{
						continue;
					}
					TilemapDecoSettings.DecoStyle decalLayerStyle = dungeonDecoSettings.decalLayerStyle;
					int decalSize = dungeonDecoSettings.decalSize;
					int decalSpacing = dungeonDecoSettings.decalSpacing;
					TilemapDecoSettings.DecoStyle patternLayerStyle = dungeonDecoSettings.patternLayerStyle;
					int patternSize = dungeonDecoSettings.patternSize;
					int patternSpacing = dungeonDecoSettings.patternSpacing;
					bool flag = false;
					bool flag2 = false;
					if (cellData.cellVisualData.roomVisualTypeIndex >= 0 && cellData.cellVisualData.roomVisualTypeIndex < GameManager.Instance.Dungeon.roomMaterialDefinitions.Length)
					{
						DungeonMaterial dungeonMaterial = GameManager.Instance.Dungeon.roomMaterialDefinitions[cellData.cellVisualData.roomVisualTypeIndex];
						if (dungeonMaterial.usesDecalLayer)
						{
							flag = true;
							decalLayerStyle = dungeonMaterial.decalLayerStyle;
							decalSize = dungeonMaterial.decalSize;
							decalSpacing = dungeonMaterial.decalSpacing;
						}
						if (dungeonMaterial.usesPatternLayer)
						{
							flag2 = true;
							patternLayerStyle = dungeonMaterial.patternLayerStyle;
							patternSize = dungeonMaterial.patternSize;
							patternSpacing = dungeonMaterial.patternSpacing;
						}
					}
					float num = -0.35f + (float)decalSize / 10f;
					float num2 = -0.35f + (float)patternSize / 10f;
					if (flag)
					{
						switch (decalLayerStyle)
						{
						case TilemapDecoSettings.DecoStyle.NONE:
							cellData.cellVisualData.isDecal = false;
							break;
						case TilemapDecoSettings.DecoStyle.GROW_FROM_WALLS:
							if (cellData.HasMossyNeighbor(this) && UnityEngine.Random.value > (float)(10 - decalSize) / 10f)
							{
								cellData.cellVisualData.isDecal = true;
							}
							if (cellData.IsLowerFaceWall())
							{
								cellData.cellVisualData.isDecal = true;
							}
							break;
						case TilemapDecoSettings.DecoStyle.PERLIN_NOISE:
						{
							float num7 = Noise.Generate((float)cellData.position.x / (4f + (float)decalSpacing), (float)cellData.position.y / (4f + (float)decalSpacing));
							if (num7 < num)
							{
								cellData.cellVisualData.isDecal = true;
							}
							break;
						}
						case TilemapDecoSettings.DecoStyle.HORIZONTAL_STRIPES:
						{
							int num4 = cellData.position.y % (decalSize + decalSpacing);
							if (num4 < decalSize)
							{
								cellData.cellVisualData.isDecal = true;
							}
							break;
						}
						case TilemapDecoSettings.DecoStyle.VERTICAL_STRIPES:
						{
							int num5 = cellData.position.x % (decalSize + decalSpacing);
							if (num5 < decalSize)
							{
								cellData.cellVisualData.isDecal = true;
							}
							break;
						}
						case TilemapDecoSettings.DecoStyle.AROUND_LIGHTS:
							if (cellData.cellVisualData.distanceToNearestLight <= decalSize)
							{
								float num6 = (float)cellData.cellVisualData.distanceToNearestLight / ((float)decalSize * 1f);
								if (UnityEngine.Random.value > num6)
								{
									cellData.cellVisualData.isDecal = true;
								}
							}
							break;
						case TilemapDecoSettings.DecoStyle.PATCHES:
						{
							int num3 = func(cellData.position);
							if (num3 < decalSize || (num3 == decalSize && (double)UnityEngine.Random.value > 0.5))
							{
								cellData.cellVisualData.isDecal = true;
							}
							break;
						}
						}
					}
					if (!flag2)
					{
						continue;
					}
					switch (patternLayerStyle)
					{
					case TilemapDecoSettings.DecoStyle.NONE:
						cellData.cellVisualData.isPattern = false;
						break;
					case TilemapDecoSettings.DecoStyle.GROW_FROM_WALLS:
						if (cellData.HasPatternNeighbor(this) && UnityEngine.Random.value > (float)(10 - patternSize) / 10f)
						{
							cellData.cellVisualData.isPattern = true;
						}
						if (cellData.IsLowerFaceWall())
						{
							cellData.cellVisualData.isPattern = true;
						}
						break;
					case TilemapDecoSettings.DecoStyle.PERLIN_NOISE:
					{
						float num10 = Noise.Generate((float)cellData.position.x / (4f + (float)patternSpacing), (float)cellData.position.y / (4f + (float)patternSpacing));
						if (num10 < num2)
						{
							cellData.cellVisualData.isPattern = true;
						}
						break;
					}
					case TilemapDecoSettings.DecoStyle.HORIZONTAL_STRIPES:
					{
						int num9 = cellData.position.y % (patternSize + patternSpacing);
						if (num9 < patternSize)
						{
							cellData.cellVisualData.isPattern = true;
						}
						break;
					}
					case TilemapDecoSettings.DecoStyle.VERTICAL_STRIPES:
					{
						int num11 = cellData.position.x % (patternSize + patternSpacing);
						if (num11 < patternSize)
						{
							cellData.cellVisualData.isPattern = true;
						}
						break;
					}
					case TilemapDecoSettings.DecoStyle.AROUND_LIGHTS:
						if (cellData.cellVisualData.distanceToNearestLight <= patternSize)
						{
							cellData.cellVisualData.isPattern = true;
						}
						break;
					case TilemapDecoSettings.DecoStyle.PATCHES:
					{
						int num8 = func(cellData.position);
						if (num8 < patternSize || (num8 == patternSize && (double)UnityEngine.Random.value > 0.5))
						{
							cellData.cellVisualData.isPattern = true;
						}
						break;
					}
					}
				}
			}
			if (dungeonDecoSettings.patternExpansion > 0)
			{
				for (int m = 0; m < dungeonDecoSettings.patternExpansion; m++)
				{
					HashSet<CellData> hashSet = new HashSet<CellData>();
					for (int n = 0; n < Width; n++)
					{
						for (int num12 = 0; num12 < Height; num12++)
						{
							CellData cellData2 = this.cellData[n][num12];
							if (cellData2 != null && !cellData2.cellVisualData.isPattern && cellData2.HasPatternNeighbor(this) && !cellData2.doesDamage)
							{
								hashSet.Add(cellData2);
							}
						}
					}
					foreach (CellData item in hashSet)
					{
						item.cellVisualData.isPattern = true;
					}
				}
			}
			if (dungeonDecoSettings.decalExpansion > 0)
			{
				for (int num13 = 0; num13 < dungeonDecoSettings.decalExpansion; num13++)
				{
					HashSet<CellData> hashSet2 = new HashSet<CellData>();
					for (int num14 = 0; num14 < Width; num14++)
					{
						for (int num15 = 0; num15 < Height; num15++)
						{
							CellData cellData3 = this.cellData[num14][num15];
							if (cellData3 != null && !cellData3.cellVisualData.isDecal && cellData3.HasMossyNeighbor(this) && !cellData3.doesDamage)
							{
								hashSet2.Add(cellData3);
							}
						}
					}
					foreach (CellData item2 in hashSet2)
					{
						item2.cellVisualData.isDecal = true;
					}
				}
			}
			if (!dungeonDecoSettings.debug_view)
			{
				return;
			}
			for (int num16 = 0; num16 < Width; num16++)
			{
				for (int num17 = 0; num17 < Height; num17++)
				{
					CellData cellData4 = this.cellData[num16][num17];
					if (cellData4 != null)
					{
						if (cellData4.cellVisualData.isDecal && cellData4.cellVisualData.isPattern)
						{
							DebugDrawCross(cellData4.position.ToCenterVector3(-10f), Color.grey);
						}
						else if (cellData4.cellVisualData.isDecal)
						{
							DebugDrawCross(cellData4.position.ToCenterVector3(-10f), Color.green);
						}
						else if (cellData4.cellVisualData.isPattern)
						{
							DebugDrawCross(cellData4.position.ToCenterVector3(-10f), Color.red);
						}
					}
				}
			}
		}

		private void DoRoomDistanceDebug()
		{
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					CellData cellData = this.cellData[i][j];
					Vector3 centerPoint = cellData.position.ToCenterVector3(-10f);
					if (!(cellData.distanceFromNearestRoom > (float)Pixelator.Instance.perimeterTileWidth))
					{
						Color crosscolor = new Color(cellData.distanceFromNearestRoom / 7f, cellData.distanceFromNearestRoom / 7f, cellData.distanceFromNearestRoom / 7f);
						DebugDrawCross(centerPoint, crosscolor);
					}
				}
			}
		}

		private void DebugDrawCross(Vector3 centerPoint, Color crosscolor)
		{
			Debug.DrawLine(centerPoint + new Vector3(-0.5f, 0f, 0f), centerPoint + new Vector3(0.5f, 0f, 0f), crosscolor, 1000f);
			Debug.DrawLine(centerPoint + new Vector3(0f, -0.5f, 0f), centerPoint + new Vector3(0f, 0.5f, 0f), crosscolor, 1000f);
		}

		private void FloodFillDungeonInterior()
		{
			Stack<CellData> stack = new Stack<CellData>();
			for (int i = 0; i < rooms.Count; i++)
			{
				if (rooms[i] == Entrance || rooms[i].IsStartOfWarpWing)
				{
					stack.Push(this[rooms[i].GetRandomAvailableCellDumb()]);
				}
			}
			while (stack.Count > 0)
			{
				CellData cellData = stack.Pop();
				if (cellData.type == CellType.WALL)
				{
					continue;
				}
				List<CellData> cellNeighbors = GetCellNeighbors(cellData);
				cellData.isGridConnected = true;
				for (int j = 0; j < cellNeighbors.Count; j++)
				{
					if (cellNeighbors[j] != null && cellNeighbors[j].type != CellType.WALL && !cellNeighbors[j].isGridConnected)
					{
						stack.Push(cellNeighbors[j]);
					}
				}
			}
		}

		private void FloodFillDungeonExterior()
		{
			Stack<IntVector2> stack = new Stack<IntVector2>();
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			stack.Push(IntVector2.Zero);
			while (stack.Count > 0)
			{
				IntVector2 intVector = stack.Pop();
				hashSet.Add(intVector);
				CellData cellData = this[intVector];
				if (cellData != null && (cellData.type != CellType.WALL || cellData.breakable) && !cellData.isExitCell)
				{
					continue;
				}
				if (cellData != null)
				{
					cellData.isRoomInternal = false;
				}
				for (int i = 0; i < IntVector2.Cardinals.Length; i++)
				{
					IntVector2 intVector2 = intVector + IntVector2.Cardinals[i];
					if (!hashSet.Contains(intVector2) && intVector2.x >= 0 && intVector2.y >= 0 && intVector2.x < Width && intVector2.y < Height)
					{
						stack.Push(intVector2);
					}
				}
			}
		}

		private void ComputeRoomDistanceData()
		{
			Queue<CellData> queue = new Queue<CellData>();
			HashSet<CellData> hashSet = new HashSet<CellData>();
			for (int i = 0; i < this.cellData.Length; i++)
			{
				for (int j = 0; j < this.cellData[i].Length; j++)
				{
					CellData cellData = this.cellData[i][j];
					if (cellData != null && cellData.distanceFromNearestRoom == 1f)
					{
						queue.Enqueue(cellData);
						hashSet.Add(cellData);
					}
				}
			}
			while (queue.Count > 0)
			{
				CellData cellData2 = queue.Dequeue();
				hashSet.Remove(cellData2);
				List<CellData> cellNeighbors = GetCellNeighbors(cellData2, true);
				for (int k = 0; k < cellNeighbors.Count; k++)
				{
					CellData cellData3 = cellNeighbors[k];
					if (cellData3 == null)
					{
						continue;
					}
					float num = ((k % 2 != 1) ? (cellData2.distanceFromNearestRoom + 1f) : (cellData2.distanceFromNearestRoom + 1.414f));
					if (cellData3.distanceFromNearestRoom > num)
					{
						cellData3.distanceFromNearestRoom = num;
						cellData3.nearestRoom = cellData2.nearestRoom;
						if (!hashSet.Contains(cellData3))
						{
							queue.Enqueue(cellData3);
							hashSet.Add(cellData3);
						}
					}
				}
			}
		}

		private void CalculatePerRoomOcclusionData()
		{
		}

		private HashSet<CellData> ComputeRoomDistanceHorizon(HashSet<CellData> horizon, float dist)
		{
			HashSet<CellData> hashSet = new HashSet<CellData>();
			foreach (CellData item in horizon)
			{
				List<CellData> cellNeighbors = GetCellNeighbors(item, true);
				for (int i = 0; i < cellNeighbors.Count; i++)
				{
					CellData cellData = cellNeighbors[i];
					if (cellData != null && !hashSet.Contains(cellData))
					{
						float num = ((i % 2 != 1) ? (dist + 1f) : (dist + 1.414f));
						if (cellData.distanceFromNearestRoom > num)
						{
							cellData.distanceFromNearestRoom = num;
							cellData.nearestRoom = item.nearestRoom;
							hashSet.Add(cellData);
						}
					}
				}
			}
			return hashSet;
		}

		public void CheckIntegrity()
		{
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					if (j > 1 && j < Height - 1 && cellData[i][j + 1] != null && isSingleCellWall(i, j))
					{
						cellData[i][j + 1].type = CellType.WALL;
						RoomHandler parentRoom = cellData[i][j].parentRoom;
						if (parentRoom != null)
						{
							IntVector2 item = new IntVector2(i, j + 1);
							if (parentRoom.RawCells.Remove(item))
							{
								parentRoom.Cells.Remove(item);
								parentRoom.CellsWithoutExits.Remove(item);
							}
						}
					}
					if (cellData[i][j] == null || cellData[i][j].type != CellType.FLOOR)
					{
						continue;
					}
					bool flag = false;
					foreach (CellData cellNeighbor in GetCellNeighbors(cellData[i][j]))
					{
						if (cellNeighbor != null && cellNeighbor.type == CellType.FLOOR)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						cellData[i][j].type = CellType.WALL;
					}
				}
			}
			ExciseElbows();
		}

		protected void ExciseElbows()
		{
			bool flag = true;
			List<CellData> list = new List<CellData>();
			int num = 0;
			while (flag && num < 1000)
			{
				num++;
				list.Clear();
				flag = false;
				for (int i = 0; i < Width; i++)
				{
					for (int j = 0; j < Height; j++)
					{
						if (this.cellData[i][j] == null || (!this.cellData[i][j].isExitCell && (this.cellData[i][j].parentRoom == null || !this.cellData[i][j].parentRoom.area.IsProceduralRoom)))
						{
							continue;
						}
						if (this.cellData[i][j].isExitCell && !GameManager.Instance.Dungeon.UsesWallWarpWingDoors)
						{
							bool flag2 = false;
							RoomHandler absoluteRoomFromPosition = GetAbsoluteRoomFromPosition(new IntVector2(i, j));
							foreach (RoomHandler connectedRoom in absoluteRoomFromPosition.connectedRooms)
							{
								RuntimeExitDefinition exitDefinitionForConnectedRoom = absoluteRoomFromPosition.GetExitDefinitionForConnectedRoom(connectedRoom);
								if (((exitDefinitionForConnectedRoom.upstreamExit != null && exitDefinitionForConnectedRoom.upstreamExit.isWarpWingStart) || (exitDefinitionForConnectedRoom.downstreamExit != null && exitDefinitionForConnectedRoom.downstreamExit.isWarpWingStart)) && exitDefinitionForConnectedRoom.ContainsPosition(new IntVector2(i, j)))
								{
									flag2 = true;
									break;
								}
							}
							if (flag2)
							{
								continue;
							}
						}
						int num2 = 0;
						int num3 = 0;
						for (int k = 0; k < 4; k++)
						{
							CellData cellData = this[new IntVector2(i, j) + IntVector2.Cardinals[k]];
							if (cellData.type != CellType.WALL)
							{
								num2++;
								CellData cellData2 = this[new IntVector2(i, j) + 2 * IntVector2.Cardinals[k]];
								if (cellData2.type != CellType.WALL)
								{
									num3++;
								}
							}
						}
						if (num2 == 2 && num3 != 2)
						{
							list.Add(this.cellData[i][j]);
						}
					}
				}
				if (list.Count > 0)
				{
					flag = true;
				}
				foreach (CellData item in list)
				{
					BraveUtility.DrawDebugSquare(item.position.ToVector2(), Color.yellow, 1000f);
					item.type = CellType.WALL;
					for (int l = 0; l < rooms.Count; l++)
					{
						RoomHandler roomHandler = rooms[l];
						roomHandler.RawCells.Remove(item.position);
						roomHandler.Cells.Remove(item.position);
						roomHandler.CellsWithoutExits.Remove(item.position);
						if (roomHandler.area.instanceUsedExits == null)
						{
							continue;
						}
						for (int m = 0; m < roomHandler.area.instanceUsedExits.Count; m++)
						{
							if (roomHandler.area.exitToLocalDataMap.ContainsKey(roomHandler.area.instanceUsedExits[m]))
							{
								RuntimeExitDefinition runtimeExitDefinition = roomHandler.exitDefinitionsByExit[roomHandler.area.exitToLocalDataMap[roomHandler.area.instanceUsedExits[m]]];
								runtimeExitDefinition.RemovePosition(item.position);
							}
						}
					}
				}
			}
		}

		public int GetRoomVisualTypeAtPosition(Vector2 position)
		{
			return GetRoomVisualTypeAtPosition(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
		}

		public int GetRoomVisualTypeAtPosition(IntVector2 position)
		{
			return this[position].cellVisualData.roomVisualTypeIndex;
		}

		public int GetRoomVisualTypeAtPosition(int x, int y)
		{
			if (x < 0 || x >= Width || y < 0 || y >= Height || cellData[x][y] == null)
			{
				return 0;
			}
			return cellData[x][y].cellVisualData.roomVisualTypeIndex;
		}

		public void FakeRegisterDoorFeet(IntVector2 position, bool northSouth)
		{
			if (northSouth)
			{
				this[position].cellVisualData.doorFeetOverrideMode = 1;
				this[position + IntVector2.Right].cellVisualData.doorFeetOverrideMode = 1;
			}
			else
			{
				this[position].cellVisualData.doorFeetOverrideMode = 2;
				this[position + IntVector2.Up].cellVisualData.doorFeetOverrideMode = 2;
			}
		}

		public void RegisterDoor(IntVector2 position, DungeonDoorController door, IntVector2 subsidiaryDoorPosition)
		{
			if (doors == null)
			{
				doors = new Dictionary<IntVector2, DungeonDoorController>(new IntVector2EqualityComparer());
			}
			if (doors.ContainsKey(position))
			{
				return;
			}
			doors.Add(position, door);
			this[position].isDoorFrameCell = true;
			if (door.northSouth)
			{
				doors.Add(position + IntVector2.Right, door);
				this[position + IntVector2.Right].isDoorFrameCell = true;
				this[position + IntVector2.Up].isDoorFrameCell = true;
				this[position + IntVector2.UpRight].isDoorFrameCell = true;
				this[position].isExitNonOccluder = true;
				this[position + IntVector2.Right].isExitNonOccluder = true;
			}
			else
			{
				doors.Add(position + IntVector2.Up, door);
				this[position + IntVector2.Up].isDoorFrameCell = true;
				for (int i = -2; i < 3; i++)
				{
					this[position + new IntVector2(i, 1)].isExitNonOccluder = true;
					if (Math.Abs(i) < 2)
					{
						this[position + IntVector2.Right * i].isExitNonOccluder = true;
						this[position + new IntVector2(i, 2)].isExitNonOccluder = true;
					}
				}
			}
			if (door.subsidiaryDoor != null)
			{
				doors.Add(subsidiaryDoorPosition, door.subsidiaryDoor);
				this[subsidiaryDoorPosition].isDoorFrameCell = true;
				if (door.subsidiaryDoor.northSouth)
				{
					doors.Add(subsidiaryDoorPosition + IntVector2.Right, door.subsidiaryDoor);
					this[subsidiaryDoorPosition + IntVector2.Right].isDoorFrameCell = true;
					this[subsidiaryDoorPosition + IntVector2.Up].isDoorFrameCell = true;
					this[subsidiaryDoorPosition + IntVector2.UpRight].isDoorFrameCell = true;
				}
				else
				{
					doors.Add(subsidiaryDoorPosition + IntVector2.Up, door.subsidiaryDoor);
					this[subsidiaryDoorPosition + IntVector2.Up].isDoorFrameCell = true;
				}
			}
		}

		public DungeonDoorController GetDoorAtPosition(IntVector2 position)
		{
			if (doors == null)
			{
				return null;
			}
			if (!doors.ContainsKey(position))
			{
				return null;
			}
			return doors[position];
		}

		public bool HasDoorAtPosition(IntVector2 position)
		{
			if (doors == null)
			{
				return false;
			}
			return doors.ContainsKey(position);
		}

		public void DestroyDoorAtPosition(IntVector2 position)
		{
			DungeonDoorController dungeonDoorController = doors[position];
			doors.Remove(position);
			if (dungeonDoorController.northSouth)
			{
				doors.Remove(position + IntVector2.Right);
			}
			else
			{
				doors.Remove(position + IntVector2.Up);
			}
			UnityEngine.Object.Destroy(dungeonDoorController.gameObject);
		}

		public List<CellData> GetCellNeighbors(CellData d, bool getDiagonals = false)
		{
			if (getDiagonals)
			{
			}
			s_neighborsList.Clear();
			int num = (getDiagonals ? 1 : 2);
			for (int i = 0; i < 8; i += num)
			{
				s_neighborsList.Add(GetCellInDirection(d, (Direction)i));
			}
			return s_neighborsList;
		}

		public bool CheckLineForCellType(IntVector2 p1, IntVector2 p2, CellType t)
		{
			List<CellData> linearCells = GetLinearCells(p1, p2);
			for (int i = 0; i < linearCells.Count; i++)
			{
				if (linearCells[i].type == t)
				{
					return true;
				}
			}
			return false;
		}

		public List<CellData> GetLinearCells(IntVector2 p1, IntVector2 p2)
		{
			HashSet<CellData> hashSet = new HashSet<CellData>();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = p1.y;
			int num5 = p1.x;
			int num6 = 0;
			int num7 = 0;
			int num8 = p2.x - p1.x;
			int num9 = p2.y - p1.y;
			hashSet.Add(cellData[num5][num4]);
			if (num9 < 0)
			{
				num3 = -1;
				num9 = -num9;
			}
			else
			{
				num3 = 1;
			}
			if (num8 < 0)
			{
				num2 = -1;
				num8 = -num8;
			}
			else
			{
				num2 = 1;
			}
			num6 = 2 * num9;
			num7 = 2 * num8;
			if (num7 >= num6)
			{
				int num10 = num8;
				int num11 = num8;
				for (num = 0; num < num8; num++)
				{
					num5 += num2;
					num10 += num6;
					if (num10 > num7)
					{
						num4 += num3;
						num10 -= num7;
						if (num10 + num11 < num7)
						{
							hashSet.Add(cellData[num5][num4 - num3]);
						}
						else if (num10 + num11 > num7)
						{
							hashSet.Add(cellData[num5 - num2][num4]);
						}
						else
						{
							hashSet.Add(cellData[num5][num4 - num3]);
							hashSet.Add(cellData[num5 - num2][num4]);
						}
					}
					hashSet.Add(cellData[num5][num4]);
					num11 = num10;
				}
			}
			else
			{
				int num10 = num9;
				int num11 = num10;
				for (num = 0; num < num9; num++)
				{
					num4 += num3;
					num10 += num7;
					if (num10 > num6)
					{
						num5 += num2;
						num10 -= num6;
						if (num10 + num11 < num6)
						{
							hashSet.Add(cellData[num5 - num2][num4]);
						}
						else if (num10 + num11 > num6)
						{
							hashSet.Add(cellData[num5][num4 - num3]);
						}
						else
						{
							hashSet.Add(cellData[num5 - num2][num4]);
							hashSet.Add(cellData[num5][num4 - num3]);
						}
					}
					hashSet.Add(cellData[num5][num4]);
					num11 = num10;
				}
			}
			return new List<CellData>(hashSet);
		}

		public CellData GetCellInDirection(CellData d, Direction dir)
		{
			IntVector2 position = d.position;
			switch (dir)
			{
			case Direction.NORTH:
				position += IntVector2.Up;
				break;
			case Direction.NORTHEAST:
				position += IntVector2.Up + IntVector2.Right;
				break;
			case Direction.EAST:
				position += IntVector2.Right;
				break;
			case Direction.SOUTHEAST:
				position += IntVector2.Right + IntVector2.Down;
				break;
			case Direction.SOUTH:
				position += IntVector2.Down;
				break;
			case Direction.SOUTHWEST:
				position += IntVector2.Down + IntVector2.Left;
				break;
			case Direction.WEST:
				position += IntVector2.Left;
				break;
			case Direction.NORTHWEST:
				position += IntVector2.Left + IntVector2.Up;
				break;
			default:
				Debug.LogError("Switching on invalid direction in GetCellInDirection: " + dir);
				break;
			}
			return (!CheckInBounds(position)) ? null : cellData[position.x][position.y];
		}

		public bool CheckInBounds(int x, int y)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
			{
				return true;
			}
			return false;
		}

		public bool CheckInBounds(IntVector2 vec)
		{
			if (vec.x >= 0 && vec.x < Width && vec.y >= 0 && vec.y < Height)
			{
				return true;
			}
			return false;
		}

		public bool CheckInBoundsAndValid(int x, int y)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
			{
				return this[x, y] != null;
			}
			return false;
		}

		public bool CheckInBoundsAndValid(IntVector2 vec)
		{
			if (vec.x >= 0 && vec.x < Width && vec.y >= 0 && vec.y < Height)
			{
				return this[vec] != null;
			}
			return false;
		}

		public bool CheckInBounds(IntVector2 vec, int distanceThresh)
		{
			if (vec.x >= distanceThresh && vec.x < Width - distanceThresh && vec.y >= distanceThresh && vec.y < Height - distanceThresh)
			{
				return true;
			}
			return false;
		}

		public void DistributeComplexSecretPuzzleItems(List<PickupObject> requiredObjects, RoomHandler puzzleRoom, bool preferSignatureEnemies = false, float preferBossesChance = 0f)
		{
			int i = 0;
			bool flag = UnityEngine.Random.value < preferBossesChance;
			List<AIActor> list = new List<AIActor>();
			List<AIActor> list2 = new List<AIActor>();
			List<AIActor> list3 = new List<AIActor>();
			for (int j = 0; j < StaticReferenceManager.AllEnemies.Count; j++)
			{
				AIActor aIActor = StaticReferenceManager.AllEnemies[j];
				if (!aIActor.IsNormalEnemy || aIActor.IsHarmlessEnemy || aIActor.IsInReinforcementLayer || !aIActor.healthHaver || (puzzleRoom != null && aIActor.ParentRoom == puzzleRoom))
				{
					continue;
				}
				if (aIActor.healthHaver.IsBoss)
				{
					list3.Add(aIActor);
					continue;
				}
				list.Add(StaticReferenceManager.AllEnemies[j]);
				if (StaticReferenceManager.AllEnemies[j].IsSignatureEnemy && preferSignatureEnemies)
				{
					list2.Add(StaticReferenceManager.AllEnemies[j]);
				}
			}
			AIActor aIActor2 = null;
			for (; i < requiredObjects.Count; i++)
			{
				if (flag && list3.Count > 0)
				{
					aIActor2 = list3[UnityEngine.Random.Range(0, list3.Count)];
				}
				else if (list2.Count > 0)
				{
					aIActor2 = list2[UnityEngine.Random.Range(0, list2.Count)];
				}
				else if (list.Count > 0)
				{
					aIActor2 = list[UnityEngine.Random.Range(0, list.Count)];
				}
				if (aIActor2 != null)
				{
					aIActor2.AdditionalSimpleItemDrops.Add(requiredObjects[i]);
					if (requiredObjects[i] is RobotArmBalloonsItem)
					{
						RobotArmBalloonsItem robotArmBalloonsItem = requiredObjects[i] as RobotArmBalloonsItem;
						robotArmBalloonsItem.AttachBalloonToGameActor(aIActor2);
					}
					list3.Remove(aIActor2);
					list2.Remove(aIActor2);
					list.Remove(aIActor2);
					continue;
				}
				Debug.LogError("Failed to attach an item to any enemy!");
				break;
			}
		}

		public void SolidifyLavaInRadius(Vector2 position, float radius)
		{
			int num = Mathf.CeilToInt(radius);
			IntVector2 intVector = position.ToIntVector2(VectorConversions.Floor) + new IntVector2(-num, -num);
			IntVector2 intVector2 = position.ToIntVector2(VectorConversions.Ceil) + new IntVector2(num, num);
			for (int i = intVector.x; i <= intVector2.x; i++)
			{
				for (int j = intVector.y; j <= intVector2.y; j++)
				{
					Vector2 b = new Vector2((float)i + 0.5f, (float)j + 0.5f);
					float num2 = Vector2.Distance(position, b);
					if (num2 <= radius)
					{
						SolidifyLavaInCell(new IntVector2(i, j));
					}
				}
			}
		}

		private void InitializeSizzleSystem()
		{
			GameObject gameObject = GameObject.Find("Gungeon_Sizzle_Main");
			if (gameObject == null)
			{
				gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Particles/Gungeon_Sizzle_Main"), Vector3.zero, Quaternion.identity);
				gameObject.name = "Gungeon_Sizzle_Main";
			}
			m_sizzleSystem = gameObject.GetComponent<ParticleSystem>();
		}

		private void SpawnWorldParticle(ParticleSystem system, Vector3 worldPos)
		{
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = worldPos;
			emitParams.velocity = Vector3.zero;
			emitParams.startSize = system.startSize;
			emitParams.startLifetime = system.startLifetime;
			emitParams.startColor = system.startColor;
			emitParams.randomSeed = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
			system.Emit(emitParams, 1);
		}

		public void SolidifyLavaInCell(IntVector2 cellPosition)
		{
			if (!CheckInBounds(cellPosition))
			{
				return;
			}
			CellData cellData = this[cellPosition];
			if (cellData != null && cellData.doesDamage)
			{
				cellData.doesDamage = false;
				if (m_sizzleSystem == null)
				{
					InitializeSizzleSystem();
				}
				SpawnWorldParticle(m_sizzleSystem, cellPosition.ToCenterVector3(cellPosition.y) + UnityEngine.Random.insideUnitCircle.ToVector3ZUp() / 3f);
				if (UnityEngine.Random.value < 0.5f)
				{
					SpawnWorldParticle(m_sizzleSystem, cellPosition.ToCenterVector3(cellPosition.y) + UnityEngine.Random.insideUnitCircle.ToVector3ZUp() / 3f);
				}
			}
		}

		public void TriggerFloorAnimationsInCell(IntVector2 cellPosition)
		{
			if (!CheckInBounds(cellPosition))
			{
				return;
			}
			CellData cellData = this[cellPosition];
			if (cellData != null && cellData.type == CellType.FLOOR && TK2DTilemapChunkAnimator.PositionToAnimatorMap.ContainsKey(cellPosition))
			{
				for (int i = 0; i < TK2DTilemapChunkAnimator.PositionToAnimatorMap[cellPosition].Count; i++)
				{
					TK2DTilemapChunkAnimator.PositionToAnimatorMap[cellPosition][i].TriggerAnimationSequence();
				}
			}
		}

		public void UntriggerFloorAnimationsInCell(IntVector2 cellPosition)
		{
			if (!CheckInBounds(cellPosition))
			{
				return;
			}
			CellData cellData = this[cellPosition];
			if (cellData != null && cellData.type == CellType.FLOOR && TK2DTilemapChunkAnimator.PositionToAnimatorMap.ContainsKey(cellPosition))
			{
				for (int i = 0; i < TK2DTilemapChunkAnimator.PositionToAnimatorMap[cellPosition].Count; i++)
				{
					TK2DTilemapChunkAnimator.PositionToAnimatorMap[cellPosition][i].UntriggerAnimationSequence();
				}
			}
		}

		public void GenerateLocksAndKeys(RoomHandler start, RoomHandler exit)
		{
			ReducedDungeonGraph graph = new ReducedDungeonGraph();
			PlaceSingleLockAndKey(graph);
		}

		public void PlaceSingleLockAndKey(ReducedDungeonGraph graph)
		{
		}

		public void GetObjectOcclusionAndObstruction(Vector2 sourcePoint, Vector2 listenerPoint, out float occlusion, out float obstruction)
		{
			occlusion = 0f;
			obstruction = 0f;
			IntVector2 intVector = sourcePoint.ToIntVector2(VectorConversions.Floor);
			IntVector2 intVector2 = listenerPoint.ToIntVector2(VectorConversions.Floor);
			if (CheckInBounds(intVector) && CheckInBounds(intVector2))
			{
				RoomHandler absoluteRoomFromPosition = GetAbsoluteRoomFromPosition(intVector);
				RoomHandler absoluteRoomFromPosition2 = GetAbsoluteRoomFromPosition(intVector2);
				if (absoluteRoomFromPosition != null && absoluteRoomFromPosition2 != null && absoluteRoomFromPosition != absoluteRoomFromPosition2)
				{
					occlusion = 0.5f;
					obstruction = 0.5f;
				}
			}
		}

		public bool isDeadEnd(int x, int y)
		{
			int num = 0;
			if (CheckInBounds(x, y - 1) && cellData[x][y - 1].type == CellType.FLOOR)
			{
				num++;
			}
			if (CheckInBounds(x + 1, y) && cellData[x + 1][y].type == CellType.FLOOR)
			{
				num++;
			}
			if (CheckInBounds(x, y + 1) && cellData[x][y + 1].type == CellType.FLOOR)
			{
				num++;
			}
			if (CheckInBounds(x - 1, y) && cellData[x - 1][y].type == CellType.FLOOR)
			{
				num++;
			}
			if (num > 1)
			{
				return false;
			}
			return true;
		}

		public bool isSingleCellWall(int x, int y)
		{
			if (!CheckInBounds(x, y) || !CheckInBounds(x, y - 1) || !CheckInBounds(x, y + 1) || cellData[x][y] == null || cellData[x][y - 1] == null || cellData[x][y + 1] == null)
			{
				return false;
			}
			if (cellData[x][y].type == CellType.WALL && cellData[x][y - 1].type != CellType.WALL && cellData[x][y + 1].type != CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isTopDiagonalWall(int x, int y)
		{
			return isFaceWallHigher(x, y - 1) && (cellData[x][y].diagonalWallType == DiagonalWallType.NORTHEAST || cellData[x][y].diagonalWallType == DiagonalWallType.NORTHWEST);
		}

		public bool isAnyFaceWall(int x, int y)
		{
			if (isFaceWallLower(x, y) || isFaceWallHigher(x, y))
			{
				return true;
			}
			return false;
		}

		public bool isFaceWallLower(int x, int y)
		{
			if (!CheckInBoundsAndValid(x, y) || !CheckInBoundsAndValid(x, y - 1))
			{
				return false;
			}
			if (cellData[x][y].type == CellType.WALL && cellData[x][y - 1].type != CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isFaceWallHigher(int x, int y)
		{
			if (!CheckInBoundsAndValid(x, y) || !CheckInBoundsAndValid(x, y - 1) || !CheckInBoundsAndValid(x, y - 2))
			{
				return false;
			}
			if (cellData[x][y].type == CellType.WALL && cellData[x][y - 1].type == CellType.WALL && cellData[x][y - 2].type != CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isPlainEmptyCell(int x, int y)
		{
			if (!CheckInBounds(x, y))
			{
				return false;
			}
			CellData cellData = this.cellData[x][y];
			if (cellData == null)
			{
				return false;
			}
			if (cellData.isExitCell)
			{
				return false;
			}
			if (isTopWall(x, y))
			{
				return false;
			}
			return !cellData.isOccupied && cellData.IsPassable && !cellData.containsTrap && !cellData.IsTrapZone && !cellData.PreventRewardSpawn && !cellData.doesDamage && (!cellData.cellVisualData.hasStampedPath & !cellData.forceDisallowGoop);
		}

		public bool isWallUp(int x, int y)
		{
			if (!CheckInBoundsAndValid(x, y + 1))
			{
				return false;
			}
			if (cellData[x][y + 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isWall(int x, int y)
		{
			if (!CheckInBounds(x, y))
			{
				return false;
			}
			return cellData[x][y] == null || cellData[x][y].type == CellType.WALL;
		}

		public bool isWall(IntVector2 pos)
		{
			if (!CheckInBounds(pos))
			{
				return false;
			}
			return cellData[pos.x][pos.y] == null || cellData[pos.x][pos.y].type == CellType.WALL;
		}

		public bool isWallRight(int x, int y)
		{
			if (!CheckInBoundsAndValid(x + 1, y))
			{
				return false;
			}
			if (cellData[x + 1][y].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isWallLeft(int x, int y)
		{
			if (!CheckInBoundsAndValid(x - 1, y))
			{
				return false;
			}
			if (cellData[x - 1][y].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isWallUpRight(int x, int y)
		{
			if (!CheckInBoundsAndValid(x + 1, y + 1))
			{
				return false;
			}
			if (cellData[x + 1][y + 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isWallUpLeft(int x, int y)
		{
			if (!CheckInBoundsAndValid(x - 1, y + 1))
			{
				return false;
			}
			if (cellData[x - 1][y + 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isWallDownRight(int x, int y)
		{
			if (!CheckInBoundsAndValid(x + 1, y - 1))
			{
				return false;
			}
			if (cellData[x + 1][y - 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isWallDownLeft(int x, int y)
		{
			if (!CheckInBoundsAndValid(x - 1, y - 1))
			{
				return false;
			}
			if (cellData[x - 1][y - 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isFaceWallRight(int x, int y)
		{
			return isAnyFaceWall(x + 1, y);
		}

		public bool isFaceWallLeft(int x, int y)
		{
			return isAnyFaceWall(x - 1, y);
		}

		public bool isShadowFloor(int x, int y)
		{
			if (!CheckInBoundsAndValid(x, y) || !CheckInBoundsAndValid(x, y + 1))
			{
				return false;
			}
			if (cellData[x][y].type == CellType.FLOOR && cellData[x][y + 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isTopWall(int x, int y)
		{
			if (!CheckInBoundsAndValid(x, y) || !CheckInBoundsAndValid(x, y - 1))
			{
				return false;
			}
			if (cellData[x][y].type != CellType.WALL && cellData[x][y - 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isLeftTopWall(int x, int y)
		{
			if (!CheckInBoundsAndValid(x - 1, y) || !CheckInBoundsAndValid(x - 1, y - 1))
			{
				return false;
			}
			if (cellData[x - 1][y].type != CellType.WALL && cellData[x - 1][y - 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isRightTopWall(int x, int y)
		{
			if (!CheckInBoundsAndValid(x + 1, y) || !CheckInBoundsAndValid(x + 1, y - 1))
			{
				return false;
			}
			if (cellData[x + 1][y].type != CellType.WALL && cellData[x + 1][y - 1].type == CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool hasTopWall(int x, int y)
		{
			if (!CheckInBoundsAndValid(x, y) || !CheckInBoundsAndValid(x, y + 1))
			{
				return false;
			}
			if (cellData[x][y].type == CellType.WALL && cellData[x][y + 1].type != CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool leftHasTopWall(int x, int y)
		{
			if (!CheckInBoundsAndValid(x - 1, y) || !CheckInBoundsAndValid(x - 1, y + 1))
			{
				return false;
			}
			if (cellData[x - 1][y].type == CellType.WALL && cellData[x - 1][y + 1].type != CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool rightHasTopWall(int x, int y)
		{
			if (!CheckInBoundsAndValid(x + 1, y) || !CheckInBoundsAndValid(x + 1, y + 1))
			{
				return false;
			}
			if (cellData[x + 1][y].type == CellType.WALL && cellData[x + 1][y + 1].type != CellType.WALL)
			{
				return true;
			}
			return false;
		}

		public bool isPit(int x, int y)
		{
			if (!CheckInBoundsAndValid(x, y))
			{
				return false;
			}
			CellData cellData = this.cellData[x][y];
			return cellData != null && cellData.type == CellType.PIT;
		}

		public bool isLeftSideWall(int x, int y)
		{
			return isWall(x, y) && !isWall(x + 1, y);
		}

		public bool isRightSideWall(int x, int y)
		{
			return isWall(x, y) && !isWall(x - 1, y);
		}
	}
}
