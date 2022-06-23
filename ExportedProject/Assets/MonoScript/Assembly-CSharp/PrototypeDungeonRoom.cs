using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PrototypeDungeonRoom : ScriptableObject, ISerializationCallbackReceiver
{
	public enum RoomCategory
	{
		CONNECTOR,
		HUB,
		NORMAL,
		BOSS,
		REWARD,
		SPECIAL,
		SECRET,
		ENTRANCE,
		EXIT
	}

	public enum RoomNormalSubCategory
	{
		COMBAT,
		TRAP
	}

	public enum RoomBossSubCategory
	{
		FLOOR_BOSS,
		MINI_BOSS
	}

	public enum RoomSpecialSubCategory
	{
		UNSPECIFIED_SPECIAL,
		STANDARD_SHOP,
		WEIRD_SHOP,
		MAIN_STORY,
		NPC_STORY,
		CATACOMBS_BRIDGE_ROOM
	}

	public enum RoomSecretSubCategory
	{
		UNSPECIFIED_SECRET
	}

	[HideInInspector]
	public int RoomId = -1;

	[SerializeField]
	public string QAID;

	[SerializeField]
	public string GUID;

	[SerializeField]
	public bool PreventMirroring;

	[NonSerialized]
	public PrototypeDungeonRoom MirrorSource;

	public RoomCategory category = RoomCategory.NORMAL;

	public RoomNormalSubCategory subCategoryNormal;

	public RoomBossSubCategory subCategoryBoss;

	public RoomSpecialSubCategory subCategorySpecial = RoomSpecialSubCategory.STANDARD_SHOP;

	public RoomSecretSubCategory subCategorySecret;

	public PrototypeRoomExitData exitData;

	public List<PrototypeRoomPitEntry> pits;

	public List<PrototypePlacedObjectData> placedObjects;

	public List<Vector2> placedObjectPositions;

	public List<PrototypeRoomObjectLayer> additionalObjectLayers = new List<PrototypeRoomObjectLayer>();

	[NonSerialized]
	public List<PrototypeRoomObjectLayer> runtimeAdditionalObjectLayers;

	public List<PrototypeEventTriggerArea> eventTriggerAreas;

	public List<RoomEventDefinition> roomEvents;

	public List<SerializedPath> paths = new List<SerializedPath>();

	public GlobalDungeonData.ValidTilesets overriddenTilesets;

	public List<DungeonPrerequisite> prerequisites;

	public int RequiredCurseLevel = -1;

	public bool InvalidInCoop;

	public List<PrototypeDungeonRoom> excludedOtherRooms = new List<PrototypeDungeonRoom>();

	public List<PrototypeRectangularFeature> rectangularFeatures = new List<PrototypeRectangularFeature>();

	public bool usesProceduralLighting = true;

	public bool usesProceduralDecoration = true;

	public bool cullProceduralDecorationOnWeakPlatforms;

	public bool allowFloorDecoration = true;

	public bool allowWallDecoration = true;

	public bool preventAddedDecoLayering;

	public bool precludeAllTilemapDrawing;

	public bool drawPrecludedCeilingTiles;

	public bool preventFacewallAO;

	public bool preventBorders;

	public bool usesCustomAmbientLight;

	[ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]
	public Color customAmbientLight = Color.white;

	public bool ForceAllowDuplicates;

	public GameObject doorTopDecorable;

	public SharedInjectionData requiredInjectionData;

	public RuntimeInjectionFlags injectionFlags;

	public bool IsLostWoodsRoom;

	public bool UseCustomMusicState;

	public DungeonFloorMusicController.DungeonMusicState OverrideMusicState = DungeonFloorMusicController.DungeonMusicState.CALM;

	public bool UseCustomMusic;

	public string CustomMusicEvent;

	public bool UseCustomMusicSwitch;

	public string CustomMusicSwitch;

	public int overrideRoomVisualType = -1;

	public bool overrideRoomVisualTypeForSecretRooms;

	public IntVector2 rewardChestSpawnPosition = new IntVector2(-1, -1);

	public GameObject associatedMinimapIcon;

	[SerializeField]
	private int m_width = 5;

	[SerializeField]
	private int m_height = 5;

	[NonSerialized]
	private PrototypeDungeonRoomCellData[] m_cellData;

	[FormerlySerializedAs("m_cellData")]
	[SerializeField]
	private PrototypeDungeonRoomCellData[] m_OLDcellData;

	[SerializeField]
	private CellType[] m_serializedCellType;

	[SerializeField]
	private List<int> m_serializedCellDataIndices;

	[SerializeField]
	private List<PrototypeDungeonRoomCellData> m_serializedCellDataData;

	[SerializeField]
	[HideInInspector]
	private List<IntVector2> m_cachedRepresentationIncFacewalls;

	public bool ContainsEnemies
	{
		get
		{
			if (placedObjects != null)
			{
				for (int i = 0; i < placedObjects.Count; i++)
				{
					PrototypePlacedObjectData prototypePlacedObjectData = placedObjects[i];
					if (prototypePlacedObjectData.placeableContents != null && prototypePlacedObjectData.placeableContents.ContainsEnemy)
					{
						return true;
					}
					if (!string.IsNullOrEmpty(prototypePlacedObjectData.enemyBehaviourGuid))
					{
						EnemyDatabaseEntry entry = EnemyDatabase.GetEntry(prototypePlacedObjectData.enemyBehaviourGuid);
						if (entry != null)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public int MinDifficultyRating
	{
		get
		{
			int num = 0;
			for (int i = 0; i < placedObjects.Count; i++)
			{
				PrototypePlacedObjectData prototypePlacedObjectData = placedObjects[i];
				if (prototypePlacedObjectData == null)
				{
					Debug.LogError("Null object on room: " + base.name);
					continue;
				}
				if (prototypePlacedObjectData.placeableContents != null)
				{
					num += prototypePlacedObjectData.placeableContents.GetMinimumDifficulty();
				}
				if (prototypePlacedObjectData.nonenemyBehaviour != null)
				{
					num += prototypePlacedObjectData.nonenemyBehaviour.GetMinimumDifficulty();
				}
				if (!string.IsNullOrEmpty(prototypePlacedObjectData.enemyBehaviourGuid))
				{
					num = num;
				}
			}
			return num;
		}
	}

	public int MaxDifficultyRating
	{
		get
		{
			int num = 0;
			for (int i = 0; i < placedObjects.Count; i++)
			{
				PrototypePlacedObjectData prototypePlacedObjectData = placedObjects[i];
				if (prototypePlacedObjectData.placeableContents != null)
				{
					num += prototypePlacedObjectData.placeableContents.GetMaximumDifficulty();
				}
				if (prototypePlacedObjectData.nonenemyBehaviour != null)
				{
					num += prototypePlacedObjectData.nonenemyBehaviour.GetMaximumDifficulty();
				}
				if (!string.IsNullOrEmpty(prototypePlacedObjectData.enemyBehaviourGuid))
				{
					num = num;
				}
			}
			return num;
		}
	}

	public PrototypeDungeonRoomCellData[] FullCellData
	{
		get
		{
			return m_cellData;
		}
		set
		{
			m_cellData = value;
		}
	}

	public int Width
	{
		get
		{
			return m_width;
		}
		set
		{
			RecalculateCellDataArray(value, m_height);
			m_width = value;
		}
	}

	public int Height
	{
		get
		{
			return m_height;
		}
		set
		{
			RecalculateCellDataArray(m_width, value);
			m_height = value;
		}
	}

	private static Vector2 MirrorPosition(Vector2 position, PrototypeDungeonRoom room)
	{
		int num = Mathf.RoundToInt(position.x);
		int num2 = room.Width - 1 - num;
		return new Vector2(num2, position.y);
	}

	public static bool GameObjectCanBeMirrored(GameObject g)
	{
		if ((bool)g.GetComponent<ConveyorBelt>() || (bool)g.GetComponent<ForgeFlamePipeController>() || (bool)g.GetComponent<ForgeCrushDoorController>() || (bool)g.GetComponent<ForgeHammerController>() || (bool)g.GetComponentInChildren<ProjectileTrapController>())
		{
			return false;
		}
		return true;
	}

	private static bool CanPlacedObjectBeMirrored(PrototypePlacedObjectData data)
	{
		if (Mathf.Abs(data.xMPxOffset) > 0)
		{
			return false;
		}
		if ((bool)data.placeableContents)
		{
			return data.placeableContents.IsValidMirrorPlaceable();
		}
		if ((bool)data.nonenemyBehaviour)
		{
			return GameObjectCanBeMirrored(data.nonenemyBehaviour.gameObject);
		}
		if ((bool)data.unspecifiedContents)
		{
			return GameObjectCanBeMirrored(data.unspecifiedContents);
		}
		return true;
	}

	public static bool IsValidMirrorTarget(PrototypeDungeonRoom target)
	{
		if (target.category == RoomCategory.BOSS || target.category == RoomCategory.ENTRANCE || target.category == RoomCategory.EXIT || target.category == RoomCategory.REWARD || target.category == RoomCategory.SPECIAL || target.category == RoomCategory.SECRET)
		{
			return false;
		}
		if (target.PreventMirroring || target.IsLostWoodsRoom)
		{
			return false;
		}
		if (target.precludeAllTilemapDrawing || target.drawPrecludedCeilingTiles)
		{
			return false;
		}
		if (target.overriddenTilesets != 0)
		{
			return false;
		}
		if (target.paths.Count > 0)
		{
			return false;
		}
		for (int i = 0; i < target.placedObjects.Count; i++)
		{
			if (!CanPlacedObjectBeMirrored(target.placedObjects[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < target.additionalObjectLayers.Count; j++)
		{
			for (int k = 0; k < target.additionalObjectLayers[j].placedObjects.Count; k++)
			{
				if (!CanPlacedObjectBeMirrored(target.additionalObjectLayers[j].placedObjects[k]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static PrototypeDungeonRoom MirrorRoom(PrototypeDungeonRoom sourceRoom)
	{
		IntVector2 intVector = new IntVector2(sourceRoom.m_width, sourceRoom.m_height);
		PrototypeDungeonRoom prototypeDungeonRoom = ScriptableObject.CreateInstance<PrototypeDungeonRoom>();
		prototypeDungeonRoom.MirrorSource = sourceRoom;
		prototypeDungeonRoom.category = sourceRoom.category;
		prototypeDungeonRoom.subCategoryNormal = sourceRoom.subCategoryNormal;
		prototypeDungeonRoom.subCategoryBoss = sourceRoom.subCategoryBoss;
		prototypeDungeonRoom.subCategorySecret = sourceRoom.subCategorySecret;
		prototypeDungeonRoom.subCategorySpecial = sourceRoom.subCategorySpecial;
		prototypeDungeonRoom.usesProceduralLighting = sourceRoom.usesProceduralLighting;
		prototypeDungeonRoom.usesProceduralDecoration = sourceRoom.usesProceduralDecoration;
		prototypeDungeonRoom.cullProceduralDecorationOnWeakPlatforms = sourceRoom.cullProceduralDecorationOnWeakPlatforms;
		prototypeDungeonRoom.allowFloorDecoration = sourceRoom.allowFloorDecoration;
		prototypeDungeonRoom.allowWallDecoration = sourceRoom.allowWallDecoration;
		prototypeDungeonRoom.preventAddedDecoLayering = sourceRoom.preventAddedDecoLayering;
		prototypeDungeonRoom.precludeAllTilemapDrawing = sourceRoom.precludeAllTilemapDrawing;
		prototypeDungeonRoom.drawPrecludedCeilingTiles = sourceRoom.drawPrecludedCeilingTiles;
		prototypeDungeonRoom.preventFacewallAO = sourceRoom.preventFacewallAO;
		prototypeDungeonRoom.preventBorders = sourceRoom.preventBorders;
		prototypeDungeonRoom.usesCustomAmbientLight = sourceRoom.usesCustomAmbientLight;
		prototypeDungeonRoom.customAmbientLight = sourceRoom.customAmbientLight;
		prototypeDungeonRoom.ForceAllowDuplicates = sourceRoom.ForceAllowDuplicates;
		prototypeDungeonRoom.doorTopDecorable = sourceRoom.doorTopDecorable;
		prototypeDungeonRoom.requiredInjectionData = sourceRoom.requiredInjectionData;
		prototypeDungeonRoom.injectionFlags = sourceRoom.injectionFlags;
		prototypeDungeonRoom.IsLostWoodsRoom = sourceRoom.IsLostWoodsRoom;
		prototypeDungeonRoom.UseCustomMusicState = sourceRoom.UseCustomMusicState;
		prototypeDungeonRoom.OverrideMusicState = sourceRoom.OverrideMusicState;
		prototypeDungeonRoom.UseCustomMusic = sourceRoom.UseCustomMusic;
		prototypeDungeonRoom.CustomMusicEvent = sourceRoom.CustomMusicEvent;
		prototypeDungeonRoom.RequiredCurseLevel = sourceRoom.RequiredCurseLevel;
		prototypeDungeonRoom.InvalidInCoop = sourceRoom.InvalidInCoop;
		prototypeDungeonRoom.overrideRoomVisualType = sourceRoom.overrideRoomVisualType;
		prototypeDungeonRoom.overrideRoomVisualTypeForSecretRooms = sourceRoom.overrideRoomVisualTypeForSecretRooms;
		prototypeDungeonRoom.rewardChestSpawnPosition = sourceRoom.rewardChestSpawnPosition;
		prototypeDungeonRoom.rewardChestSpawnPosition.x = intVector.x - (prototypeDungeonRoom.rewardChestSpawnPosition.x + 1);
		prototypeDungeonRoom.associatedMinimapIcon = sourceRoom.associatedMinimapIcon;
		prototypeDungeonRoom.overriddenTilesets = sourceRoom.overriddenTilesets;
		prototypeDungeonRoom.excludedOtherRooms = new List<PrototypeDungeonRoom>();
		for (int i = 0; i < sourceRoom.excludedOtherRooms.Count; i++)
		{
			prototypeDungeonRoom.excludedOtherRooms.Add(sourceRoom.excludedOtherRooms[i]);
		}
		prototypeDungeonRoom.prerequisites = new List<DungeonPrerequisite>();
		for (int j = 0; j < sourceRoom.prerequisites.Count; j++)
		{
			prototypeDungeonRoom.prerequisites.Add(sourceRoom.prerequisites[j]);
		}
		prototypeDungeonRoom.m_width = sourceRoom.m_width;
		prototypeDungeonRoom.m_height = sourceRoom.m_height;
		prototypeDungeonRoom.m_serializedCellType = new CellType[sourceRoom.m_serializedCellType.Length];
		prototypeDungeonRoom.m_serializedCellDataIndices = new List<int>();
		prototypeDungeonRoom.m_serializedCellDataData = new List<PrototypeDungeonRoomCellData>();
		for (int k = 0; k < sourceRoom.m_serializedCellType.Length; k++)
		{
			int num = k;
			int num2 = num % sourceRoom.m_width;
			int num3 = Mathf.FloorToInt(num / sourceRoom.m_width);
			int num4 = sourceRoom.m_width - (num2 + 1);
			int num5 = num3 * sourceRoom.m_width + num4;
			prototypeDungeonRoom.m_serializedCellType[num5] = sourceRoom.m_serializedCellType[k];
		}
		for (int l = 0; l < sourceRoom.m_serializedCellDataIndices.Count; l++)
		{
			int num6 = sourceRoom.m_serializedCellDataIndices[l];
			int num7 = num6 % sourceRoom.m_width;
			int num8 = Mathf.FloorToInt(num6 / sourceRoom.m_width);
			int num9 = sourceRoom.m_width - (num7 + 1);
			int item = num8 * sourceRoom.m_width + num9;
			prototypeDungeonRoom.m_serializedCellDataIndices.Add(item);
			PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = sourceRoom.m_serializedCellDataData[l];
			PrototypeDungeonRoomCellData prototypeDungeonRoomCellData2 = new PrototypeDungeonRoomCellData(prototypeDungeonRoomCellData.str, prototypeDungeonRoomCellData.state);
			prototypeDungeonRoomCellData2.MirrorData(prototypeDungeonRoomCellData);
			prototypeDungeonRoom.m_serializedCellDataData.Add(prototypeDungeonRoomCellData2);
		}
		List<int> list = new List<int>(prototypeDungeonRoom.m_serializedCellDataIndices);
		list.Sort();
		List<PrototypeDungeonRoomCellData> list2 = new List<PrototypeDungeonRoomCellData>(prototypeDungeonRoom.m_serializedCellDataData);
		for (int m = 0; m < list.Count; m++)
		{
			int item2 = list[m];
			int index = prototypeDungeonRoom.m_serializedCellDataIndices.IndexOf(item2);
			list2[m] = prototypeDungeonRoom.m_serializedCellDataData[index];
		}
		prototypeDungeonRoom.m_serializedCellDataIndices = list;
		prototypeDungeonRoom.m_serializedCellDataData = list2;
		prototypeDungeonRoom.exitData = new PrototypeRoomExitData();
		prototypeDungeonRoom.exitData.MirrorData(sourceRoom.exitData, intVector);
		prototypeDungeonRoom.pits = new List<PrototypeRoomPitEntry>();
		for (int n = 0; n < sourceRoom.pits.Count; n++)
		{
			prototypeDungeonRoom.pits.Add(sourceRoom.pits[n].CreateMirror(intVector));
		}
		prototypeDungeonRoom.eventTriggerAreas = new List<PrototypeEventTriggerArea>();
		for (int num10 = 0; num10 < sourceRoom.eventTriggerAreas.Count; num10++)
		{
			prototypeDungeonRoom.eventTriggerAreas.Add(sourceRoom.eventTriggerAreas[num10].CreateMirror(intVector));
		}
		prototypeDungeonRoom.roomEvents = new List<RoomEventDefinition>();
		for (int num11 = 0; num11 < sourceRoom.roomEvents.Count; num11++)
		{
			prototypeDungeonRoom.roomEvents.Add(new RoomEventDefinition(sourceRoom.roomEvents[num11].condition, sourceRoom.roomEvents[num11].action));
		}
		prototypeDungeonRoom.placedObjects = new List<PrototypePlacedObjectData>();
		for (int num12 = 0; num12 < sourceRoom.placedObjects.Count; num12++)
		{
			prototypeDungeonRoom.placedObjects.Add(sourceRoom.placedObjects[num12].CreateMirror(intVector));
		}
		prototypeDungeonRoom.placedObjectPositions = new List<Vector2>();
		for (int num13 = 0; num13 < sourceRoom.placedObjectPositions.Count; num13++)
		{
			Vector2 item3 = sourceRoom.placedObjectPositions[num13];
			item3.x = (float)intVector.x - (item3.x + (float)prototypeDungeonRoom.placedObjects[num13].GetWidth(true));
			prototypeDungeonRoom.placedObjectPositions.Add(item3);
		}
		prototypeDungeonRoom.additionalObjectLayers = new List<PrototypeRoomObjectLayer>();
		for (int num14 = 0; num14 < sourceRoom.additionalObjectLayers.Count; num14++)
		{
			prototypeDungeonRoom.additionalObjectLayers.Add(PrototypeRoomObjectLayer.CreateMirror(sourceRoom.additionalObjectLayers[num14], intVector));
		}
		prototypeDungeonRoom.rectangularFeatures = new List<PrototypeRectangularFeature>();
		for (int num15 = 0; num15 < sourceRoom.rectangularFeatures.Count; num15++)
		{
			prototypeDungeonRoom.rectangularFeatures.Add(PrototypeRectangularFeature.CreateMirror(sourceRoom.rectangularFeatures[num15], intVector));
		}
		prototypeDungeonRoom.paths = new List<SerializedPath>();
		for (int num16 = 0; num16 < sourceRoom.paths.Count; num16++)
		{
			prototypeDungeonRoom.paths.Add(SerializedPath.CreateMirror(sourceRoom.paths[num16], intVector, sourceRoom));
		}
		prototypeDungeonRoom.OnAfterDeserialize();
		prototypeDungeonRoom.UpdatePrecalculatedData();
		return prototypeDungeonRoom;
	}

	public void RemovePathAt(int id)
	{
		paths.RemoveAt(id);
		for (int i = 0; i < placedObjects.Count; i++)
		{
			if (placedObjects[i].assignedPathIDx == id)
			{
				placedObjects[i].assignedPathIDx = -1;
			}
			else if (placedObjects[i].assignedPathIDx > id)
			{
				placedObjects[i].assignedPathIDx = placedObjects[i].assignedPathIDx - 1;
			}
		}
		foreach (PrototypeRoomObjectLayer additionalObjectLayer in additionalObjectLayers)
		{
			for (int j = 0; j < additionalObjectLayer.placedObjects.Count; j++)
			{
				if (additionalObjectLayer.placedObjects[j].assignedPathIDx == id)
				{
					additionalObjectLayer.placedObjects[j].assignedPathIDx = -1;
				}
				else if (additionalObjectLayer.placedObjects[j].assignedPathIDx > id)
				{
					additionalObjectLayer.placedObjects[j].assignedPathIDx = additionalObjectLayer.placedObjects[j].assignedPathIDx - 1;
				}
			}
		}
	}

	public void OnBeforeSerialize()
	{
		m_serializedCellType = new CellType[m_cellData.Length];
		m_serializedCellDataIndices = new List<int>();
		m_serializedCellDataData = new List<PrototypeDungeonRoomCellData>();
		for (int i = 0; i < m_cellData.Length; i++)
		{
			PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = m_cellData[i];
			m_serializedCellType[i] = prototypeDungeonRoomCellData.state;
			if (prototypeDungeonRoomCellData.HasChanges())
			{
				m_serializedCellDataIndices.Add(i);
				m_serializedCellDataData.Add(prototypeDungeonRoomCellData);
			}
		}
	}

	public void OnAfterDeserialize()
	{
		if (m_OLDcellData != null && m_OLDcellData.Length > 0)
		{
			m_cellData = m_OLDcellData;
			m_OLDcellData = new PrototypeDungeonRoomCellData[0];
			return;
		}
		m_cellData = new PrototypeDungeonRoomCellData[m_serializedCellType.Length];
		int num = 0;
		for (int i = 0; i < m_serializedCellType.Length; i++)
		{
			if (num < m_serializedCellDataIndices.Count && m_serializedCellDataIndices[num] == i)
			{
				m_cellData[i] = m_serializedCellDataData[num++];
				continue;
			}
			PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = new PrototypeDungeonRoomCellData();
			prototypeDungeonRoomCellData.appearance = new PrototypeDungeonRoomCellAppearance();
			prototypeDungeonRoomCellData.state = m_serializedCellType[i];
			m_cellData[i] = prototypeDungeonRoomCellData;
		}
	}

	public bool CheckPrerequisites()
	{
		if (InvalidInCoop && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			return false;
		}
		if (RequiredCurseLevel > 0)
		{
			int totalCurse = PlayerStats.GetTotalCurse();
			if (totalCurse < RequiredCurseLevel)
			{
				return false;
			}
		}
		for (int i = 0; i < prerequisites.Count; i++)
		{
			if (!prerequisites[i].CheckConditionsFulfilled())
			{
				return false;
			}
		}
		return true;
	}

	public PrototypeEventTriggerArea AddEventTriggerArea(IEnumerable<IntVector2> cells)
	{
		PrototypeEventTriggerArea prototypeEventTriggerArea = new PrototypeEventTriggerArea(cells);
		eventTriggerAreas.Add(prototypeEventTriggerArea);
		return prototypeEventTriggerArea;
	}

	public List<PrototypeEventTriggerArea> GetEventTriggerAreasAtPosition(IntVector2 position)
	{
		List<PrototypeEventTriggerArea> list = null;
		foreach (PrototypeEventTriggerArea eventTriggerArea in eventTriggerAreas)
		{
			if (eventTriggerArea.triggerCells.Contains(position.ToVector2()))
			{
				if (list == null)
				{
					list = new List<PrototypeEventTriggerArea>();
				}
				list.Add(eventTriggerArea);
			}
		}
		return list;
	}

	public void RemoveEventTriggerArea(PrototypeEventTriggerArea peta)
	{
		int num = eventTriggerAreas.IndexOf(peta);
		if (num < 0)
		{
			return;
		}
		eventTriggerAreas.Remove(peta);
		foreach (PrototypePlacedObjectData placedObject in placedObjects)
		{
			for (int i = 0; i < placedObject.linkedTriggerAreaIDs.Count; i++)
			{
				if (placedObject.linkedTriggerAreaIDs[i] == num)
				{
					placedObject.linkedTriggerAreaIDs.RemoveAt(i);
					i--;
				}
				else if (placedObject.linkedTriggerAreaIDs[i] > num)
				{
					placedObject.linkedTriggerAreaIDs[i] = placedObject.linkedTriggerAreaIDs[i] - 1;
				}
			}
		}
		foreach (PrototypeRoomObjectLayer additionalObjectLayer in additionalObjectLayers)
		{
			foreach (PrototypePlacedObjectData placedObject2 in additionalObjectLayer.placedObjects)
			{
				for (int j = 0; j < placedObject2.linkedTriggerAreaIDs.Count; j++)
				{
					if (placedObject2.linkedTriggerAreaIDs[j] == num)
					{
						placedObject2.linkedTriggerAreaIDs.RemoveAt(j);
						j--;
					}
					else if (placedObject2.linkedTriggerAreaIDs[j] > num)
					{
						placedObject2.linkedTriggerAreaIDs[j] = placedObject2.linkedTriggerAreaIDs[j] - 1;
					}
				}
			}
		}
	}

	public bool DoesUnsealOnClear()
	{
		for (int i = 0; i < roomEvents.Count; i++)
		{
			if (roomEvents[i].condition == RoomEventTriggerCondition.ON_ENEMIES_CLEARED && roomEvents[i].action == RoomEventTriggerAction.UNSEAL_ROOM)
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsPit()
	{
		for (int i = 0; i < m_cellData.Length; i++)
		{
			if (m_cellData[i].state == CellType.PIT)
			{
				return true;
			}
		}
		return false;
	}

	public PrototypeRoomPitEntry GetPitEntryFromPosition(IntVector2 position)
	{
		Vector2 item = position.ToVector2();
		foreach (PrototypeRoomPitEntry pit in pits)
		{
			if (pit.containedCells.Contains(item))
			{
				return pit;
			}
		}
		return null;
	}

	public void RedefineAllPitEntries()
	{
		pits = new List<PrototypeRoomPitEntry>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (ForceGetCellDataAtPoint(i, j).state == CellType.PIT)
				{
					HandlePitCellsAddition(new IntVector2[1]
					{
						new IntVector2(i, j)
					});
				}
			}
		}
	}

	public void HandlePitCellsAddition(IEnumerable<IntVector2> cells)
	{
		if (pits == null)
		{
			pits = new List<PrototypeRoomPitEntry>();
		}
		List<Vector2> list = new List<Vector2>();
		foreach (IntVector2 cell in cells)
		{
			list.Add(cell.ToVector2());
		}
		for (int num = pits.Count - 1; num >= 0; num--)
		{
			if (pits[num].IsAdjoining(list))
			{
				list.AddRange(pits[num].containedCells);
				pits.RemoveAt(num);
			}
		}
		pits.Add(new PrototypeRoomPitEntry(list));
	}

	public void HandlePitCellsRemoval(IEnumerable<IntVector2> cells)
	{
		if (pits == null)
		{
			pits = new List<PrototypeRoomPitEntry>();
		}
		HashSet<Vector2> hashSet = new HashSet<Vector2>();
		foreach (PrototypeRoomPitEntry pit in pits)
		{
			foreach (Vector2 containedCell in pit.containedCells)
			{
				hashSet.Add(containedCell);
			}
		}
		pits.Clear();
		foreach (IntVector2 cell2 in cells)
		{
			hashSet.Remove(cell2.ToVector2());
		}
		List<Vector2> list = new List<Vector2>(hashSet);
		while (list.Count > 0)
		{
			Vector2 cell = list[0];
			list.RemoveAt(0);
			PrototypeRoomPitEntry prototypeRoomPitEntry = new PrototypeRoomPitEntry(cell);
			bool flag = true;
			while (flag)
			{
				flag = false;
				for (int num = list.Count - 1; num >= 0; num--)
				{
					if (prototypeRoomPitEntry.IsAdjoining(list[num]))
					{
						flag = true;
						prototypeRoomPitEntry.containedCells.Add(list[num]);
						list.RemoveAt(num);
					}
				}
			}
			pits.Add(prototypeRoomPitEntry);
		}
	}

	public void ClearAllObjectData()
	{
		for (int i = 0; i < m_cellData.Length; i++)
		{
			PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = m_cellData[i];
			prototypeDungeonRoomCellData.placedObjectRUBELIndex = -1;
			prototypeDungeonRoomCellData.additionalPlacedObjectIndices.Clear();
		}
	}

	public void DeleteRow(int yRow)
	{
		for (int i = 0; i < m_width; i++)
		{
			for (int j = yRow + 1; j < m_height; j++)
			{
				m_cellData[(j - 1) * m_width + i] = m_cellData[j * m_width + i];
			}
		}
		exitData.HandleRowColumnShift(-1, 0, yRow + 1, -1, this);
		Height--;
		TranslateAllObjectBasePositions(0, -1, 0, Width, yRow + 1, Height + 1);
	}

	public void DeleteColumn(int xCol)
	{
		for (int i = 0; i < m_height; i++)
		{
			for (int j = xCol + 1; j < m_width; j++)
			{
				m_cellData[i * m_width + (j - 1)] = m_cellData[i * m_width + j];
			}
		}
		Width--;
		exitData.HandleRowColumnShift(xCol + 1, -1, -1, 0, this);
		TranslateAllObjectBasePositions(-1, 0, xCol + 1, Width + 1, 0, Height);
	}

	public bool CheckRegionOccupied(int xPos, int yPos, int w, int h)
	{
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = ForceGetCellDataAtPoint(xPos + i, yPos + j);
				if (prototypeDungeonRoomCellData == null)
				{
					return true;
				}
				if (prototypeDungeonRoomCellData.IsOccupied)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CheckRegionOccupiedExcludeWallsAndPits(int xPos, int yPos, int w, int h, bool includeTopwalls = true)
	{
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = ForceGetCellDataAtPoint(xPos + i, yPos + j);
				if (prototypeDungeonRoomCellData == null)
				{
					return true;
				}
				if (prototypeDungeonRoomCellData.state != CellType.FLOOR)
				{
					return true;
				}
				if (prototypeDungeonRoomCellData.IsOccupied)
				{
					return true;
				}
				if (!includeTopwalls)
				{
					PrototypeDungeonRoomCellData prototypeDungeonRoomCellData2 = ForceGetCellDataAtPoint(xPos + i, yPos + j - 1);
					if (prototypeDungeonRoomCellData2 == null || prototypeDungeonRoomCellData2.state == CellType.WALL)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool CheckRegionOccupied(int xPos, int yPos, int w, int h, int objectLayerIndex)
	{
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				if (ForceGetCellDataAtPoint(xPos + i, yPos + j).IsOccupiedAtLayer(objectLayerIndex))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<IntVector2> GetCellRepresentation(IntVector2 worldBasePosition)
	{
		List<IntVector2> list = new List<IntVector2>();
		for (int i = 0; i < m_height; i++)
		{
			for (int j = 0; j < m_width; j++)
			{
				PrototypeDungeonRoomCellData cellDataAtPoint = GetCellDataAtPoint(j, i);
				if (cellDataAtPoint != null && (cellDataAtPoint.state == CellType.FLOOR || cellDataAtPoint.state == CellType.PIT || (cellDataAtPoint.state == CellType.WALL && cellDataAtPoint.breakable)))
				{
					IntVector2 item = worldBasePosition + new IntVector2(j, i);
					list.Add(item);
				}
			}
		}
		return list;
	}

	public void UpdatePrecalculatedData()
	{
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		for (int i = 0; i < m_height; i++)
		{
			for (int j = 0; j < m_width; j++)
			{
				PrototypeDungeonRoomCellData cellDataAtPoint = GetCellDataAtPoint(j, i);
				if (cellDataAtPoint != null && (cellDataAtPoint.state == CellType.FLOOR || cellDataAtPoint.state == CellType.PIT || (cellDataAtPoint.state == CellType.WALL && cellDataAtPoint.breakable)))
				{
					IntVector2 intVector = new IntVector2(j, i);
					hashSet.Add(intVector);
					hashSet.Add(intVector + IntVector2.Up);
					hashSet.Add(intVector + IntVector2.Up * 2);
					hashSet.Add(new IntVector2(intVector.x + 1, intVector.y));
					hashSet.Add(new IntVector2(intVector.x + 1, intVector.y + 1));
					hashSet.Add(new IntVector2(intVector.x + 1, intVector.y + 2));
					hashSet.Add(new IntVector2(intVector.x - 1, intVector.y));
					hashSet.Add(new IntVector2(intVector.x - 1, intVector.y + 1));
					hashSet.Add(new IntVector2(intVector.x - 1, intVector.y + 2));
					hashSet.Add(new IntVector2(intVector.x, intVector.y + 3));
					hashSet.Add(new IntVector2(intVector.x, intVector.y - 1));
					hashSet.Add(new IntVector2(intVector.x - 1, intVector.y - 1));
					hashSet.Add(new IntVector2(intVector.x + 1, intVector.y - 1));
					hashSet.Add(new IntVector2(intVector.x - 1, intVector.y + 3));
					hashSet.Add(new IntVector2(intVector.x + 1, intVector.y + 3));
				}
			}
		}
		UnityEngine.Random.InitState(base.name.GetHashCode());
		List<IntVector2> list = (m_cachedRepresentationIncFacewalls = new List<IntVector2>(hashSet).Shuffle());
	}

	public List<IntVector2> GetCellRepresentationIncFacewalls()
	{
		if (m_cachedRepresentationIncFacewalls != null && m_cachedRepresentationIncFacewalls.Count > 0)
		{
			return m_cachedRepresentationIncFacewalls;
		}
		Debug.LogError("PROTOTYPE DUNGEON ROOM: " + base.name + " IS MISSING PRECALCULATED DATA.");
		return null;
	}

	public PrototypeDungeonRoomCellData GetCellDataAtPoint(int ix, int iy)
	{
		return ForceGetCellDataAtPoint(ix, iy);
	}

	public PrototypeDungeonRoomCellData ForceGetCellDataAtPoint(int ix, int iy)
	{
		if (m_cellData == null || m_cellData.Length != m_width * m_height)
		{
			InitializeArray(m_width, m_height);
		}
		if (iy < 0 || ix < 0 || ix >= m_width || iy >= m_height)
		{
			return null;
		}
		if (iy * m_width + ix < 0 || iy * m_width + ix >= m_cellData.Length)
		{
			return null;
		}
		return m_cellData[iy * m_width + ix];
	}

	public PrototypeRoomExit GetExitDataAtPoint(int ix, int iy)
	{
		return exitData[ix, iy];
	}

	private bool IsValidCellDataPosition(int ix, int iy)
	{
		return iy >= 0 && iy < m_height && ix >= 0 && ix < m_width;
	}

	public bool ProcessExitPosition(int ix, int iy)
	{
		return exitData.ProcessExitPosition(ix, iy, this);
	}

	public bool HasFloorNeighbor(int ix, int iy)
	{
		if (m_cellData == null || m_cellData.Length != m_width * m_height)
		{
			InitializeArray(m_width, m_height);
		}
		if (ix == -1 || iy == -1 || ix == m_width || iy == m_height)
		{
			return BoundaryHasFloorNeighbor(ix, iy);
		}
		if (iy < m_height - 1 && IsValidCellDataPosition(ix, iy + 1) && m_cellData[(iy + 1) * m_width + ix].state == CellType.FLOOR)
		{
			return true;
		}
		if (iy > 0 && IsValidCellDataPosition(ix, iy - 1) && m_cellData[(iy - 1) * m_width + ix].state == CellType.FLOOR)
		{
			return true;
		}
		if (ix < m_width - 1 && IsValidCellDataPosition(ix + 1, iy) && m_cellData[iy * m_width + (ix + 1)].state == CellType.FLOOR)
		{
			return true;
		}
		if (ix > 0 && IsValidCellDataPosition(ix - 1, iy) && m_cellData[iy * m_width + (ix - 1)].state == CellType.FLOOR)
		{
			return true;
		}
		return false;
	}

	public bool HasBreakableNeighbor(int ix, int iy)
	{
		if (m_cellData == null || m_cellData.Length != m_width * m_height)
		{
			InitializeArray(m_width, m_height);
		}
		if (iy < m_height - 1 && IsValidCellDataPosition(ix, iy + 1) && m_cellData[(iy + 1) * m_width + ix].breakable)
		{
			return true;
		}
		if (iy > 0 && IsValidCellDataPosition(ix, iy - 1) && m_cellData[(iy - 1) * m_width + ix].breakable)
		{
			return true;
		}
		if (ix < m_width - 1 && IsValidCellDataPosition(ix + 1, iy) && m_cellData[iy * m_width + (ix + 1)].breakable)
		{
			return true;
		}
		if (ix > 0 && IsValidCellDataPosition(ix - 1, iy) && m_cellData[iy * m_width + (ix - 1)].breakable)
		{
			return true;
		}
		return false;
	}

	public bool HasNonWallNeighbor(int ix, int iy)
	{
		if (m_cellData == null || m_cellData.Length != m_width * m_height)
		{
			InitializeArray(m_width, m_height);
		}
		if (IsValidCellDataPosition(ix, iy + 1) && m_cellData[(iy + 1) * m_width + ix].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix, iy - 1) && m_cellData[(iy - 1) * m_width + ix].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix + 1, iy) && m_cellData[iy * m_width + (ix + 1)].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix - 1, iy) && m_cellData[iy * m_width + (ix - 1)].state != CellType.WALL)
		{
			return true;
		}
		return false;
	}

	public bool HasNonWallNeighborWithDiagonals(int ix, int iy)
	{
		if (m_cellData == null || m_cellData.Length != m_width * m_height)
		{
			InitializeArray(m_width, m_height);
		}
		if (IsValidCellDataPosition(ix, iy + 1) && m_cellData[(iy + 1) * m_width + ix].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix, iy - 1) && m_cellData[(iy - 1) * m_width + ix].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix + 1, iy) && m_cellData[iy * m_width + (ix + 1)].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix - 1, iy) && m_cellData[iy * m_width + (ix - 1)].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix + 1, iy + 1) && m_cellData[(iy + 1) * m_width + (ix + 1)].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix + 1, iy - 1) && m_cellData[(iy - 1) * m_width + (ix + 1)].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix - 1, iy + 1) && m_cellData[(iy + 1) * m_width + (ix - 1)].state != CellType.WALL)
		{
			return true;
		}
		if (IsValidCellDataPosition(ix - 1, iy - 1) && m_cellData[(iy - 1) * m_width + (ix - 1)].state != CellType.WALL)
		{
			return true;
		}
		return false;
	}

	private bool BoundaryHasFloorNeighbor(int ix, int iy)
	{
		if (ix == -1 && IsValidCellDataPosition(ix + 1, iy) && m_cellData[iy * m_width + (ix + 1)].state == CellType.FLOOR)
		{
			return true;
		}
		if (ix == m_width && IsValidCellDataPosition(ix - 1, iy) && m_cellData[iy * m_width + (ix - 1)].state == CellType.FLOOR)
		{
			return true;
		}
		if (iy == -1 && IsValidCellDataPosition(ix, iy + 1) && m_cellData[(iy + 1) * m_width + ix].state == CellType.FLOOR)
		{
			return true;
		}
		if (iy == m_height && IsValidCellDataPosition(ix, iy - 1) && m_cellData[(iy - 1) * m_width + ix].state == CellType.FLOOR)
		{
			return true;
		}
		return false;
	}

	public DungeonData.Direction GetFloorDirection(int ix, int iy)
	{
		if (iy < m_height - 1 && IsValidCellDataPosition(ix, iy + 1) && m_cellData[(iy + 1) * m_width + ix].state == CellType.FLOOR)
		{
			return DungeonData.Direction.NORTH;
		}
		if (iy > 0 && IsValidCellDataPosition(ix, iy - 1) && m_cellData[(iy - 1) * m_width + ix].state == CellType.FLOOR)
		{
			return DungeonData.Direction.SOUTH;
		}
		if (ix < m_width - 1 && IsValidCellDataPosition(ix + 1, iy) && m_cellData[iy * m_width + (ix + 1)].state == CellType.FLOOR)
		{
			return DungeonData.Direction.EAST;
		}
		if (ix > 0 && IsValidCellDataPosition(ix - 1, iy) && m_cellData[iy * m_width + (ix - 1)].state == CellType.FLOOR)
		{
			return DungeonData.Direction.WEST;
		}
		return DungeonData.Direction.SOUTHWEST;
	}

	private void InitializeArray(int w, int h)
	{
		m_cellData = new PrototypeDungeonRoomCellData[w * h];
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				m_cellData[j * w + i] = new PrototypeDungeonRoomCellData(string.Empty, CellType.FLOOR);
			}
		}
	}

	public void TranslateAndResize(int newWidth, int newHeight, int xTrans, int yTrans)
	{
		RecalculateCellDataArray(newWidth, newHeight, xTrans, yTrans);
		int endX = Math.Max(m_width, newWidth);
		int endY = Math.Max(m_height, newHeight);
		m_width = newWidth;
		m_height = newHeight;
		exitData.TranslateAllExits(xTrans, yTrans, this);
		TranslateAllObjectBasePositions(xTrans, yTrans, 0, endX, 0, endY);
	}

	private void TranslateAllObjectBasePositions(int deltaX, int deltaY, int startX, int endX, int startY, int endY)
	{
		foreach (PrototypePlacedObjectData placedObject in placedObjects)
		{
			if (placedObject.contentsBasePosition.x >= (float)startX && placedObject.contentsBasePosition.x < (float)endX && placedObject.contentsBasePosition.y >= (float)startY && placedObject.contentsBasePosition.y < (float)endY)
			{
				placedObject.contentsBasePosition += new Vector2(deltaX, deltaY);
			}
		}
		for (int i = 0; i < placedObjectPositions.Count; i++)
		{
			if (placedObjectPositions[i].x >= (float)startX && placedObjectPositions[i].x < (float)endX && placedObjectPositions[i].y >= (float)startY && placedObjectPositions[i].y < (float)endY)
			{
				placedObjectPositions[i] += new Vector2(deltaX, deltaY);
			}
		}
		foreach (PrototypeRoomObjectLayer additionalObjectLayer in additionalObjectLayers)
		{
			foreach (PrototypePlacedObjectData placedObject2 in additionalObjectLayer.placedObjects)
			{
				if (placedObject2.contentsBasePosition.x >= (float)startX && placedObject2.contentsBasePosition.x < (float)endX && placedObject2.contentsBasePosition.y >= (float)startY && placedObject2.contentsBasePosition.y < (float)endY)
				{
					placedObject2.contentsBasePosition += new Vector2(deltaX, deltaY);
				}
			}
			for (int j = 0; j < additionalObjectLayer.placedObjectBasePositions.Count; j++)
			{
				if (additionalObjectLayer.placedObjectBasePositions[j].x >= (float)startX && additionalObjectLayer.placedObjectBasePositions[j].x < (float)endX && additionalObjectLayer.placedObjectBasePositions[j].y >= (float)startY && additionalObjectLayer.placedObjectBasePositions[j].y < (float)endY)
				{
					additionalObjectLayer.placedObjectBasePositions[j] = additionalObjectLayer.placedObjectBasePositions[j] + new Vector2(deltaX, deltaY);
				}
			}
		}
		ClearAndRebuildObjectCellData();
	}

	public List<PrototypeRoomExit> GetExitsMatchingDirection(DungeonData.Direction dir, PrototypeRoomExit.ExitType exitType)
	{
		List<PrototypeRoomExit> list = new List<PrototypeRoomExit>();
		for (int i = 0; i < exitData.exits.Count; i++)
		{
			if (exitData.exits[i].exitDirection != dir)
			{
				continue;
			}
			switch (exitType)
			{
			case PrototypeRoomExit.ExitType.NO_RESTRICTION:
				list.Add(exitData.exits[i]);
				continue;
			case PrototypeRoomExit.ExitType.EXIT_ONLY:
				if (exitData.exits[i].exitType != PrototypeRoomExit.ExitType.ENTRANCE_ONLY)
				{
					list.Add(exitData.exits[i]);
					continue;
				}
				break;
			}
			if (exitType == PrototypeRoomExit.ExitType.ENTRANCE_ONLY && exitData.exits[i].exitType != PrototypeRoomExit.ExitType.EXIT_ONLY)
			{
				list.Add(exitData.exits[i]);
			}
		}
		return list;
	}

	public List<Tuple<PrototypeRoomExit, PrototypeRoomExit>> GetExitPairsMatchingDirections(DungeonData.Direction dir1, DungeonData.Direction dir2)
	{
		List<Tuple<PrototypeRoomExit, PrototypeRoomExit>> list = new List<Tuple<PrototypeRoomExit, PrototypeRoomExit>>();
		for (int i = 0; i < exitData.exits.Count; i++)
		{
			PrototypeRoomExit prototypeRoomExit = exitData.exits[i];
			for (int j = 0; j < exitData.exits.Count; j++)
			{
				PrototypeRoomExit prototypeRoomExit2 = exitData.exits[j];
				if (prototypeRoomExit.exitDirection == dir1 && prototypeRoomExit2.exitDirection == dir2)
				{
					list.Add(new Tuple<PrototypeRoomExit, PrototypeRoomExit>(prototypeRoomExit, prototypeRoomExit2));
				}
			}
		}
		return list;
	}

	public void ClearAndRebuildObjectCellData()
	{
		if (m_cellData != null)
		{
			PrototypeDungeonRoomCellData[] cellData = m_cellData;
			foreach (PrototypeDungeonRoomCellData prototypeDungeonRoomCellData in cellData)
			{
				prototypeDungeonRoomCellData.placedObjectRUBELIndex = -1;
				prototypeDungeonRoomCellData.additionalPlacedObjectIndices.Clear();
			}
			RebuildObjectCellData();
		}
	}

	public void RebuildObjectCellData()
	{
		foreach (PrototypePlacedObjectData placedObject in placedObjects)
		{
			Vector2 contentsBasePosition = placedObject.contentsBasePosition;
			int num = 0;
			while (true)
			{
				if (num < Height)
				{
					for (int i = 0; i < Width; i++)
					{
						Vector2 vector = new Vector2(i, num);
						PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = ForceGetCellDataAtPoint(i, num);
						if (prototypeDungeonRoomCellData != null && prototypeDungeonRoomCellData.placedObjectRUBELIndex >= 0 && placedObjects[prototypeDungeonRoomCellData.placedObjectRUBELIndex] == placedObject)
						{
							if (!(contentsBasePosition != vector))
							{
								goto end_IL_00a8;
							}
							prototypeDungeonRoomCellData.placedObjectRUBELIndex = -1;
						}
					}
					num++;
					continue;
				}
				if (placedObject == null)
				{
					Debug.LogError("null object data at placed object index!");
					break;
				}
				for (int j = (int)contentsBasePosition.x; (float)j < contentsBasePosition.x + (float)placedObject.GetWidth(); j++)
				{
					for (int k = (int)contentsBasePosition.y; (float)k < contentsBasePosition.y + (float)placedObject.GetHeight(); k++)
					{
						if (k * Width + j >= 0 && k * Width + j < m_cellData.Length)
						{
							PrototypeDungeonRoomCellData prototypeDungeonRoomCellData2 = ForceGetCellDataAtPoint(j, k);
							if (prototypeDungeonRoomCellData2 != null)
							{
								prototypeDungeonRoomCellData2.placedObjectRUBELIndex = placedObjects.IndexOf(placedObject);
							}
						}
					}
				}
				break;
				continue;
				end_IL_00a8:
				break;
			}
		}
		foreach (PrototypeRoomObjectLayer additionalObjectLayer in additionalObjectLayers)
		{
			int num2 = additionalObjectLayers.IndexOf(additionalObjectLayer);
			for (int l = 0; l < additionalObjectLayer.placedObjects.Count; l++)
			{
				PrototypePlacedObjectData prototypePlacedObjectData = additionalObjectLayer.placedObjects[l];
				Vector2 contentsBasePosition2 = prototypePlacedObjectData.contentsBasePosition;
				int num3 = 0;
				while (true)
				{
					if (num3 < Height)
					{
						for (int m = 0; m < Width; m++)
						{
							Vector2 vector2 = new Vector2(m, num3);
							PrototypeDungeonRoomCellData prototypeDungeonRoomCellData3 = ForceGetCellDataAtPoint(m, num3);
							if (prototypeDungeonRoomCellData3 == null)
							{
								continue;
							}
							int num4 = ((prototypeDungeonRoomCellData3.additionalPlacedObjectIndices.Count <= num2) ? (-1) : prototypeDungeonRoomCellData3.additionalPlacedObjectIndices[num2]);
							if (num4 >= 0 && additionalObjectLayer.placedObjects[num4] == prototypePlacedObjectData)
							{
								if (!(contentsBasePosition2 != vector2))
								{
									goto end_IL_02a3;
								}
								prototypeDungeonRoomCellData3.additionalPlacedObjectIndices[num2] = -1;
							}
						}
						num3++;
						continue;
					}
					if (prototypePlacedObjectData == null)
					{
						Debug.LogError("null object data at placed object index in layer: " + additionalObjectLayers.IndexOf(additionalObjectLayer));
						break;
					}
					for (int n = (int)contentsBasePosition2.x; (float)n < contentsBasePosition2.x + (float)prototypePlacedObjectData.GetWidth(); n++)
					{
						for (int num5 = (int)contentsBasePosition2.y; (float)num5 < contentsBasePosition2.y + (float)prototypePlacedObjectData.GetHeight(); num5++)
						{
							if (num5 * Width + n < 0 || num5 * Width + n >= m_cellData.Length)
							{
								continue;
							}
							PrototypeDungeonRoomCellData prototypeDungeonRoomCellData4 = ForceGetCellDataAtPoint(n, num5);
							if (prototypeDungeonRoomCellData4 == null)
							{
								continue;
							}
							if (prototypeDungeonRoomCellData4.additionalPlacedObjectIndices.Count <= num2)
							{
								while (prototypeDungeonRoomCellData4.additionalPlacedObjectIndices.Count <= num2)
								{
									prototypeDungeonRoomCellData4.additionalPlacedObjectIndices.Add(-1);
								}
							}
							prototypeDungeonRoomCellData4.additionalPlacedObjectIndices[num2] = additionalObjectLayer.placedObjects.IndexOf(prototypePlacedObjectData);
						}
					}
					break;
					continue;
					end_IL_02a3:
					break;
				}
			}
		}
	}

	private void RecalculateCellDataArray(int newWidth, int newHeight, int xTrans = 0, int yTrans = 0)
	{
		if (m_cellData == null)
		{
			InitializeArray(newWidth, newHeight);
			return;
		}
		PrototypeDungeonRoomCellData[] array = new PrototypeDungeonRoomCellData[newWidth * newHeight];
		for (int i = 0; i < m_width; i++)
		{
			for (int j = 0; j < m_height; j++)
			{
				if (i < newWidth && j < newHeight)
				{
					int num = i + xTrans;
					int num2 = j + yTrans;
					if (num >= 0 && num < newWidth && num2 >= 0 && num2 < newHeight)
					{
						array[num2 * newWidth + num] = ForceGetCellDataAtPoint(i, j);
					}
				}
			}
		}
		for (int k = 0; k < newWidth; k++)
		{
			for (int l = 0; l < newHeight; l++)
			{
				if (array[l * newWidth + k] == null)
				{
					array[l * newWidth + k] = new PrototypeDungeonRoomCellData(string.Empty, CellType.WALL);
				}
			}
		}
		m_cellData = array;
	}
}
