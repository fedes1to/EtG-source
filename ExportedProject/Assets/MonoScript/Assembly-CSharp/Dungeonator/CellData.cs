using System;
using System.Collections.Generic;

namespace Dungeonator
{
	public class CellData
	{
		public IntVector2 position;

		public CellType type;

		public DiagonalWallType diagonalWallType;

		public IntVector2 positionInTilemap;

		public bool breakable;

		public bool fallingPrevented;

		public List<SpeculativeRigidbody> platforms;

		public bool containsTrap;

		public bool forceAllowGoop;

		public bool forceDisallowGoop;

		public bool isWallMimicHideout;

		public bool doesDamage;

		public CellDamageDefinition damageDefinition;

		public bool isExitCell;

		public DungeonData.Direction exitDirection;

		public bool isDoorFrameCell;

		public bool isExitNonOccluder;

		public DungeonDoorController exitDoor;

		public RoomHandler connectedRoom1;

		public RoomHandler connectedRoom2;

		public bool isSecretRoomCell;

		public RoomHandler nearestRoom;

		public float distanceFromNearestRoom = float.MaxValue;

		public CellVisualData cellVisualData;

		public bool isOccupied;

		public bool isOccludedByTopWall;

		private bool? m_isNextToWall;

		public CellOcclusionData occlusionData;

		public CellArea parentArea;

		public RoomHandler parentRoom;

		[NonSerialized]
		public RoomHandler targetPitfallRoom;

		public bool hasBeenGenerated;

		public bool isRoomInternal = true;

		public bool isGridConnected;

		public float lastSplashTime = -1f;

		public bool IsFireplaceCell;

		public bool PreventRewardSpawn;

		public bool IsTrapZone;

		public bool IsPlayerInaccessible;

		public Action<CellData> OnCellGooped;

		public bool HasCachedPhysicsTile;

		public PhysicsEngine.Tile CachedPhysicsTile;

		[NonSerialized]
		private bool m_hasCachedUpperFacewallness;

		[NonSerialized]
		private bool m_upperFacewallness;

		[NonSerialized]
		private bool m_hasCachedLowerFacewallness;

		[NonSerialized]
		private bool m_lowerFacewallness;

		private bool? m_cachedFacewallness;

		public bool isNextToWall
		{
			get
			{
				if (!m_isNextToWall.HasValue)
				{
					m_isNextToWall = HasWallNeighbor(true, false);
				}
				return m_isNextToWall.Value;
			}
		}

		public bool cachedCanContainTeleporter { get; set; }

		public bool IsPassable
		{
			get
			{
				return !isOccupied && type == CellType.FLOOR;
			}
		}

		public float UniqueHash
		{
			get
			{
				int num = 0;
				num += position.x;
				num += num << 10;
				num ^= num >> 6;
				num += position.y;
				num += num << 10;
				num ^= num >> 6;
				num += num << 3;
				num ^= num >> 11;
				num += num << 15;
				uint num2 = (uint)num;
				return (float)num2 * 1f / 4.2949673E+09f;
			}
		}

		public CellData(int x, int y, CellType t = CellType.WALL)
		{
			position = new IntVector2(x, y);
			positionInTilemap = new IntVector2(x, y);
			type = t;
			cellVisualData = default(CellVisualData);
			cellVisualData.distanceToNearestLight = 100;
			cellVisualData.faceWallOverrideIndex = -1;
			cellVisualData.pitOverrideIndex = -1;
			cellVisualData.inheritedOverrideIndex = -1;
			cellVisualData.pathTilesetGridIndex = -1;
			cellVisualData.forcedMatchingStyle = DungeonTileStampData.IntermediaryMatchingStyle.ANY;
			cellVisualData.RatChunkBorderIndex = -1;
			occlusionData = new CellOcclusionData(this);
		}

		public CellData(IntVector2 p, CellType t = CellType.WALL)
		{
			position = p;
			positionInTilemap = p;
			type = t;
			cellVisualData = default(CellVisualData);
			cellVisualData.distanceToNearestLight = 100;
			cellVisualData.faceWallOverrideIndex = -1;
			cellVisualData.pitOverrideIndex = -1;
			cellVisualData.inheritedOverrideIndex = -1;
			cellVisualData.pathTilesetGridIndex = -1;
			cellVisualData.forcedMatchingStyle = DungeonTileStampData.IntermediaryMatchingStyle.ANY;
			cellVisualData.RatChunkBorderIndex = -1;
			occlusionData = new CellOcclusionData(this);
		}

		public bool HasPhantomCarpetNeighbor(bool includeDiagonals = true)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			List<CellData> cellNeighbors = data.GetCellNeighbors(this, includeDiagonals);
			for (int i = 0; i < cellNeighbors.Count; i++)
			{
				if (cellNeighbors[i] != null && cellNeighbors[i].cellVisualData.IsPhantomCarpet)
				{
					return true;
				}
			}
			return false;
		}

		public bool SurroundedByPits(bool includeDiagonals = true)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			List<CellData> cellNeighbors = data.GetCellNeighbors(this, includeDiagonals);
			for (int i = 0; i < cellNeighbors.Count; i++)
			{
				if (cellNeighbors[i] != null && cellNeighbors[i].type != CellType.PIT)
				{
					return false;
				}
			}
			return true;
		}

		public bool HasFloorNeighbor(DungeonData d, bool includeTopwalls = false, bool includeDiagonals = false)
		{
			List<CellData> cellNeighbors = d.GetCellNeighbors(this, includeDiagonals);
			for (int i = 0; i < cellNeighbors.Count; i++)
			{
				if (cellNeighbors[i] != null && cellNeighbors[i].type == CellType.FLOOR && (includeTopwalls || !cellNeighbors[i].IsTopWall()))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasWallNeighbor(bool includeDiagonals = true, bool includeTopwalls = true)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			List<CellData> cellNeighbors = data.GetCellNeighbors(this, includeDiagonals);
			for (int i = 0; i < cellNeighbors.Count; i++)
			{
				if (cellNeighbors[i] == null)
				{
					continue;
				}
				if (!includeTopwalls)
				{
					if (includeDiagonals)
					{
						if (i >= 3 && i <= 5)
						{
							continue;
						}
					}
					else if (i == 2)
					{
						continue;
					}
				}
				if (cellNeighbors[i].type != CellType.WALL)
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public bool HasPitNeighbor(DungeonData d)
		{
			List<CellData> cellNeighbors = d.GetCellNeighbors(this, true);
			for (int i = 0; i < cellNeighbors.Count; i++)
			{
				if (cellNeighbors[i] != null && cellNeighbors[i].type == CellType.PIT)
				{
					return true;
				}
			}
			return false;
		}

		public PrototypeRoomPitEntry.PitBorderType GetPitBorderType(DungeonData d)
		{
			if (parentArea != null && parentArea.IsProceduralRoom)
			{
				return PrototypeRoomPitEntry.PitBorderType.FLAT;
			}
			if (type == CellType.PIT)
			{
				PrototypeRoomPitEntry prototypeRoomPitEntry = null;
				if (parentArea != null && !parentArea.IsProceduralRoom)
				{
					prototypeRoomPitEntry = parentArea.prototypeRoom.GetPitEntryFromPosition(position - parentArea.basePosition + IntVector2.One);
				}
				if (prototypeRoomPitEntry != null)
				{
					return prototypeRoomPitEntry.borderType;
				}
			}
			else if (type != CellType.WALL || breakable)
			{
				foreach (CellData cellNeighbor in d.GetCellNeighbors(this, true))
				{
					if (parentArea != null && cellNeighbor != null && cellNeighbor.parentArea != null && !(cellNeighbor.parentArea.prototypeRoom == null) && cellNeighbor.type == CellType.PIT)
					{
						PrototypeRoomPitEntry pitEntryFromPosition = cellNeighbor.parentArea.prototypeRoom.GetPitEntryFromPosition(cellNeighbor.position - parentArea.basePosition + IntVector2.One);
						if (pitEntryFromPosition != null)
						{
							return pitEntryFromPosition.borderType;
						}
					}
				}
			}
			return PrototypeRoomPitEntry.PitBorderType.NONE;
		}

		public bool IsSideWallAdjacent()
		{
			if (type != CellType.WALL && (GameManager.Instance.Dungeon.data[position + IntVector2.Right].type == CellType.WALL || GameManager.Instance.Dungeon.data[position + IntVector2.Left].type == CellType.WALL))
			{
				return true;
			}
			return false;
		}

		public bool IsLowerFaceWall()
		{
			if (!Dungeon.IsGenerating)
			{
				if (!m_hasCachedLowerFacewallness)
				{
					m_lowerFacewallness = GameManager.Instance.Dungeon.data.isFaceWallLower(position.x, position.y);
					m_hasCachedLowerFacewallness = true;
				}
				return m_lowerFacewallness;
			}
			return GameManager.Instance.Dungeon.data.isFaceWallLower(position.x, position.y);
		}

		public bool IsUpperFacewall()
		{
			if (!Dungeon.IsGenerating)
			{
				if (!m_hasCachedUpperFacewallness)
				{
					m_upperFacewallness = GameManager.Instance.Dungeon.data.isFaceWallHigher(position.x, position.y);
					m_hasCachedUpperFacewallness = true;
				}
				return m_upperFacewallness;
			}
			return GameManager.Instance.Dungeon.data.isFaceWallHigher(position.x, position.y);
		}

		public bool IsAnyFaceWall()
		{
			if (m_cachedFacewallness.HasValue)
			{
				return m_cachedFacewallness.Value;
			}
			bool flag = GameManager.Instance.Dungeon.data.isAnyFaceWall(position.x, position.y);
			if (!Dungeon.IsGenerating)
			{
				m_cachedFacewallness = flag;
			}
			return flag;
		}

		public bool IsTopWall()
		{
			return GameManager.Instance.Dungeon.data.isTopWall(position.x, position.y);
		}

		public bool HasPassableNeighbor(DungeonData d)
		{
			foreach (CellData cellNeighbor in d.GetCellNeighbors(this))
			{
				if (cellNeighbor == null || !cellNeighbor.IsPassable)
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public CellData GetExitNeighbor()
		{
			foreach (CellData cellNeighbor in GameManager.Instance.Dungeon.data.GetCellNeighbors(this))
			{
				if (cellNeighbor == null || !cellNeighbor.isExitCell)
				{
					continue;
				}
				return cellNeighbor;
			}
			return null;
		}

		public bool HasNonTopWallWallNeighbor()
		{
			List<CellData> cellNeighbors = GameManager.Instance.Dungeon.data.GetCellNeighbors(this, true);
			for (int i = 0; i < cellNeighbors.Count; i++)
			{
				if ((i < 3 || i > 5) && cellNeighbors[i].type == CellType.WALL)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasTypeNeighbor(DungeonData d, CellType t)
		{
			foreach (CellData cellNeighbor in d.GetCellNeighbors(this))
			{
				if (cellNeighbor == null || cellNeighbor.type != t)
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public bool HasFaceWallNeighbor(DungeonData d)
		{
			foreach (CellData cellNeighbor in d.GetCellNeighbors(this))
			{
				if (cellNeighbor == null || !d.isAnyFaceWall(cellNeighbor.position.x, cellNeighbor.position.y))
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public bool HasMossyNeighbor(DungeonData d)
		{
			if (type == CellType.WALL)
			{
				return false;
			}
			foreach (CellData cellNeighbor in d.GetCellNeighbors(this))
			{
				if (cellNeighbor == null || (cellNeighbor.type != CellType.WALL && !cellNeighbor.cellVisualData.isDecal))
				{
					continue;
				}
				return true;
			}
			return false;
		}

		public bool HasPatternNeighbor(DungeonData d)
		{
			if (type == CellType.WALL)
			{
				return false;
			}
			foreach (CellData cellNeighbor in d.GetCellNeighbors(this))
			{
				if (cellNeighbor == null || (cellNeighbor.type != CellType.WALL && !cellNeighbor.cellVisualData.isPattern))
				{
					continue;
				}
				return true;
			}
			return false;
		}
	}
}
