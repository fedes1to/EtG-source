using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RobotDave
{
	public enum RobotFeatureType
	{
		NONE = 0,
		FLAT_EXPANSE = 5,
		COLUMN_SAWBLADE = 10,
		PIT_BORDER = 15,
		TRAP_PLUS = 20,
		TRAP_SQUARE = 25,
		CORNER_COLUMNS = 30,
		PIT_INNER = 35,
		TABLES_EDGE = 40,
		ROLLING_LOG_VERTICAL = 45,
		ROLLING_LOG_HORIZONTAL = 50,
		CASTLE_CHANDELIER = 60,
		MINES_CAVE_IN = 70,
		MINES_SQUARE_CART = 75,
		MINES_DOUBLE_CART = 76,
		MINES_TURRET_CART = 77,
		CONVEYOR_HORIZONTAL = 110,
		CONVEYOR_VERTICAL = 115
	}

	private static DungeonPlaceable m_trapData;

	private static DungeonPlaceable m_horizontalTable;

	private static DungeonPlaceable m_verticalTable;

	public static RobotRoomFeature GetFeatureFromType(RobotFeatureType type)
	{
		switch (type)
		{
		case RobotFeatureType.FLAT_EXPANSE:
			return new FlatExpanseFeature();
		case RobotFeatureType.COLUMN_SAWBLADE:
			return new RobotRoomColumnFeature();
		case RobotFeatureType.PIT_BORDER:
			return new RobotRoomSurroundingPitFeature();
		case RobotFeatureType.TRAP_PLUS:
			return new RobotRoomTrapPlusFeature();
		case RobotFeatureType.TRAP_SQUARE:
			return new RobotRoomTrapSquareFeature();
		case RobotFeatureType.CORNER_COLUMNS:
			return new RobotRoomCornerColumnsFeature();
		case RobotFeatureType.PIT_INNER:
			return new RobotRoomInnerPitFeature();
		case RobotFeatureType.TABLES_EDGE:
			return new RobotRoomTablesFeature();
		case RobotFeatureType.ROLLING_LOG_VERTICAL:
			return new RobotRoomRollingLogsVerticalFeature();
		case RobotFeatureType.ROLLING_LOG_HORIZONTAL:
			return new RobotRoomRollingLogsHorizontalFeature();
		case RobotFeatureType.CASTLE_CHANDELIER:
			return new RobotRoomChandelierFeature();
		case RobotFeatureType.MINES_CAVE_IN:
			return new RobotRoomCaveInFeature();
		case RobotFeatureType.MINES_SQUARE_CART:
			return new RobotRoomMineCartSquareFeature();
		case RobotFeatureType.MINES_DOUBLE_CART:
			return new RobotRoomMineCartSquareDoubleFeature();
		case RobotFeatureType.MINES_TURRET_CART:
			return new RobotRoomMineCartSquareTurretFeature();
		case RobotFeatureType.CONVEYOR_HORIZONTAL:
			return new RobotRoomConveyorHorizontalFeature();
		case RobotFeatureType.CONVEYOR_VERTICAL:
			return new RobotRoomConveyorVerticalFeature();
		default:
			return new FlatExpanseFeature();
		}
	}

	public static DungeonPlaceableBehaviour GetPitTrap()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[1].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetSpikesTrap()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[2].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetFloorFlameTrap()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[3].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetSawbladePrefab()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[0].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetMineCartTurretPrefab()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[11].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetMineCartPrefab()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[6].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetCaveInPrefab()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[7].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetChandelierPrefab()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[8].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetHorizontalConveyorPrefab()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[9].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetVerticalConveyorPrefab()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		return m_trapData.variantTiers[10].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetRollingLogVertical()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		bool flag = false;
		ResizableCollider component = m_trapData.variantTiers[(!flag) ? 4 : 12].nonDatabasePlaceable.GetComponent<ResizableCollider>();
		if ((bool)component)
		{
			return component;
		}
		return m_trapData.variantTiers[(!flag) ? 4 : 12].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceableBehaviour GetRollingLogHorizontal()
	{
		if (m_trapData == null)
		{
			m_trapData = (DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset");
		}
		bool flag = false;
		ResizableCollider component = m_trapData.variantTiers[(!flag) ? 5 : 13].nonDatabasePlaceable.GetComponent<ResizableCollider>();
		if ((bool)component)
		{
			return component;
		}
		return m_trapData.variantTiers[(!flag) ? 5 : 13].nonDatabasePlaceable.GetComponent<DungeonPlaceableBehaviour>();
	}

	public static DungeonPlaceable GetHorizontalTable()
	{
		if (m_horizontalTable == null)
		{
			m_horizontalTable = (DungeonPlaceable)BraveResources.Load("RobotTableHorizontal", ".asset");
		}
		return m_horizontalTable;
	}

	public static DungeonPlaceable GetVerticalTable()
	{
		if (m_verticalTable == null)
		{
			m_verticalTable = (DungeonPlaceable)BraveResources.Load("RobotTableVertical", ".asset");
		}
		return m_verticalTable;
	}

	protected static void ResetForNewProcess()
	{
		m_trapData = null;
		m_horizontalTable = null;
		m_verticalTable = null;
		RobotRoomSurroundingPitFeature.BeenUsed = false;
	}

	public static void ApplyFeatureToDwarfRegion(PrototypeDungeonRoom extantRoom, IntVector2 basePosition, IntVector2 dimensions, RobotDaveIdea idea, RobotFeatureType specificFeature, int targetObjectLayer)
	{
		ResetForNewProcess();
		ClearDataForRegion(extantRoom, idea, basePosition, dimensions, targetObjectLayer);
		RobotRoomFeature robotRoomFeature = ((specificFeature == RobotFeatureType.NONE) ? SelectFeatureForZone(idea, basePosition, dimensions, false, 1) : GetFeatureFromType(specificFeature));
		robotRoomFeature.LocalBasePosition = basePosition;
		robotRoomFeature.LocalDimensions = dimensions;
		RobotRoomFeature robotRoomFeature2 = null;
		if (specificFeature == RobotFeatureType.NONE && robotRoomFeature.CanContainOtherFeature())
		{
			IntVector2 intVector = robotRoomFeature.LocalBasePosition + new IntVector2(robotRoomFeature.RequiredInsetForOtherFeature(), robotRoomFeature.RequiredInsetForOtherFeature());
			IntVector2 intVector2 = robotRoomFeature.LocalDimensions - new IntVector2(robotRoomFeature.RequiredInsetForOtherFeature() * 2, robotRoomFeature.RequiredInsetForOtherFeature() * 2);
			robotRoomFeature2 = SelectFeatureForZone(idea, intVector, intVector2, true, 1);
			robotRoomFeature2.LocalBasePosition = intVector;
			robotRoomFeature2.LocalDimensions = intVector2;
		}
		robotRoomFeature.Develop(extantRoom, idea, targetObjectLayer);
		if (robotRoomFeature2 != null)
		{
			robotRoomFeature2.Develop(extantRoom, idea, targetObjectLayer);
		}
	}

	public static void DwarfProcessIdea(PrototypeDungeonRoom extantRoom, RobotDaveIdea idea, IntVector2 desiredDimensions)
	{
		ResetForNewProcess();
		ProcessBasicRoomData(extantRoom, idea, desiredDimensions);
		List<RobotRoomFeature> list = RequestRidiculousNumberOfFeatures(extantRoom, idea, false);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Develop(extantRoom, idea, -1);
		}
		PlaceEnemiesInRoom(extantRoom, idea);
	}

	public static PrototypeDungeonRoom RuntimeProcessIdea(RobotDaveIdea idea, IntVector2 desiredDimensions)
	{
		ResetForNewProcess();
		PrototypeDungeonRoom prototypeDungeonRoom = ScriptableObject.CreateInstance<PrototypeDungeonRoom>();
		ProcessBasicRoomData(prototypeDungeonRoom, idea, desiredDimensions);
		List<RobotRoomFeature> list = RequestRidiculousNumberOfFeatures(prototypeDungeonRoom, idea, true);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Develop(prototypeDungeonRoom, idea, -1);
		}
		PlaceEnemiesInRoom(prototypeDungeonRoom, idea);
		prototypeDungeonRoom.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.SEAL_ROOM));
		prototypeDungeonRoom.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.UNSEAL_ROOM));
		return prototypeDungeonRoom;
	}

	protected static PrototypePlacedObjectData PlacePlaceable(DungeonPlaceable item, PrototypeDungeonRoom room, IntVector2 position)
	{
		if (item == null || room == null)
		{
			return null;
		}
		if (room.CheckRegionOccupiedExcludeWallsAndPits(position.x, position.y, item.GetWidth(), item.GetHeight(), false))
		{
			return null;
		}
		Vector2 vector = position.ToVector2();
		PrototypePlacedObjectData prototypePlacedObjectData = new PrototypePlacedObjectData();
		prototypePlacedObjectData.fieldData = new List<PrototypePlacedObjectFieldData>();
		prototypePlacedObjectData.instancePrerequisites = new DungeonPrerequisite[0];
		prototypePlacedObjectData.placeableContents = item;
		prototypePlacedObjectData.contentsBasePosition = vector;
		int count = room.placedObjects.Count;
		room.placedObjects.Add(prototypePlacedObjectData);
		room.placedObjectPositions.Add(vector);
		for (int i = 0; i < item.GetWidth(); i++)
		{
			for (int j = 0; j < item.GetHeight(); j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(position.x + i, position.y + j);
				prototypeDungeonRoomCellData.placedObjectRUBELIndex = count;
			}
		}
		return prototypePlacedObjectData;
	}

	private static void PlaceEnemiesInRoom(PrototypeDungeonRoom room, RobotDaveIdea idea)
	{
		if (idea.ValidEasyEnemyPlaceables == null || idea.ValidEasyEnemyPlaceables.Length == 0)
		{
			return;
		}
		int num = room.Width * room.Height;
		int value = Mathf.CeilToInt((float)num / 45f);
		value = Mathf.Clamp(value, 1, 6);
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
		{
			value = Mathf.Min(value, 3);
			if (Random.value < 0.1f)
			{
				return;
			}
		}
		int num2 = 0;
		if (value > 3 && idea.ValidHardEnemyPlaceables != null && idea.ValidHardEnemyPlaceables.Length > 0 && Random.value < 0.5f)
		{
			num2++;
			value -= 2;
		}
		if (value > 3)
		{
			value = Random.Range(3, value + 1);
		}
		int num3 = Mathf.FloorToInt((float)room.Width / 5f);
		int num4 = Mathf.FloorToInt((float)room.Height / 5f);
		int num5 = Mathf.CeilToInt((float)num3 / 2f);
		int num6 = Mathf.CeilToInt((float)num4 / 2f);
		List<IntVector2> list = new List<IntVector2>();
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					int num7 = Random.Range(-num5, num5 + 1);
					int num8 = Random.Range(-num6, num6 + 1);
					list.Add(new IntVector2(num3 * (i + 1) + num7, num4 * (j + 1) + num8));
				}
			}
		}
		list = list.GenerationShuffle();
		for (int l = 0; l < value; l++)
		{
			DungeonPlaceable item = idea.ValidEasyEnemyPlaceables[Random.Range(0, idea.ValidEasyEnemyPlaceables.Length)];
			for (int m = 0; m < list.Count; m++)
			{
				PrototypePlacedObjectData prototypePlacedObjectData = PlacePlaceable(item, room, list[m]);
				if (prototypePlacedObjectData != null)
				{
					break;
				}
			}
		}
		for (int n = 0; n < num2; n++)
		{
			DungeonPlaceable item2 = idea.ValidHardEnemyPlaceables[Random.Range(0, idea.ValidHardEnemyPlaceables.Length)];
			for (int num9 = 0; num9 < list.Count; num9++)
			{
				PrototypePlacedObjectData prototypePlacedObjectData2 = PlacePlaceable(item2, room, list[num9]);
				if (prototypePlacedObjectData2 != null)
				{
					break;
				}
			}
		}
	}

	private static RobotRoomFeature SelectFeatureForZone(RobotDaveIdea idea, IntVector2 basePos, IntVector2 dim, bool isInternal, int numFeatures)
	{
		List<RobotRoomFeature> list = new List<RobotRoomFeature>();
		FlatExpanseFeature flatExpanseFeature = new FlatExpanseFeature();
		if (flatExpanseFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(flatExpanseFeature);
		}
		RobotRoomColumnFeature robotRoomColumnFeature = new RobotRoomColumnFeature();
		if (robotRoomColumnFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomColumnFeature);
		}
		RobotRoomSurroundingPitFeature robotRoomSurroundingPitFeature = new RobotRoomSurroundingPitFeature();
		if (robotRoomSurroundingPitFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomSurroundingPitFeature);
		}
		RobotRoomTrapPlusFeature robotRoomTrapPlusFeature = new RobotRoomTrapPlusFeature();
		if (robotRoomTrapPlusFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomTrapPlusFeature);
		}
		RobotRoomTrapSquareFeature robotRoomTrapSquareFeature = new RobotRoomTrapSquareFeature();
		if (robotRoomTrapSquareFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomTrapSquareFeature);
		}
		RobotRoomCornerColumnsFeature robotRoomCornerColumnsFeature = new RobotRoomCornerColumnsFeature();
		if (robotRoomCornerColumnsFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomCornerColumnsFeature);
		}
		RobotRoomInnerPitFeature robotRoomInnerPitFeature = new RobotRoomInnerPitFeature();
		if (robotRoomInnerPitFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomInnerPitFeature);
		}
		RobotRoomTablesFeature robotRoomTablesFeature = new RobotRoomTablesFeature();
		if (robotRoomTablesFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomTablesFeature);
		}
		RobotRoomRollingLogsVerticalFeature robotRoomRollingLogsVerticalFeature = new RobotRoomRollingLogsVerticalFeature();
		if (robotRoomRollingLogsVerticalFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomRollingLogsVerticalFeature);
		}
		RobotRoomRollingLogsHorizontalFeature robotRoomRollingLogsHorizontalFeature = new RobotRoomRollingLogsHorizontalFeature();
		if (robotRoomRollingLogsHorizontalFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomRollingLogsHorizontalFeature);
		}
		RobotRoomMineCartSquareFeature robotRoomMineCartSquareFeature = new RobotRoomMineCartSquareFeature();
		if (robotRoomMineCartSquareFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomMineCartSquareFeature);
		}
		RobotRoomCaveInFeature robotRoomCaveInFeature = new RobotRoomCaveInFeature();
		if (robotRoomCaveInFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomCaveInFeature);
		}
		RobotRoomMineCartSquareDoubleFeature robotRoomMineCartSquareDoubleFeature = new RobotRoomMineCartSquareDoubleFeature();
		if (robotRoomMineCartSquareDoubleFeature.AcceptableInIdea(idea, dim, isInternal, numFeatures))
		{
			list.Add(robotRoomMineCartSquareDoubleFeature);
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	private static List<RobotRoomFeature> RequestRidiculousNumberOfFeatures(PrototypeDungeonRoom room, RobotDaveIdea idea, bool isRuntime)
	{
		List<RobotRoomFeature> list = new List<RobotRoomFeature>();
		int num = 1;
		int num2 = room.Width * room.Height;
		if (num2 <= 49)
		{
			num = 1;
		}
		else
		{
			float value = Random.value;
			if (value < 0.5f)
			{
				num = 1;
			}
			else
			{
				float num3 = (float)room.Width / ((float)room.Height * 1f);
				num = ((num2 < 100 || num3 > 1.75f || num3 < 0.6f) ? 2 : ((!(value < 0.75f)) ? 2 : 2));
			}
		}
		List<IntVector2> list2 = new List<IntVector2>();
		List<IntVector2> list3 = new List<IntVector2>();
		switch (num)
		{
		case 1:
			list2.Add(IntVector2.Zero);
			list3.Add(new IntVector2(room.Width, room.Height));
			break;
		case 2:
		{
			float f3 = (float)room.Width / 2f;
			float f4 = (float)room.Height / 2f;
			if (room.Width > room.Height)
			{
				list2.Add(IntVector2.Zero);
				list3.Add(new IntVector2(Mathf.FloorToInt(f3), room.Height));
				list2.Add(new IntVector2(Mathf.FloorToInt(f3), 0));
				list3.Add(new IntVector2(Mathf.CeilToInt(f3), room.Height));
			}
			else
			{
				list2.Add(IntVector2.Zero);
				list3.Add(new IntVector2(room.Width, Mathf.FloorToInt(f4)));
				list2.Add(new IntVector2(0, Mathf.FloorToInt(f4)));
				list3.Add(new IntVector2(room.Width, Mathf.CeilToInt(f4)));
			}
			break;
		}
		case 4:
		{
			float f = (float)room.Width / 2f;
			float f2 = (float)room.Height / 2f;
			bool flag = Random.value < 0.5f;
			int x = ((!flag) ? Mathf.CeilToInt(f) : Mathf.FloorToInt(f));
			int x2 = (flag ? Mathf.CeilToInt(f) : Mathf.FloorToInt(f));
			int y = ((!flag) ? Mathf.CeilToInt(f2) : Mathf.FloorToInt(f2));
			int y2 = (flag ? Mathf.CeilToInt(f2) : Mathf.FloorToInt(f2));
			list2.Add(IntVector2.Zero);
			list3.Add(new IntVector2(x, y));
			list2.Add(new IntVector2(x, 0));
			list3.Add(new IntVector2(x2, y));
			list2.Add(new IntVector2(0, y));
			list3.Add(new IntVector2(x, y2));
			list2.Add(new IntVector2(x, y));
			list3.Add(new IntVector2(x2, y2));
			break;
		}
		}
		for (int i = 0; i < num; i++)
		{
			IntVector2 intVector = list2[i];
			IntVector2 intVector2 = list3[i];
			RobotRoomFeature robotRoomFeature = SelectFeatureForZone(idea, intVector, intVector2, false, num);
			if (robotRoomFeature != null)
			{
				robotRoomFeature.LocalBasePosition = intVector;
				robotRoomFeature.LocalDimensions = intVector2;
				robotRoomFeature.Use();
				list.Add(robotRoomFeature);
			}
		}
		int count = list.Count;
		for (int j = 0; j < count; j++)
		{
			if (list[j].CanContainOtherFeature())
			{
				IntVector2 intVector3 = list[j].LocalBasePosition + new IntVector2(list[j].RequiredInsetForOtherFeature(), list[j].RequiredInsetForOtherFeature());
				IntVector2 intVector4 = list[j].LocalDimensions - new IntVector2(list[j].RequiredInsetForOtherFeature() * 2, list[j].RequiredInsetForOtherFeature() * 2);
				RobotRoomFeature robotRoomFeature2 = SelectFeatureForZone(idea, intVector3, intVector4, true, num);
				if (robotRoomFeature2 != null)
				{
					robotRoomFeature2.LocalBasePosition = intVector3;
					robotRoomFeature2.LocalDimensions = intVector4;
					robotRoomFeature2.Use();
					list.Add(robotRoomFeature2);
				}
			}
		}
		return list;
	}

	private static void ClearDataForRegion(PrototypeDungeonRoom room, RobotDaveIdea idea, IntVector2 basePosition, IntVector2 desiredDimensions, int targetObjectLayer)
	{
		for (int i = basePosition.x; i < basePosition.x + desiredDimensions.x; i++)
		{
			for (int j = basePosition.y; j < basePosition.y + desiredDimensions.y; j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(i, j);
				prototypeDungeonRoomCellData.state = CellType.FLOOR;
				if (targetObjectLayer == -1)
				{
					prototypeDungeonRoomCellData.placedObjectRUBELIndex = -1;
				}
				else if (prototypeDungeonRoomCellData.additionalPlacedObjectIndices.Count > targetObjectLayer)
				{
					prototypeDungeonRoomCellData.additionalPlacedObjectIndices[targetObjectLayer] = -1;
				}
				prototypeDungeonRoomCellData.doesDamage = false;
				prototypeDungeonRoomCellData.damageDefinition = default(CellDamageDefinition);
				prototypeDungeonRoomCellData.appearance = new PrototypeDungeonRoomCellAppearance();
			}
		}
		if (targetObjectLayer == -1)
		{
			for (int k = 0; k < room.placedObjects.Count; k++)
			{
				Vector2 vector = room.placedObjectPositions[k];
				if (vector.x >= (float)basePosition.x && vector.x < (float)(basePosition.x + desiredDimensions.x) && vector.y >= (float)basePosition.y && vector.y < (float)(basePosition.y + desiredDimensions.y))
				{
					if (room.placedObjects[k].assignedPathIDx >= 0)
					{
						room.RemovePathAt(room.placedObjects[k].assignedPathIDx);
					}
					room.placedObjectPositions.RemoveAt(k);
					room.placedObjects.RemoveAt(k);
					k--;
				}
			}
			return;
		}
		PrototypeRoomObjectLayer prototypeRoomObjectLayer = room.additionalObjectLayers[targetObjectLayer];
		for (int l = 0; l < prototypeRoomObjectLayer.placedObjects.Count; l++)
		{
			Vector2 vector2 = prototypeRoomObjectLayer.placedObjectBasePositions[l];
			if (vector2.x >= (float)basePosition.x && vector2.x < (float)(basePosition.x + desiredDimensions.x) && vector2.y >= (float)basePosition.y && vector2.y < (float)(basePosition.y + desiredDimensions.y))
			{
				if (prototypeRoomObjectLayer.placedObjects[l].assignedPathIDx >= 0)
				{
					room.RemovePathAt(prototypeRoomObjectLayer.placedObjects[l].assignedPathIDx);
				}
				prototypeRoomObjectLayer.placedObjectBasePositions.RemoveAt(l);
				prototypeRoomObjectLayer.placedObjects.RemoveAt(l);
				l--;
			}
		}
	}

	private static void ProcessBasicRoomData(PrototypeDungeonRoom room, RobotDaveIdea idea, IntVector2 desiredDimensions)
	{
		room.category = PrototypeDungeonRoom.RoomCategory.NORMAL;
		room.Width = desiredDimensions.x;
		room.Height = desiredDimensions.y;
		for (int i = 0; i < room.Width; i++)
		{
			for (int j = 0; j < room.Height; j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(i, j);
				prototypeDungeonRoomCellData.state = CellType.FLOOR;
				prototypeDungeonRoomCellData.placedObjectRUBELIndex = -1;
				prototypeDungeonRoomCellData.doesDamage = false;
				prototypeDungeonRoomCellData.damageDefinition = default(CellDamageDefinition);
				prototypeDungeonRoomCellData.appearance = new PrototypeDungeonRoomCellAppearance();
			}
		}
		room.exitData = new PrototypeRoomExitData();
		room.pits = new List<PrototypeRoomPitEntry>();
		room.placedObjects = new List<PrototypePlacedObjectData>();
		room.placedObjectPositions = new List<Vector2>();
		room.additionalObjectLayers = new List<PrototypeRoomObjectLayer>();
		room.eventTriggerAreas = new List<PrototypeEventTriggerArea>();
		room.roomEvents = new List<RoomEventDefinition>();
		room.paths = new List<SerializedPath>();
		room.prerequisites = new List<DungeonPrerequisite>();
		room.excludedOtherRooms = new List<PrototypeDungeonRoom>();
		room.rectangularFeatures = new List<PrototypeRectangularFeature>();
	}
}
